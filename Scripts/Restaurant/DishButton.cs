using UnityEngine;
using UnityEngine.UI;
using System;

public class DishButton : MonoBehaviour
{
    private Recipe dish;
    private Action<Recipe> onSelected;
    private Button btn;
    private Text label;

    private void Awake()
    {
        btn = GetComponent<Button>();
        label = GetComponentInChildren<Text>();
        btn.onClick.AddListener(() => onSelected?.Invoke(dish));
    }

    public void Initialize(Recipe r, Action<Recipe> callback)
    {
        dish = r;
        onSelected = callback;
        label.text = r.recipeName;
    }
}