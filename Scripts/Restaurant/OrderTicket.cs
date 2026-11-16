using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OrderTicket : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image soldOutOverlay;
    [SerializeField] private Text nameText;

    [Header("Background Sprites")]
    [SerializeField] private Sprite mainBackgroundSprite;
    [SerializeField] private Sprite dessertBackgroundSprite;
    [SerializeField] private Color dimColor = new Color(0.3f, 0.3f, 0.3f);

    [Header("Tooltip Prefab")]
    [SerializeField] private GameObject tooltipPrefab;

    [Header("Manual SoldOut Toggle")]
    [Tooltip("테스트용, 체크시 주문전표 클릭하면 SoldOut 상태 반환")]
    [SerializeField] private bool allowManualToggle = true;

    public Recipe Recipe { get; private set; }

    // 내부 상태
    private Recipe recipe;
    private Button button;
    private GameObject tooltipInstance;
    private RectTransform canvasRect;
    private RectTransform tooltipRect;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (soldOutOverlay != null)
            soldOutOverlay.gameObject.SetActive(false);

        Canvas c = GetComponentInParent<Canvas>();
        if (c != null)
            canvasRect = c.GetComponent<RectTransform>();
    }

    /// <summary>
    /// OrderManager에서 주문 생성 시 호출
    /// </summary>
    public void Initialize(Recipe r)
    {
        Recipe = r;
        recipe = r;
        nameText.text = r.recipeName;
        iconImage.sprite = r.resultIcon;

        // 배경 스프라이트 설정
        backgroundImage.sprite = recipe.courseType == CourseType.Dessert
            ? dessertBackgroundSprite
            : mainBackgroundSprite;

        bool hasAll = InventoryManager.Instance.HasIngredients(r);
        Color applyColor = hasAll ? Color.white : dimColor;
        backgroundImage.color = applyColor;
        iconImage.color = applyColor;

        // 버튼 세팅
        button.interactable = hasAll;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    /// <summary>
    /// 수동 클릭 시 SoldOut 처리
    /// </summary>
    private void OnClick()
    {
        if (!allowManualToggle || !button.interactable)
            return;

        MarkSoldOut();
    }

    /// <summary>
    /// SoldOut 오버레이 켜고 2초 뒤 파괴
    /// </summary>
    public void MarkSoldOut()
    {
        if (soldOutOverlay.gameObject.activeSelf)
            return;

        backgroundImage.color = dimColor;
        iconImage.color = dimColor;
        soldOutOverlay.gameObject.SetActive(true);
        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    #region Tooltip
    public void OnPointerEnter(PointerEventData eventData)
    {
        // SoldOut 상태거나 tooltipPrefab/Canvas 없으면 동작 안 함
        if (soldOutOverlay.gameObject.activeSelf
            || tooltipPrefab == null
            || canvasRect == null
            || tooltipInstance != null)
            return;

        // 툴팁 인스턴스화
        tooltipInstance = Instantiate(tooltipPrefab, canvasRect, false);
        tooltipRect = tooltipInstance.GetComponent<RectTransform>();

        // 툴팁 내용 표시
        var tt = tooltipInstance.GetComponent<Tooltip>();
        tt.Show(recipe.GetRequiredIngredients());

        // 마우스 위치 -> 캔버스 로컬 좌표로 변환
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.enterEventCamera,
            out localPoint);

        // 레이아웃 강제 갱신 후 크기 가져오기
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
        Vector2 size = tooltipRect.rect.size;
        Vector2 canvasSize = canvasRect.rect.size;

        // 기본 오프셋 적용
        Vector2 pos = localPoint + new Vector2(50, -20);
        float halfW = size.x / 2f, halfH = size.y / 2f;

        // 화면 밖으로 나가지 않도록 클램핑
        pos.x = Mathf.Clamp(pos.x,
            -canvasSize.x / 2f + halfW,
             canvasSize.x / 2f - halfW);
        pos.y = Mathf.Clamp(pos.y,
            -canvasSize.y / 2f + halfH,
             canvasSize.y / 2f - halfH);

        tooltipRect.anchoredPosition = pos;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipInstance != null)
        {
            Destroy(tooltipInstance);
            tooltipInstance = null;
        }
    }
    #endregion
}
