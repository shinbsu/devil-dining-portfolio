using UnityEngine;
using UnityEngine.UI;

public class IngredientButton : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Text nameText;
    [SerializeField] private Button button;

    private Ingredient ingredient;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
    }

    public void SetupIngredient(Ingredient ingredient, System.Action<Ingredient> onClickCallback)
    {
        this.ingredient = ingredient;

        if (icon != null)
        {
            icon.sprite = ingredient.icon;
            icon.color = ingredient.color;
        }

        if (nameText != null)
            nameText.text = ingredient.ingredientName;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClickCallback?.Invoke(ingredient));
        }
    }

    public void SetInteractable(bool interactable)
    {
        if (button != null)
            button.interactable = interactable;
    }

    public bool IsForIngredient(Ingredient other)
    {
        return ingredient != null && ingredient.ingredientName == other.ingredientName;
    }

    public Ingredient GetIngredient()
    {
        return ingredient;
    }
}