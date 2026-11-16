using System.Collections.Generic;
using UnityEngine;

public class CookingManager : MonoBehaviour
{
    public static CookingManager Instance { get; private set; }

    [Header("Recipe Database")]
    [SerializeField] private List<Recipe> allRecipes = new List<Recipe>();
    [SerializeField] private List<Ingredient> ingredientAssets = new List<Ingredient>();

    [Header("Experience/Progress")]
    [SerializeField] private int money = 0;
    [SerializeField] private int totalDishesMade = 0;

    private List<Recipe> unlockedRecipes = new List<Recipe>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 시작 레시피 해금 대신 모든 레시피 해금으로 변경
        // UnlockStartingRecipes();
        UnlockAllRecipesForEditor();
    }

    private void UnlockStartingRecipes()
    {
        foreach (Recipe recipe in allRecipes)
        {
            if (recipe.isStarterRecipe)
            {
                unlockedRecipes.Add(recipe);
            }
        }
    }

    public List<Recipe> GetUnlockedRecipes()
    {
        return unlockedRecipes;
    }

    public void SellDish(Recipe recipe, int customerSatisfaction)
    {
        int finalPrice = CalculateFinalPrice(recipe.price, customerSatisfaction);
        money += finalPrice;
        totalDishesMade++;

        Debug.Log($"{recipe.name} 판매: {finalPrice}G | 총 잔고: {money}G");

        CheckForRecipeUnlocks();
    }

    private int CalculateFinalPrice(int basePrice, int satisfaction)
    {
        float multiplier = 0.5f + (satisfaction / 100f);
        return Mathf.RoundToInt(basePrice * multiplier);
    }

    private void CheckForRecipeUnlocks()
    {
        foreach (Recipe recipe in allRecipes)
        {
            if (!unlockedRecipes.Contains(recipe) &&
                recipe.unlockRequirement <= totalDishesMade)
            {
                UnlockRecipe(recipe);
            }
        }
    }

    private void UnlockRecipe(Recipe recipe)
    {
        unlockedRecipes.Add(recipe);
        Debug.Log($"새로운 레시피 해금: {recipe.name}");
    }

    public Recipe FindRecipeForIngredients(List<Ingredient> selectedIngredients)
    {
        if (selectedIngredients == null || selectedIngredients.Count == 0)
            return null;

        foreach (Recipe recipe in unlockedRecipes)
        {
            if (DoIngredientsMatchRecipe(selectedIngredients, recipe))
            {
                return recipe;
            }
        }
        return null;
    }

    private bool DoIngredientsMatchRecipe(List<Ingredient> selectedIngredients, Recipe recipe)
    {
        if (selectedIngredients.Count != recipe.requiredIngredients.Count)
            return false;

        List<Ingredient> tempSelected = new List<Ingredient>(selectedIngredients);
        List<Ingredient> tempRequired = new List<Ingredient>(recipe.requiredIngredients);

        for (int i = tempSelected.Count - 1; i >= 0; i--)
        {
            bool found = false;
            for (int j = tempRequired.Count - 1; j >= 0; j--)
            {
                if (tempSelected[i] == tempRequired[j] ||
                    tempSelected[i].ingredientName == tempRequired[j].ingredientName)
                {
                    tempRequired.RemoveAt(j);
                    found = true;
                    break;
                }
            }

            if (!found)
                return false;
        }

        return tempRequired.Count == 0;
    }

    public void UnlockAllRecipesForEditor()
    {
        // 실행 중 체크 제거
        // if (!Application.isPlaying) return;

        foreach (Recipe recipe in allRecipes)
        {
            if (!unlockedRecipes.Contains(recipe))
            {
                unlockedRecipes.Add(recipe);
            }
        }

        Debug.Log($"{unlockedRecipes.Count}개의 레시피를 해금했습니다.");
    }

    public List<Ingredient> GetAvailableIngredients()
    {
        return new List<Ingredient>(ingredientAssets);
    }
}