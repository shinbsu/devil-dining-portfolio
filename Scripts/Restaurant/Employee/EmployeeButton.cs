using UnityEngine;
using UnityEngine.UI;
using System;

public class EmployeeButton : MonoBehaviour
{
    [Header("버튼에 표시할 텍스트")]
    [SerializeField] private Text buttonText;

    private Button button;

    private void Awake()
    {
        // Button 컴포넌트 확보
        button = GetComponent<Button>();
        if (button == null)
            button = gameObject.AddComponent<Button>();

        // buttonText 가 Inspector에서 연결되지 않은 경우, 자식 Text 컴포넌트를 찾아 할당
        if (buttonText == null)
            buttonText = GetComponentInChildren<Text>();

        if (buttonText == null)
            Debug.LogError($"[{nameof(EmployeeButton)}] buttonText 연결안됨, 자식Text도 못찾음.");
    }

    /// <summary>
    /// 주방 직원용 초기화 (레시피 지시)
    /// </summary>
    /// <param name="emp">KitchenEmployee 인스턴스</param>
    /// <param name="assignFunc">emp.AssignCooking 레퍼런스</param>
    /// <param name="recipe">선택된 Recipe</param>
    /// <param name="station">선택된 CookingStation</param>
    public void Initialize(
        KitchenEmployee emp,
        Func<Recipe, CookingStation, bool> assignFunc,
        Recipe recipe,
        CookingStation station)
    {
        // 버튼 텍스트 세팅
        if (buttonText != null)
            buttonText.text = $"{emp.name} ▶ {recipe.recipeName}";

        // 클릭 이벤트
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            // assignFunc이 true 반환 시, 내부에서 팝업 닫기 등 처리
            if (assignFunc(recipe, station))
            {
                // 필요시 추가 후처리 로직 추가할 부분
            }
        });
    }

    /// <summary>
    /// 홀 직원용 초기화 (서빙 지시)
    /// </summary>
    /// <param name="emp">HallEmployee 인스턴스</param>
    /// <param name="assignFunc">emp.AssignServing 레퍼런스</param>
    /// <param name="recipe">선택된 Recipe</param>
    /// <param name="customer">서빙 대상 Customer</param>
    public void Initialize(
        HallEmployee emp,
        Func<Recipe, Customer, bool> assignFunc,
        Recipe recipe,
        Customer customer)
    {
        // 버튼 텍스트: 직원 이름 -> 고객 좌석
        if (buttonText != null)
            buttonText.text = $"{emp.name} ▶ Seat {customer.SeatIndex + 1}";

        // 클릭 이벤트
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            if (assignFunc(recipe, customer))
            {
                // 필요시 추가 후처리 로직 추가할 부분
            }
        });
    }
}
