using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StaffSelectionManager : MonoBehaviour
{
    public Button GameStr;

    [System.Serializable]
    public class StaffData
    {
        public string staffName;
        public Sprite staffImage;
        public bool isKitchenStaff; // true면 주방, false면 홀 직원
        public float efficiency = 1.0f; // 효율성 스탯
        public float speed = 1.0f; // 속도 스탯
    }

    [Header("Staff Data")]
    [SerializeField] private List<StaffData> availableStaff = new List<StaffData>();

    [Header("UI References")]
    [SerializeField] private Transform hallStaffContent; // 홀 직원 그리드 컨텐츠
    [SerializeField] private Transform kitchenStaffContent; // 주방 직원 그리드 컨텐츠

    [SerializeField] private Transform selectedHallStaffArea; // 선택된 홀 직원 표시 영역
    [SerializeField] private Transform selectedKitchenStaffArea; // 선택된 주방 직원 표시 영역

    [SerializeField] private Button startGameButton; // 게임 시작 버튼
    [SerializeField] private GameObject staffButtonPrefab; // 직원 버튼 프리팹

    [Header("Settings")]
    [SerializeField] private int maxHallStaff = 4; // 최대 선택 가능한 홀 직원 수
    [SerializeField] private int maxKitchenStaff = 4; // 최대 선택 가능한 주방 직원 수

    // 선택된 직원 목록
    private List<StaffData> selectedHallStaff = new List<StaffData>();
    private List<StaffData> selectedKitchenStaff = new List<StaffData>();

    // 직원 버튼 참조 저장용
    private Dictionary<Button, StaffData> buttonToStaffMap = new Dictionary<Button, StaffData>();

    private void Start()
    {
        InitializeStaffButtons();
        UpdateSelectedStaffDisplay();
        startGameButton.onClick.AddListener(StartGame);
    }

    private void InitializeStaffButtons()
    {
        // 기존 버튼들 제거
        foreach (Transform child in hallStaffContent)
            Destroy(child.gameObject);
        foreach (Transform child in kitchenStaffContent)
            Destroy(child.gameObject);

        // 직원 버튼 생성
        foreach (var staff in availableStaff)
        {
            Transform parentContent = staff.isKitchenStaff ? kitchenStaffContent : hallStaffContent;
            GameObject buttonObj = Instantiate(staffButtonPrefab, parentContent);

            // 이미지 설정
            Image staffImage = buttonObj.transform.GetChild(0).GetComponent<Image>();
            staffImage.sprite = staff.staffImage;

            // 버튼 이벤트 연결
            Button button = buttonObj.GetComponent<Button>();
            buttonToStaffMap.Add(button, staff);
            button.onClick.AddListener(() => ToggleStaffSelection(button, staff));
        }
    }

    private void ToggleStaffSelection(Button button, StaffData staff)
    {
        // 이미 선택된 직원인지 확인
        List<StaffData> targetList = staff.isKitchenStaff ? selectedKitchenStaff : selectedHallStaff;
        int maxCount = staff.isKitchenStaff ? maxKitchenStaff : maxHallStaff;

        if (targetList.Contains(staff))
        {
            // 이미 선택된 경우 해제
            targetList.Remove(staff);
            button.GetComponent<Image>().color = Color.white; // 선택 해제 시 색상
        }
        else
        {
            // 새로 선택하는 경우
            if (targetList.Count < maxCount)
            {
                targetList.Add(staff);
                button.GetComponent<Image>().color = Color.green; // 선택 시 색상
            }
            else
            {
                Debug.Log($"최대 {maxCount}명의 {(staff.isKitchenStaff ? "주방" : "홀")} 직원만 선택 가능합니다.");
            }
        }

        // 선택된 직원 표시 업데이트
        UpdateSelectedStaffDisplay();
    }

    private void UpdateSelectedStaffDisplay()
    {
        // 홀 직원 영역 초기화
        foreach (Transform child in selectedHallStaffArea)
        {
            // Text 컴포넌트는 유지하고 이미지만 제거
            if (child.GetComponent<Text>() == null && child.GetComponent<Text>() == null)
                Destroy(child.gameObject);
        }

        // 주방 직원 영역 초기화
        foreach (Transform child in selectedKitchenStaffArea)
        {
            // Text 컴포넌트는 유지하고 이미지만 제거
            if (child.GetComponent<Text>() == null && child.GetComponent<Text>() == null)
                Destroy(child.gameObject);
        }

        // 선택된 홀 직원 표시
        foreach (var staff in selectedHallStaff)
        {
            CreateSelectedStaffIcon(staff, selectedHallStaffArea);
        }

        // 선택된 주방 직원 표시
        foreach (var staff in selectedKitchenStaff)
        {
            CreateSelectedStaffIcon(staff, selectedKitchenStaffArea);
        }

        // 게임 시작 버튼 활성화 여부 (최소 1명 이상 선택해야 함)
        startGameButton.interactable = (selectedHallStaff.Count > 0 && selectedKitchenStaff.Count > 0);
    }

    private void CreateSelectedStaffIcon(StaffData staff, Transform parentArea)
    {
        GameObject iconObj = new GameObject("SelectedStaffIcon");
        iconObj.transform.SetParent(parentArea, false);

        // 이미지 컴포넌트 추가
        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.sprite = staff.staffImage;

        // 크기 설정
        RectTransform rect = iconObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(50, 50);
    }

    private void StartGame()
    {
        // 선택된 직원 정보를 게임 매니저에 전달하고 게임 시작
        if (selectedHallStaff.Count > 0 && selectedKitchenStaff.Count > 0)
        {
            Debug.Log("게임 시작!");

            // RestaurantManager.Instance.SetSelectedStaff(selectedHallStaff, selectedKitchenStaff);
            SceneManager.LoadScene("RestaurantScene");
        }
        else
        {
            Debug.Log("홀과 주방 직원을 모두 1명 이상 선택해야 합니다.");
        }
    }

    // 게임 매니저에서 선택된 직원 정보를 가져갈 수 있는 메서드
    public List<StaffData> GetSelectedHallStaff()
    {
        return new List<StaffData>(selectedHallStaff);
    }

    public List<StaffData> GetSelectedKitchenStaff()
    {
        return new List<StaffData>(selectedKitchenStaff);
    }
}