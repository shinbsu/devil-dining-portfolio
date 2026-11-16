using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CookingUIController : MonoBehaviour
{
    [Header("Ingredient Selection")]
    [SerializeField] private Transform ingredientButtonsContainer;
    [SerializeField] private GameObject ingredientButtonPrefab;
    [SerializeField] private int maxIngredientsAllowed = 5;

    [Header("Selected Ingredients")]
    [SerializeField] private Transform selectedIngredientsContainer;
    [SerializeField] private Text selectedIngredientsText;
    [SerializeField] private Button clearButton;

    [Header("Cooking Process")]
    [SerializeField] private Button cookButton;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Image progressFill;
    [SerializeField] private Color startColor = Color.yellow;
    [SerializeField] private Color endColor = Color.green;

    [Header("Result Display")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Image resultImage;
    [SerializeField] private Text resultNameText;
    [SerializeField] private Text resultDescriptionText;
    [SerializeField] private Button closeResultButton;

    [Header("Audio")]
    [SerializeField] private AudioClip ingredientSelectSound;
    [SerializeField] private AudioClip cookingStartSound;
    [SerializeField] private AudioClip cookingFinishSound;

    private List<Ingredient> selectedIngredients = new List<Ingredient>();
    private List<IngredientButton> ingredientButtons = new List<IngredientButton>();
    private List<Image> selectedIngredientImages = new List<Image>();
    private bool isCooking = false;
    private AudioSource audioSource;

    public delegate void OnCookingStarted(List<Ingredient> ingredients);
    public static event OnCookingStarted CookingStarted;

    public delegate void OnCookingFinished(Recipe recipe);
    public static event OnCookingFinished CookingFinished;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        cookButton.onClick.AddListener(StartCooking);
        clearButton.onClick.AddListener(ClearIngredients);
        closeResultButton.onClick.AddListener(() => resultPanel.SetActive(false));

        progressBar.gameObject.SetActive(false);
        resultPanel.SetActive(false);

        InitializeSelectedIngredientsUI();
    }

    private void Start()
    {
        CreateIngredientButtons();
    }

    private void InitializeSelectedIngredientsUI()
    {
        while (selectedIngredientImages.Count < maxIngredientsAllowed)
        {
            GameObject newImageObj = new GameObject("SelectedIngredient");
            newImageObj.transform.SetParent(selectedIngredientsContainer);
            newImageObj.transform.localScale = Vector3.one;

            Image newImage = newImageObj.AddComponent<Image>();
            LayoutElement layoutElement = newImageObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 50;

            newImageObj.SetActive(false);

            selectedIngredientImages.Add(newImage);
        }
    }

    private void CreateIngredientButtons()
    {
        foreach (Transform child in ingredientButtonsContainer)
        {
            Destroy(child.gameObject);
        }
        ingredientButtons.Clear();

        List<Ingredient> availableIngredients = GetAvailableIngredients();
        foreach (Ingredient ingredient in availableIngredients)
        {
            GameObject buttonObj = Instantiate(ingredientButtonPrefab, ingredientButtonsContainer);
            IngredientButton button = buttonObj.GetComponent<IngredientButton>();

            if (button != null)
            {
                button.SetupIngredient(ingredient, AddIngredient);
                ingredientButtons.Add(button);
            }
        }
    }

    private List<Ingredient> GetAvailableIngredients()
    {
        if (CookingManager.Instance != null)
        {
            return CookingManager.Instance.GetAvailableIngredients();
        }

        // CookingManager가 없을 경우 기본 재료 세트 반환
        return new List<Ingredient>();
    }

    public void AddIngredient(Ingredient ingredient)
    {
        if (isCooking || selectedIngredients.Count >= maxIngredientsAllowed)
            return;

        selectedIngredients.Add(ingredient);

        if (ingredientSelectSound != null)
            audioSource.PlayOneShot(ingredientSelectSound);

        UpdateUI();
    }

    public void ClearIngredients()
    {
        if (isCooking)
            return;

        selectedIngredients.Clear();
        UpdateUI();
    }

    private void UpdateUI()
    {
        UpdateSelectedIngredientsUI();
        cookButton.interactable = selectedIngredients.Count > 0 && !isCooking;
        UpdateSelectedIngredientsText();
    }

    private void UpdateSelectedIngredientsText()
    {
        if (selectedIngredientsText != null)
        {
            string text = "선택된 재료: ";
            if (selectedIngredients.Count == 0)
            {
                text += "없음";
            }
            else
            {
                for (int i = 0; i < selectedIngredients.Count; i++)
                {
                    text += selectedIngredients[i].name;
                    if (i < selectedIngredients.Count - 1)
                    {
                        text += ", ";
                    }
                }
            }
            selectedIngredientsText.text = text;
        }
    }

    public void StartCooking()
    {
        if (isCooking || selectedIngredients.Count == 0)
            return;

        isCooking = true;
        UpdateUI();

        if (CookingStarted != null)
            CookingStarted(selectedIngredients);

        if (cookingStartSound != null)
            audioSource.PlayOneShot(cookingStartSound);

        StartCoroutine(CookingProcess());
    }

    private IEnumerator CookingProcess()
    {
        progressBar.gameObject.SetActive(true);
        progressBar.value = 0;

        float cookingDuration = 3.0f;
        Recipe recipe = CookingManager.Instance?.FindRecipeForIngredients(selectedIngredients);
        if (recipe != null)
        {
            cookingDuration = recipe.cookingTime;
        }

        float elapsedTime = 0;

        while (elapsedTime < cookingDuration)
        {
            elapsedTime += Time.deltaTime;
            progressBar.value = elapsedTime / cookingDuration;

            if (progressFill != null)
                progressFill.color = Color.Lerp(startColor, endColor, progressBar.value);

            yield return null;
        }

        progressBar.value = 1;

        if (cookingFinishSound != null)
            audioSource.PlayOneShot(cookingFinishSound);

        ShowCookingResult();

        if (CookingFinished != null)
            CookingFinished(recipe);

        progressBar.gameObject.SetActive(false);
        isCooking = false;
        selectedIngredients.Clear();
        UpdateUI();
    }

    private void ShowCookingResult()
    {
        Recipe result = null;

        if (CookingManager.Instance != null && selectedIngredients.Count > 0)
        {
            result = CookingManager.Instance.FindRecipeForIngredients(selectedIngredients);
        }

        resultPanel.SetActive(true);

        if (result != null)
        {
            resultNameText.text = result.recipeName;
            resultDescriptionText.text = result.description;
            if (resultImage != null)
            {
                if (result.resultIcon != null)
                {
                    resultImage.sprite = result.resultIcon;
                    resultImage.color = Color.white;
                }
                else
                {
                    resultImage.color = Color.gray;
                }
            }

            if (CookingManager.Instance != null)
            {
                int customerSatisfaction = 80; // 고정된 만족도 값 (실제에서는 계산 필요)
                CookingManager.Instance.SellDish(result, customerSatisfaction);
            }
        }
        else
        {
            resultNameText.text = "알 수 없는 요리";
            resultDescriptionText.text = "이 조합으로는 요리를 만들 수 없습니다.";
            if (resultImage != null)
            {
                resultImage.color = Color.gray;
            }
        }
    }

    private void UpdateSelectedIngredientsUI()
    {
        foreach (Image img in selectedIngredientImages)
        {
            if (img != null)
                img.gameObject.SetActive(false);
        }

        for (int i = 0; i < selectedIngredients.Count; i++)
        {
            selectedIngredientImages[i].gameObject.SetActive(true);

            if (selectedIngredients[i].icon != null)
            {
                selectedIngredientImages[i].sprite = selectedIngredients[i].icon;
            }
            else
            {
                selectedIngredientImages[i].sprite = null;
            }

            selectedIngredientImages[i].color = selectedIngredients[i].color;
        }
    }

    public bool CanStartCooking()
    {
        return !isCooking && selectedIngredients.Count > 0;
    }
}