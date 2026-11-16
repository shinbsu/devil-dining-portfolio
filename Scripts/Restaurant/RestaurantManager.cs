using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class RestaurantManager : MonoBehaviour
{
    public static RestaurantManager Instance { get; private set; }

    [Header("Station References")]
    public List<CookingStation> cookingStations;
    public ServingStation servingStation;

    [Header("Popup Canvas")]
    public Canvas popupCanvas;

    [Header("Recipe Popup")]
    public GameObject recipePopup;
    public Transform recipeListParent;
    public GameObject recipeButtonPrefab;

    [Header("Employee Popup")]
    public GameObject employeePopup;
    public Transform employeeListParent;
    public GameObject employeeButtonPrefab;

    [Header("Serving Popup")]
    public GameObject servingPopup;
    public Transform dishListParent;
    public GameObject dishButtonPrefab;

    [Header("Customer Selection Popup")]
    public GameObject customerPopup;
    public Transform customerListParent;
    public GameObject customerButtonPrefab;

    private Vector3 lastClickWorldPos;
    private CookingStation currentCookStation;
    private ServingStation currentServeStation;
    private Recipe selectedRecipe;
    private Recipe selectedDish;
    private Customer selectedCustomer;
    private Dictionary<Recipe, int> assignedCount = new Dictionary<Recipe, int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        // 우클릭 시 열린 팝업 모두 닫기
        if (Input.GetMouseButtonDown(1))
            CloseAllPopups();
    }

    // 요리대 클릭 -> 레시피 선택 팝업
    public void OnCookingStationClicked(CookingStation station)
    {
        if (!station.isAvailable) return;

        var pending = OrderManager.Instance.GetPendingRecipes();
        if (pending.Count == 0) return;

        currentCookStation = station;
        lastClickWorldPos = station.transform.position;
        ShowRecipePopup(pending);
    }

    private void ShowRecipePopup(List<Recipe> recipes)
    {
        CloseAllPopups();
        PositionPopup(recipePopup, lastClickWorldPos);
        recipePopup.SetActive(true);
        SafeClearChildren(recipeListParent);

        // 이미 지시된 만큼 스킵
        var seen = new Dictionary<Recipe, int>();
        foreach (var r in recipes)
        {
            int s = seen.ContainsKey(r) ? seen[r] : 0;
            int a = assignedCount.ContainsKey(r) ? assignedCount[r] : 0;
            if (s < a)
            {
                seen[r] = s + 1;
                continue;
            }
            seen[r] = s + 1;

            var go = Instantiate(recipeButtonPrefab, recipeListParent);
            var rb = go.GetComponent<RecipeButton>();
            rb.Initialize(r, OnRecipeSelected);
        }
    }

    /// <summary>
    /// 요리 지시가 완료되어 더 이상 “지시 대기” 상태가 아니게 될 때 호출
    /// assignedCount[recipe]를 1 감소
    /// </summary>
    public void DecrementAssignedCount(Recipe recipe)
    {
        if (assignedCount.TryGetValue(recipe, out int count))
        {
            count--;
            if (count <= 0)
                assignedCount.Remove(recipe);
            else
                assignedCount[recipe] = count;
        }
    }

    private void OnRecipeSelected(Recipe recipe)
    {
        selectedRecipe = recipe;
        recipePopup.SetActive(false);

        lastClickWorldPos = currentCookStation.transform.position;
        ShowEmployeePopupForCooking();
    }

    private void ShowEmployeePopupForCooking()
    {
        CloseAllPopups();
        PositionPopup(employeePopup, lastClickWorldPos);
        employeePopup.SetActive(true);
        SafeClearChildren(employeeListParent);

        foreach (var emp in FindObjectsOfType<KitchenEmployee>())
        {
            if (emp.State == EmployeeState.Idle && currentCookStation.isAvailable)
            {
                var go = Instantiate(employeeButtonPrefab, employeeListParent);
                var eb = go.GetComponent<EmployeeButton>();
                eb.Initialize(
                    emp,
                    (Recipe r, CookingStation s) =>
                    {
                        bool ok = emp.AssignCooking(r, s);
                        if (ok)
                        {
                            if (assignedCount.ContainsKey(r))
                                assignedCount[r]++;
                            else
                                assignedCount[r] = 1;

                            CloseAllPopups();
                        }
                        return ok;
                    },
                    selectedRecipe,
                    currentCookStation
                );
            }
        }
    }

    // 선반 클릭 -> 요리 목록 팝업
    public void OnServingStationClicked(ServingStation station)
    {
        if (!station.HasDish) return;

        currentServeStation = station;
        lastClickWorldPos = station.transform.position;
        ShowServingPopup();
    }

    private void ShowServingPopup()
    {
        CloseAllPopups();
        PositionPopup(servingPopup, lastClickWorldPos);
        servingPopup.SetActive(true);
        SafeClearChildren(dishListParent);

        foreach (var dish in currentServeStation.GetAllDishes())
        {
            var go = Instantiate(dishButtonPrefab, dishListParent);
            var db = go.GetComponent<DishButton>();
            db.Initialize(dish, OnDishSelected);
        }
    }

    // 요리 선택 -> 고객이 하나면 바로, 여러 명이면 고객 선택 팝업
    private void OnDishSelected(Recipe dish)
    {
        selectedDish = dish;
        servingPopup.SetActive(false);

        var pendingCust = OrderManager.Instance.GetPendingCustomers(dish);
        if (pendingCust.Count <= 1)
        {
            selectedCustomer = pendingCust.Count == 1 ? pendingCust[0] : null;
            ShowEmployeePopupForServing();
        }
        else
        {
            ShowCustomerPopup(pendingCust);
        }
    }

    private void ShowCustomerPopup(List<Customer> list)
    {
        CloseAllPopups();
        PositionPopup(customerPopup, lastClickWorldPos);
        customerPopup.SetActive(true);
        SafeClearChildren(customerListParent);

        foreach (var cust in list)
        {
            var go = Instantiate(customerButtonPrefab, customerListParent);
            var btn = go.GetComponent<Button>();
            var txt = go.GetComponentInChildren<Text>();
            txt.text = $"Seat {cust.SeatIndex + 1}";

            btn.onClick.AddListener(() =>
            {
                selectedCustomer = cust;
                customerPopup.SetActive(false);
                ShowEmployeePopupForServing();
            });
        }
    }

    private void ShowEmployeePopupForServing()
    {
        CloseAllPopups();
        PositionPopup(employeePopup, lastClickWorldPos);
        employeePopup.SetActive(true);
        SafeClearChildren(employeeListParent);

        foreach (var emp in FindObjectsOfType<HallEmployee>())
        {
            if (emp.CurrentState == HallEmployee.State.Patrol)
            {
                var go = Instantiate(employeeButtonPrefab, employeeListParent);
                var eb = go.GetComponent<EmployeeButton>();
                eb.Initialize(
                    emp,
                    (Recipe r, Customer c) =>
                    {
                        bool ok = emp.AssignServing(r, c);
                        if (ok)
                        {
                            OrderManager.Instance.RemoveCustomer(r, c);
                            CloseAllPopups();
                        }
                        return ok;
                    },
                    selectedDish,
                    selectedCustomer
                );
            }
        }
    }

    // 공용 유틸
    private void PositionPopup(GameObject popup, Vector3 worldPos)
    {
        // 1. 현재 메인 카메라 참조
        var camera = Camera.main;

        // 2. 월드 좌표를 스크린 좌표로 변환
        Vector2 screenP = RectTransformUtility.WorldToScreenPoint(camera, worldPos);

        // 3. 스크린 좌표를 팝업 캔버스의 로컬 좌표로 변환
        var popupRT = popup.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            popupCanvas.GetComponent<RectTransform>(),
            screenP,
            popupCanvas.worldCamera, // 오버레이 캔버스의 카메라 (보통 null)
            out Vector2 localP);

        // 4. 팝업 위치 설정
        popupRT.anchoredPosition = localP;
    }

    private void CloseAllPopups()
    {
        recipePopup.SetActive(false);
        employeePopup.SetActive(false);
        servingPopup.SetActive(false);
        customerPopup.SetActive(false);
    }

    private void SafeClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            Destroy(parent.GetChild(i).gameObject);
    }
}
