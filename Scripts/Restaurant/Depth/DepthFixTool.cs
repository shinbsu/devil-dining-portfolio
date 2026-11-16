using UnityEngine;
using System.Collections.Generic;
using Spine.Unity;

public class DepthFixTool : MonoBehaviour
{
    [Header("깊이 설정 일괄 적용 도구")]
    [SerializeField] private KeyCode fixKey = KeyCode.F;
    [SerializeField] private bool autoFixOnStart = true;

    [Header("레이어 설정")]
    [SerializeField] private string targetSortingLayer = "Default";
    [SerializeField] private int farBackgroundOrder = -20;    // 벽, 바닥
    [SerializeField] private int backFurnitureOrder = -10;    // 뒤쪽 가구
    [SerializeField] private int frontFurnitureOrder = -5;    // 앞쪽 가구 (화분 등)
    [SerializeField] private int chairOrder = -2;            // 의자
    [SerializeField] private int customerOrder = 0;          // 손님
    [SerializeField] private int employeeOrder = 1;          // 직원 (손님보다 조금 앞에)
    [SerializeField] private int uiOrder = 100;

    [Header("태그별 분류 (옵션)")]
    [SerializeField] private string[] customerTags = { "Customer" };
    [SerializeField] private string[] employeeTags = { "Employee", "Player" };
    [SerializeField] private string[] chairTags = { "Chair" };
    [SerializeField] private string[] furnitureTags = { "Table", "Furniture" };
    [SerializeField] private string[] backgroundTags = { "Background", "Wall", "Floor" };

    void Start()
    {
        if (autoFixOnStart)
        {
            FixAllDepthSettings();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(fixKey))
        {
            FixAllDepthSettings();
        }
    }

    [ContextMenu("Fix All Depth Settings")]
    public void FixAllDepthSettings()
    {
        Debug.Log("=== 깊이 설정 일괄 수정 시작 ===");

        // 1. 카메라 설정 확인/적용
        SetupCamera();

        // 2. 모든 SpriteRenderer 찾기
        SpriteRenderer[] allSpriteRenderers = FindObjectsOfType<SpriteRenderer>(true);

        // 3. 모든 Spine 오브젝트 찾기
        SkeletonAnimation[] allSpineAnimations = FindObjectsOfType<SkeletonAnimation>(true);

        int fixedCount = 0;
        Dictionary<string, int> categoryCount = new Dictionary<string, int>();

        // SpriteRenderer 처리
        foreach (var renderer in allSpriteRenderers)
        {
            GameObject obj = renderer.gameObject;
            string category = CategorizeObject(obj);
            int targetOrder = GetTargetOrder(category);

            // Spine 캐릭터인 경우 부모 기준으로 분류
            if (IsSpineChild(obj))
            {
                GameObject parent = GetSpineParent(obj);
                if (parent != null)
                {
                    category = CategorizeObject(parent);
                    targetOrder = GetTargetOrder(category);
                }
            }

            // Sorting Layer와 Order 설정
            if (renderer.sortingLayerName != targetSortingLayer || renderer.sortingOrder != targetOrder)
            {
                renderer.sortingLayerName = targetSortingLayer;
                renderer.sortingOrder = targetOrder;
                fixedCount++;
            }

            // 통계 업데이트
            if (!categoryCount.ContainsKey(category))
                categoryCount[category] = 0;
            categoryCount[category]++;

            Debug.Log($"[{category}] {obj.name} (Sprite) - Y: {obj.transform.position.y:F2}, Order: {targetOrder}");
        }

        // Spine 오브젝트 처리
        foreach (var skeletonAnim in allSpineAnimations)
        {
            GameObject obj = skeletonAnim.gameObject;
            string category = CategorizeObject(obj);
            int targetOrder = GetTargetOrder(category);

            MeshRenderer meshRenderer = skeletonAnim.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                if (meshRenderer.sortingLayerName != targetSortingLayer || meshRenderer.sortingOrder != targetOrder)
                {
                    meshRenderer.sortingLayerName = targetSortingLayer;
                    meshRenderer.sortingOrder = targetOrder;
                    fixedCount++;
                }

                // 통계 업데이트
                if (!categoryCount.ContainsKey(category))
                    categoryCount[category] = 0;
                categoryCount[category]++;

                Debug.Log($"[{category}] {obj.name} (Spine) - Y: {obj.transform.position.y:F2}, Order: {targetOrder}");
            }
        }

