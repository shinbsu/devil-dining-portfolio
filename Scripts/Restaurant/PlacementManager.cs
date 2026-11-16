using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PlaceableZoneMapping
{
    public PlaceableItemSO item;          // Item 종류 (SO 에셋)
    public List<ZoneArea> allowedZones;  // 씬 레벨에서 드래그한 ZoneArea
}

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance { get; private set; }

    [Header("보관함 데이터 (SO)")]
    public InventorySystem inventory;

    [Header("아이템별 배치 가능 구역 매핑")]
    public List<PlaceableZoneMapping> zoneMappings;

    // 선택된 아이템 + 미리보기 인스턴스
    private PlaceableItemSO selectedItem;
    private GameObject ghostInstance;

    /// <summary>현재 배치용 고스트가 떠 있는 중인가?</summary>
    public bool IsInPlacementMode => ghostInstance != null;

    public GameObject confirmPopup;
    public Button confirmBtn, storeBtn, cancelBtn;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        inventory.Init();
    }

    /// <summary>
    /// 인벤토리 UI에서 호출됨.
    /// 선택된 PlaceableItemSO를 받아 배치 모드 고스트 시작.
    /// </summary>
    public void OnSelectItem(PlaceableItemSO item)
    {
        if (item == null || !inventory.Has(item.id))
            return;

        selectedItem = item;
        StartPlacementPreview();
    }

    private void StartPlacementPreview()
    {
        // 기존 고스트가 있다면 삭제
        if (ghostInstance != null)
            Destroy(ghostInstance);

        // 새 고스트 인스턴스 생성
        ghostInstance = Instantiate(selectedItem.prefab);
        SetGhostMaterial(ghostInstance, 0.5f);

        // 마우스 움직임 따라다니기
        StartCoroutine(GhostFollowMouse());
    }

    private IEnumerator GhostFollowMouse()
    {
        // 마우스 이동 → ghostInstance 위치 갱신
        while (ghostInstance != null && selectedItem != null)
        {
            Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            wp.z = 0;
            ghostInstance.transform.position = wp;

            // 이 아이템에 대해 설정된 zoneMappings 가져오기
            var mapping = zoneMappings
                .FirstOrDefault(m => m.item == selectedItem);

            // 유효 영역 체크 (mapping이 없으면 무조건 false)
            bool valid = mapping != null
                         && mapping.allowedZones
                            .Any(z => z.IsInside(wp));

            // 영역 안이면 불투명, 밖이면 반투명
            SetGhostMaterial(ghostInstance, valid ? 1f : 0.5f);

            // 좌클릭 & 팝업 미노출 & valid일 때만 설치 확인 팝업
            if (Input.GetMouseButtonDown(0)
                && !confirmPopup.activeSelf
                && valid)
            {
                ShowConfirmPopup(wp);
                yield break;
            }

            // 우클릭 취소
            if (Input.GetMouseButtonDown(1))
            {
                CancelPlacement();
                yield break;
            }

            yield return null;
        }
    }

    private void ShowConfirmPopup(Vector3 worldPos)
    {
        // 1) 팝업 & 캔버스 RectTransform 가져오기
        Canvas canvas = confirmPopup.GetComponentInParent<Canvas>();
        RectTransform canvasRT = canvas.GetComponent<RectTransform>();
        RectTransform popupRT = confirmPopup.GetComponent<RectTransform>();

        // 2) Pivot을 하단 중앙으로 설정 (한 번만 해도 됩니다)
        popupRT.pivot = new Vector2(0.5f, 0f);

        // 3) “아이템 바로 위” world 좌표 계산
        //    ghostInstance에 SpriteRenderer가 붙어 있다고 가정
        float heightOffset = 0f;
        if (ghostInstance != null)
        {
            var sr = ghostInstance.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
                heightOffset = sr.bounds.extents.y;
        }
        // 추가로 UI가 약간 더 위에 떠 있길 원하면 +0.2f 등
        Vector3 worldAbove = worldPos + Vector3.up * (heightOffset + 0.2f);

        // 4) worldAbove → screenPoint
        Vector2 screenPt = RectTransformUtility.WorldToScreenPoint(
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            worldAbove);

        // 5) screenPoint → canvas local point
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRT,
            screenPt,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out Vector2 localPoint);

        // 6) 팝업 위치 지정
        popupRT.anchoredPosition = localPoint;

        // 7) 팝업 켜고 버튼 리스너 세팅
        confirmPopup.SetActive(true);
        confirmBtn.onClick.RemoveAllListeners();
        storeBtn.onClick.RemoveAllListeners();
        cancelBtn.onClick.RemoveAllListeners();

        confirmBtn.onClick.AddListener(ConfirmPlacement);
        storeBtn.onClick.AddListener(StorePlacement);
        cancelBtn.onClick.AddListener(CancelPlacement);
    }

    private void ConfirmPlacement()
    {
        if (!inventory.Remove(selectedItem.id)) return;
        SetSolidMaterial(ghostInstance);
        ghostInstance = null;
        selectedItem = null;
        confirmPopup.SetActive(false);
    }

    private void StorePlacement()
    {
        inventory.Add(selectedItem.id);
        // UI 갱신은 InventoryUIManager 에서 다시!
        InventoryUIManager.Instance?.RefreshInventoryList();
        confirmPopup.SetActive(false);
        // 그대로 ghostInstance 유지 → 다시 배치 가능
    }

    private void CancelPlacement()
    {
        if (ghostInstance != null) Destroy(ghostInstance);
        ghostInstance = null;
        selectedItem = null;
        confirmPopup.SetActive(false);
    }

    // 고스트 반투명 처리
    private void SetGhostMaterial(GameObject go, float alpha)
    {
        foreach (var r in go.GetComponentsInChildren<Renderer>())
            foreach (var m in r.materials)
            {
                var c = m.color;
                c.a = alpha;
                m.color = c;
                m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                m.EnableKeyword("_ALPHABLEND_ON");
            }
    }

    // 실제 불투명 처리
    private void SetSolidMaterial(GameObject go)
    {
        foreach (var r in go.GetComponentsInChildren<Renderer>())
            foreach (var m in r.materials)
            {
                var c = m.color;
                c.a = 1f;
                m.color = c;
            }
    }
}
