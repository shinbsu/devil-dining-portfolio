using UnityEngine;
using System.Collections.Generic;

public class KitchenSystem : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private float defaultCookingTime = 3f;
    [SerializeField] private Color progressBarColor = Color.green;

    [Header("Managers")]
    [SerializeField] private CookingUIController uiController;

    [Header("Input Controls")]
    [SerializeField] private KeyCode startCookingKey = KeyCode.S;
    [SerializeField] private KeyCode clearIngredientsKey = KeyCode.X;
    [SerializeField] private KeyCode unlockRecipesKey = KeyCode.A;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    private void OnEnable()
    {
        CookingUIController.CookingStarted += OnCookingStarted;
        CookingUIController.CookingFinished += OnCookingFinished;
    }

    private void OnDisable()
    {
        CookingUIController.CookingStarted -= OnCookingStarted;
        CookingUIController.CookingFinished -= OnCookingFinished;
    }

    private void Start()
    {
        if (CookingManager.Instance == null)
        {
            Debug.LogError("CookingManager 싱글톤 인스턴스가 없습니다.");
        }

        if (uiController == null)
        {
            uiController = GetComponent<CookingUIController>();
            if (uiController == null)
            {
                Debug.LogError("CookingUIController를 찾을 수 없습니다.");
            }
        }
    }

    private void Update()
    {
        // S 키로 요리 시작
        if (Input.GetKeyDown(startCookingKey))
        {
            if (uiController != null && uiController.CanStartCooking())
            {
                if (showDebugLogs)
                    Debug.Log("S 키 입력: 요리 시작");
                uiController.StartCooking();
            }
        }

        // X 키로 재료 초기화
        if (Input.GetKeyDown(clearIngredientsKey))
        {
            if (uiController != null)
            {
                if (showDebugLogs)
                    Debug.Log("X 키 입력: 재료 초기화");
                uiController.ClearIngredients();
            }
        }

        // A 키로 모든 레시피 해금
        if (Input.GetKeyDown(unlockRecipesKey))
        {
            if (CookingManager.Instance != null)
            {
                if (showDebugLogs)
                    Debug.Log("A 키 입력: 모든 레시피 해금");
                CookingManager.Instance.UnlockAllRecipesForEditor();
            }
        }
    }

    private void OnCookingStarted(List<Ingredient> ingredients)
    {
        if (showDebugLogs)
            Debug.Log($"요리 시작: {ingredients.Count}개의 재료로 요리 시작");
    }

    private void OnCookingFinished(Recipe recipe)
    {
        if (recipe != null)
        {
            if (showDebugLogs)
                Debug.Log($"요리 완료: {recipe.name}");
        }
        else
        {
            if (showDebugLogs)
                Debug.Log("알 수 없는 요리가 완성되었습니다.");
        }
    }
}