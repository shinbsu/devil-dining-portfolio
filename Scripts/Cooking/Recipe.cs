using UnityEngine;
using System.Collections.Generic;

public enum CourseType
{
    Main,    // 메인 요리
    Dessert  // 디저트
}

[CreateAssetMenu(fileName = "New Recipe", menuName = "Cooking/Recipe")]
public class Recipe : ScriptableObject
{
    [Header("기본 정보")]
    public string recipeName;

    [TextArea]
    public string description;

    public Sprite resultIcon;

    [Header("요리 옵션")]
    public CourseType courseType = CourseType.Main;
    public float cookingTime = 3f;
    public int price = 100;
    public int unlockRequirement = 0;
    public bool isStarterRecipe = false;

    [Header("필요 재료")]
    public List<Ingredient> requiredIngredients = new List<Ingredient>();
}
