using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("테스트 모드: 재료 항상 충분")]
    public bool testMode = true;

    private Dictionary<Ingredient, int> stock = new Dictionary<Ingredient, int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // // 테스트용: 모든 가능한 재료 5개씩 채워두기
        // foreach (var ing in Resources.LoadAll<Ingredient>(""))
        //     stock[ing] = 5;
    }

    public bool HasIngredients(Recipe r)
    {
        if (testMode) return true;
        foreach (var ing in r.requiredIngredients)
            if (!stock.ContainsKey(ing) || stock[ing] < 1)
                return false;
        return true;
    }

    public int GetAmount(Ingredient ing)
    {
        if (testMode) return 999;
        return stock.ContainsKey(ing) ? stock[ing] : 0;
    }

    // 요리 시작 시 재료 차감
    public void ConsumeIngredients(Recipe r)
    {
        if (testMode) return;
        foreach (var ing in r.requiredIngredients)
            stock[ing] = Mathf.Max(0, stock[ing] - 1);
    }
}
