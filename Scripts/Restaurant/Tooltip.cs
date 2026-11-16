using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Tooltip : MonoBehaviour
{
    [Header("한 줄 재료 표시용 프리팹 (Image+Text)")]
    public GameObject ingredientLinePrefab;

    /// <summary>
    /// 주문전표에서 전달된 IngredientAmount 리스트를 받아 화면에 뿌리기
    /// </summary>
    public void Show(List<IngredientAmount> list)
    {
        foreach (var amt in list)
        {
            // 한 줄 UI 생성
            var go = Instantiate(ingredientLinePrefab, transform);
            // 아이콘
            go.transform.Find("Icon")
                .GetComponent<Image>().sprite = amt.Ingredient.icon;
            // 텍스트
            var txt = go.transform.Find("Name")
                        .GetComponent<Text>();
            txt.text = $"{amt.Ingredient.ingredientName} ×{amt.Amount}";

            // 재고가 충분하면 흰색, 부족하면 회색
            bool ok = InventoryManager.Instance.GetAmount(amt.Ingredient) >= amt.Amount;
            txt.color = ok ? Color.white : Color.gray;
        }
    }
}
