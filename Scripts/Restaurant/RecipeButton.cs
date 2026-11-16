using UnityEngine;
using UnityEngine.UI;
using System;

public class RecipeButton : MonoBehaviour
{
    private Recipe recipe;
    private Action<Recipe> onSelected;
    private Button btn;
    private Text label;

    private void Awake()
    {
        btn = GetComponent<Button>();
        label = GetComponentInChildren<Text>();
        btn.onClick.AddListener(() => onSelected?.Invoke(recipe));
    }

    public void Initialize(Recipe r, Action<Recipe> callback)
    {
        recipe = r;
        onSelected = callback;
        label.text = r.recipeName;
    }
}