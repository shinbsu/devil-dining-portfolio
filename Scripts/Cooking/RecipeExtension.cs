using System.Collections.Generic;
using UnityEngine;

// Recipe ScriptableObject에 대한 확장 메서드
public static class RecipeExtension
{
    public static string GetRecipeName(this Recipe recipe)
        => recipe != null ? recipe.recipeName : "";

    // 디저트 여부 확인
    public static bool IsDessert(this Recipe recipe)
        => recipe != null && recipe.courseType == CourseType.Dessert;

    // 요리 시간 가져오기
    public static float GetCookingTime(this Recipe recipe)
        => recipe != null ? recipe.cookingTime : 5f;

    // 필요 재료 목록을 IngredientAmount 형식으로 변환
    public static List<IngredientAmount> GetRequiredIngredients(this Recipe recipe)
    {
        var result = new List<IngredientAmount>();
        if (recipe == null) return result;

        foreach (var ingredient in recipe.requiredIngredients)
        {
            if (ingredient != null)
                result.Add(new IngredientAmount { Ingredient = ingredient, Amount = 1 });
        }
        return result;
    }
}
