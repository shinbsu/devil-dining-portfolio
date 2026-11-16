using UnityEngine;
using System.Collections.Generic;

public class DepthTest : MonoBehaviour
{
    [Header("깊이 테스트 도구")]
    [SerializeField] private bool showDepthInfo = false; // 기본적으로 false
    [SerializeField] private KeyCode testKey = KeyCode.Space;
    [SerializeField] private bool enableDebugGUI = true;

    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
        mainCamera = Camera.main;

        // 현재 카메라 설정 확인
        if (mainCamera != null)
        {
            Debug.Log($"=== 카메라 설정 확인 ===");
            Debug.Log($"Camera Transparency Sort Mode: {mainCamera.transparencySortMode}");
            Debug.Log($"Camera Transparency Sort Axis: {mainCamera.transparencySortAxis}");
        }
        else
        {
            Debug.LogError("Main Camera를 찾을 수 없습니다!");
        }

        Debug.Log($"DepthTestHelper가 {gameObject.name}에 추가되었습니다. 스페이스바를 눌러 테스트하세요.");
    }

    void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            ShowDepthInfo();
        }

        // 실시간으로 깊이 정보 표시 (디버그용)
        if (showDepthInfo && spriteRenderer != null)
        {
            Debug.Log($"{gameObject.name} - Y: {transform.position.y:F2}, SortingLayer: {spriteRenderer.sortingLayerName}, Order: {spriteRenderer.sortingOrder}");
        }
    }

    void ShowDepthInfo()
    {
        Debug.Log($"=== [{testKey}] 키가 눌렸습니다! ===");

        // 모든 SpriteRenderer의 위치와 깊이 정보 출력
        var allRenderers = FindObjectsOfType<SpriteRenderer>();
        var sortedList = new List<SpriteRenderer>(allRenderers);

        // Y값 기준으로 정렬 (높은 Y값부터 = 뒤에서부터)
        sortedList.Sort((a, b) => b.transform.position.y.CompareTo(a.transform.position.y));

        Debug.Log("=== 깊이 정보 (Y값 기준 정렬) ===");
        for (int i = 0; i < sortedList.Count; i++)
        {
            var sr = sortedList[i];
            string depthOrder = (i == 0) ? "가장 뒤" : (i == sortedList.Count - 1) ? "가장 앞" : $"{i + 1}번째";
            Debug.Log($"[{depthOrder}] {sr.gameObject.name} - Y: {sr.transform.position.y:F2}, Layer: {sr.sortingLayerName}, Order: {sr.sortingOrder}");
        }

        Debug.Log("=== 카메라 현재 설정 ===");
        Debug.Log($"Sort Mode: {mainCamera.transparencySortMode}");
        Debug.Log($"Sort Axis: {mainCamera.transparencySortAxis}");
    }

    // 화면에 GUI로 정보 표시
    void OnGUI()
    {
        if (!enableDebugGUI) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label($"=== 깊이 테스트 도구 ===");
        GUILayout.Label($"테스트 키: {testKey}");
        GUILayout.Label($"현재 오브젝트: {gameObject.name}");
        GUILayout.Label($"Y 위치: {transform.position.y:F2}");

        if (spriteRenderer != null)
        {
            GUILayout.Label($"Sorting Layer: {spriteRenderer.sortingLayerName}");
            GUILayout.Label($"Order in Layer: {spriteRenderer.sortingOrder}");
        }

        if (GUILayout.Button("깊이 정보 출력"))
        {
            ShowDepthInfo();
        }

        if (GUILayout.Button("위로 이동"))
        {
            MoveUp();
        }

        if (GUILayout.Button("아래로 이동"))
        {
            MoveDown();
        }

        GUILayout.EndArea();
    }

    // 수동으로 깊이 조정 (테스트용)
    [ContextMenu("Move Up")]
    void MoveUp()
    {
        transform.position += Vector3.up * 0.5f;
        Debug.Log($"{gameObject.name} 위로 이동 - Y: {transform.position.y:F2} (더 뒤로 렌더링)");
    }

    [ContextMenu("Move Down")]
    void MoveDown()
    {
        transform.position += Vector3.down * 0.5f;
        Debug.Log($"{gameObject.name} 아래로 이동 - Y: {transform.position.y:F2} (더 앞으로 렌더링)");
    }
}