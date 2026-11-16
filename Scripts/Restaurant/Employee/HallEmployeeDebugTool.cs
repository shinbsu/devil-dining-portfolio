using UnityEngine;

public class HallEmployeeDebugTool : MonoBehaviour
{
    [Header("디버그 도구")]
    [SerializeField] private KeyCode debugKey = KeyCode.H;
    [SerializeField] private bool showDebugGUI = true;

    void Update()
    {
        if (Input.GetKeyDown(debugKey))
        {
            DebugAllHallEmployees();
        }
    }

    void DebugAllHallEmployees()
    {
        Debug.Log("=== 홀 직원 디버그 시작 ===");

        HallEmployee[] hallEmployees = FindObjectsOfType<HallEmployee>();

        foreach (var emp in hallEmployees)
        {
            Debug.Log($"--- 홀 직원: {emp.name} ---");
            Debug.Log($"위치: {emp.transform.position}");
            Debug.Log($"상태: {emp.CurrentState}");

            // SpriteRenderer 정보
            SpriteRenderer[] renderers = emp.GetComponentsInChildren<SpriteRenderer>();
            Debug.Log($"SpriteRenderer 개수: {renderers.Length}");

            foreach (var renderer in renderers)
            {
                Debug.Log($"  - {renderer.gameObject.name}: Layer={renderer.sortingLayerName}, Order={renderer.sortingOrder}");
            }

            // 2.5D 등록 상태 확인
            if (Pseudo3DConverter.Instance != null)
            {
                Debug.Log($"2.5D 인스턴스 있음: Y={emp.transform.position.y}, Z={emp.transform.position.z}");
            }
            else
            {
                Debug.Log("2.5D 인스턴스 없음!");
            }

            Debug.Log("------------------------");
        }

        // 다른 캐릭터와 비교
        Customer[] customers = FindObjectsOfType<Customer>();
        KitchenEmployee[] kitchenEmps = FindObjectsOfType<KitchenEmployee>();

        Debug.Log($"비교 - 손님 수: {customers.Length}, 주방직원 수: {kitchenEmps.Length}");

        if (customers.Length > 0)
        {
            var firstCustomer = customers[0];
            Debug.Log($"첫 번째 손님 위치: {firstCustomer.transform.position}");
        }
    }

    [ContextMenu("Fix All Hall Employees")]
    public void FixAllHallEmployees()
    {
        HallEmployee[] hallEmployees = FindObjectsOfType<HallEmployee>();

        foreach (var emp in hallEmployees)
        {
            // 강제 2.5D 재등록
            if (Pseudo3DConverter.Instance != null)
            {
                Pseudo3DConverter.Instance.RegisterAndConvertObject(emp.transform);
            }

            // 모든 SpriteRenderer 강제 설정
            SpriteRenderer[] renderers = emp.GetComponentsInChildren<SpriteRenderer>();
            foreach (var renderer in renderers)
            {
                renderer.sortingLayerName = "Default";
                renderer.sortingOrder = 0;
            }

            Debug.Log($"홀 직원 {emp.name} 강제 수정 완료");
        }
    }

    void OnGUI()
    {
        if (!showDebugGUI) return;

        GUILayout.BeginArea(new Rect(10, 300, 300, 200));
        GUILayout.Label("=== 홀 직원 디버그 ===");

        if (GUILayout.Button($"[{debugKey}] 홀 직원 디버그"))
        {
            DebugAllHallEmployees();
        }

        if (GUILayout.Button("홀 직원 강제 수정"))
        {
            FixAllHallEmployees();
        }

        // 실시간 정보 표시
        HallEmployee[] hallEmployees = FindObjectsOfType<HallEmployee>();
        foreach (var emp in hallEmployees)
        {
            if (emp != null)
            {
                GUILayout.Label($"{emp.name}: Y={emp.transform.position.y:F2}, Z={emp.transform.position.z:F2}");
            }
        }

        GUILayout.EndArea();
    }
}