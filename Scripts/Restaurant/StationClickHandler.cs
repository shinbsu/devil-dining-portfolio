using UnityEngine;
using UnityEngine.EventSystems;

public class StationClickHandler : MonoBehaviour
{
    [Tooltip("Scene에 있는 Main Camera 참조")]
    public Camera mainCamera;
    [Tooltip("Station 레이어만 체크")]
    public LayerMask stationLayer;

    private void Awake()
    {
        // Inspector에 할당 안 했으면 자동으로 찾아 넣어줌
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        // 0) ‘배치 모드’ 중이라면 절대 처리하지 않음
        if (PlacementManager.Instance != null && PlacementManager.Instance.IsInPlacementMode)
            return;

        // 1) 인벤토리 패널이 열려 있으면 클릭 무시
        if (InventoryUIManager.Instance != null && InventoryUIManager.Instance.IsOpen)
            return;

        // 2) UI 위 포인터가 있으면 무시
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // 3) 좌클릭 입력
        if (!Input.GetMouseButtonDown(0))
            return;

        // 4) 월드 좌표로 Raycast
        Vector2 wp = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(wp, Vector2.zero, 0f, stationLayer);
        if (hit.collider == null)
            return;

        // 5) CookingStation 클릭 처리
        var cookStation = hit.collider.GetComponent<CookingStation>();
        if (cookStation != null)
        {
            RestaurantManager.Instance.OnCookingStationClicked(cookStation);
            return;
        }

        // 6) ServingStation 클릭 처리
        var serveStation = hit.collider.GetComponent<ServingStation>();
        if (serveStation != null)
        {
            RestaurantManager.Instance.OnServingStationClicked(serveStation);
            return;
        }
    }
}
