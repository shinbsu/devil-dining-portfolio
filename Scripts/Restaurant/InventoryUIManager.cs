using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance { get; private set; }

    [Header("보관함 Data (SO)")]
    public InventorySystem inventory;

    [Header("Toggle Button")]
    public Button inventoryToggleButton;

    [Header("Inventory Panel")]
    public GameObject inventoryPanel;           // UI Panel 오브젝트
    public RectTransform contentParent;         // 버튼들이 쌓일 Content(Scroll View의 Content)
    public GameObject itemButtonPrefab;         // InventoryItemButton Prefab

    /// <summary>
    /// 인벤토리 패널이 켜져 있는지
    /// </summary>
    public bool IsOpen => inventoryPanel != null && inventoryPanel.activeSelf;

    private void Awake()
    {
        // ② 싱글톤 초기화
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // panel은 기본 끄기
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
    }

    private void Start()
    {
        inventory.Init();

        // 2) Toggle Button 클릭 리스너 연결
        if (inventoryToggleButton != null)
            inventoryToggleButton.onClick.AddListener(ToggleInventoryPanel);
    }

    /// <summary>
    /// 인벤토리 패널 보이기/숨기기 토글
    /// </summary>
    private void ToggleInventoryPanel()
    {
        if (inventoryPanel == null) return;

        bool isActive = inventoryPanel.activeSelf;
        if (!isActive)
        {
            // 켜기 직전 매번 갱신
            RefreshInventoryList();
        }
        inventoryPanel.SetActive(!isActive);
    }

    /// <summary>
    /// 현재 보관함 상태를 기반으로 버튼 목록 갱신
    /// </summary>
    public void RefreshInventoryList()
    {
        if (contentParent == null || itemButtonPrefab == null || inventory == null)
            return;

        // 1) 기존 버튼들 삭제
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        // 2) 모든 PlaceableItemSO를 한 번만 순회
        foreach (var item in inventory.allPlaceables)
        {
            int count = inventory.GetCount(item.id);
            // 재고가 하나도 없으면 아예 목록에 안 보이게 하고 싶으면 continue
            if (count <= 0)
                continue;

            // 3) 버튼 생성 (한 번만!)
            var go = Instantiate(itemButtonPrefab, contentParent);
            go.name = $"Inv_{item.id}";

            // 4) 버튼 내부 텍스트 세팅
            var texts = go.GetComponentsInChildren<Text>();
            if (texts.Length >= 2)
            {
                texts[0].text = item.displayName;
                texts[1].text = count.ToString();
            }
            else if (texts.Length == 1)
            {
                texts[0].text = $"{item.displayName} x{count}";
            }

            // 5) 클릭 가능 여부
            var btn = go.GetComponent<Button>();
            btn.interactable = (count > 0);

            // 6) 클릭 시 배치 모드로 넘기고 패널 닫기
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                PlacementManager.Instance.OnSelectItem(item);
                inventoryPanel.SetActive(false);
            });
        }

        // 7) 레이아웃 즉시 갱신
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);
    }

}