        Debug.Log($"=== 수정 완료: {fixedCount}개 오브젝트 수정됨 ===");
        foreach (var kvp in categoryCount)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value}개");
        }
    }

    // Spine 캐릭터의 자식인지 확인
    private bool IsSpineChild(GameObject obj)
    {
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            if (parent.GetComponent<SkeletonAnimation>() != null)
                return true;
            parent = parent.parent;
        }
        return false;
    }

    // Spine 캐릭터의 최상위 부모 찾기
    private GameObject GetSpineParent(GameObject obj)
    {
        Transform current = obj.transform;
        GameObject spineParent = null;

        while (current != null)
        {
            if (current.GetComponent<SkeletonAnimation>() != null)
                spineParent = current.gameObject;
            current = current.parent;
        }

        return spineParent;
    }

    private void SetupCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transparencySortMode = TransparencySortMode.CustomAxis;
            mainCam.transparencySortAxis = new Vector3(0, -1, 0);
            Debug.Log("카메라 설정 적용: Y값이 낮을수록 앞에 렌더링");
        }
    }

    private string CategorizeObject(GameObject obj)
    {
        // 태그로 분류
        foreach (string tag in customerTags)
        {
            if (obj.CompareTag(tag))
                return "Customer";
        }

        foreach (string tag in employeeTags)
        {
            if (obj.CompareTag(tag))
                return "Employee";
        }

        foreach (string tag in chairTags)
        {
            if (obj.CompareTag(tag))
                return "Chair";
        }

        foreach (string tag in furnitureTags)
        {
            if (obj.CompareTag(tag))
                return "Furniture";
        }

        foreach (string tag in backgroundTags)
        {
            if (obj.CompareTag(tag))
                return "FarBackground";
        }

        // 컴포넌트로 분류
        if (obj.GetComponent<Customer>() != null)
        {
            return "Customer";
        }

        if (obj.GetComponent<HallEmployee>() != null ||
            obj.GetComponent<KitchenEmployee>() != null)
        {
            return "Employee";
        }

        // UI 판별
        if (obj.GetComponent<Canvas>() != null ||
            obj.GetComponentInParent<Canvas>() != null)
        {
            return "UI";
        }

        // 이름으로 분류
        string name = obj.name.ToLower();

        // 캐릭터 분류
        if (name.Contains("customer") || name.Contains("손님"))
            return "Customer";
        if (name.Contains("employee") || name.Contains("worker") || name.Contains("kitchen") || name.Contains("hall") || name.Contains("직원"))
            return "Employee";

        // 가구 분류
        if (name.Contains("chair") || name.Contains("의자"))
            return "Chair";
        if (name.Contains("table") || name.Contains("desk") || name.Contains("테이블"))
            return "Furniture";
        if (name.Contains("plant") || name.Contains("pot") || name.Contains("화분") || name.Contains("식물"))
            return "FrontFurniture";
        if (name.Contains("shelf") || name.Contains("cabinet") || name.Contains("선반") || name.Contains("캐비닛"))
            return "Furniture";

        // 배경 분류
        if (name.Contains("wall") || name.Contains("floor") || name.Contains("ceiling") || name.Contains("벽") || name.Contains("바닥"))
            return "FarBackground";
        if (name.Contains("pillar") || name.Contains("column") || name.Contains("기둥"))
            return "Furniture";

        // UI 분류
        if (name.Contains("ui") || name.Contains("canvas") || name.Contains("button") || name.Contains("panel"))
            return "UI";

        // Y 위치로 판별
        float yPos = obj.transform.position.y;
        if (yPos < -10f) // 매우 아래쪽은 배경
            return "FarBackground";
        if (yPos > 10f) // 매우 위쪽은 UI
            return "UI";

        return "Furniture"; // 기본값
    }

    private int GetTargetOrder(string category)
    {
        switch (category)
        {
            case "FarBackground":
                return farBackgroundOrder;     // -20: 벽, 바닥
            case "Furniture":
                return backFurnitureOrder;     // -10: 뒤쪽 가구, 기둥
            case "FrontFurniture":
                return frontFurnitureOrder;    // -5:  앞쪽 가구, 화분
            case "Chair":
                return chairOrder;             // -2:  의자
            case "Customer":
                return customerOrder;          // 0:   손님
            case "Employee":
                return employeeOrder;          // 1:   직원
            case "UI":
                return uiOrder;                // 100: UI
            default:
                return backFurnitureOrder;     // -10: 기본값
        }
    }

    [ContextMenu("Show Current Status")]
    public void ShowCurrentStatus()
    {
        Debug.Log("=== 현재 상태 확인 ===");

        Camera mainCam = Camera.main;
        Debug.Log($"카메라 Sort Mode: {mainCam.transparencySortMode}");
        Debug.Log($"카메라 Sort Axis: {mainCam.transparencySortAxis}");

        // SpriteRenderer 확인
        SpriteRenderer[] allSpriteRenderers = FindObjectsOfType<SpriteRenderer>(true);
        var spriteLayerGroups = new Dictionary<string, List<SpriteRenderer>>();

        foreach (var renderer in allSpriteRenderers)
        {
            string key = $"{renderer.sortingLayerName}_{renderer.sortingOrder}";
            if (!spriteLayerGroups.ContainsKey(key))
                spriteLayerGroups[key] = new List<SpriteRenderer>();
            spriteLayerGroups[key].Add(renderer);
        }

        Debug.Log("=== SpriteRenderer 현황 ===");
        foreach (var kvp in spriteLayerGroups)
        {
            Debug.Log($"[{kvp.Key}] {kvp.Value.Count}개 오브젝트");
        }

        // Spine 오브젝트 확인
        SkeletonAnimation[] allSpines = FindObjectsOfType<SkeletonAnimation>(true);
        var spineLayerGroups = new Dictionary<string, List<SkeletonAnimation>>();

        foreach (var spine in allSpines)
        {
            MeshRenderer mr = spine.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                string key = $"{mr.sortingLayerName}_{mr.sortingOrder}";
                if (!spineLayerGroups.ContainsKey(key))
                    spineLayerGroups[key] = new List<SkeletonAnimation>();
                spineLayerGroups[key].Add(spine);
            }
        }

        Debug.Log("=== Spine (MeshRenderer) 현황 ===");
        foreach (var kvp in spineLayerGroups)
        {
            Debug.Log($"[{kvp.Key}] {kvp.Value.Count}개 오브젝트");
            foreach (var spine in kvp.Value)
            {
                Debug.Log($"  - {spine.name} (Y: {spine.transform.position.y:F2})");
            }
        }
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 150, 280, 250));
        GUILayout.Label("=== 깊이 수정 도구 ===");

        if (GUILayout.Button($"[{fixKey}] 깊이 설정 수정"))
        {
            FixAllDepthSettings();
        }

        if (GUILayout.Button("현재 상태 확인"))
        {
            ShowCurrentStatus();
        }

        if (GUILayout.Button("Spine 캐릭터 강제 업데이트"))
        {
            if (Pseudo3DConverter.Instance != null)
                Pseudo3DConverter.Instance.ForceUpdateAllSpineCharacters();
        }

        GUILayout.Label($"Layer: {targetSortingLayer}");
        GUILayout.Label($"배경: {farBackgroundOrder} / 가구: {backFurnitureOrder}");
        GUILayout.Label($"화분: {frontFurnitureOrder} / 의자: {chairOrder}");
        GUILayout.Label($"손님: {customerOrder} / 직원: {employeeOrder}");
        GUILayout.Label($"UI: {uiOrder}");

        GUILayout.EndArea();
    }
}