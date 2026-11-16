using UnityEngine;

public class CameraDepthSetup : MonoBehaviour
{
    [Header("깊이 정렬 설정")]
    [SerializeField] private Vector3 sortAxis = new Vector3(0, -1, 0);

    void Awake()
    {
        SetupCameraDepthSorting();
    }

    void SetupCameraDepthSorting()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("Main Camera를 찾을 수 없습니다!");
            return;
        }

        // 카메라 깊이 정렬 설정
        mainCam.transparencySortMode = TransparencySortMode.CustomAxis;
        mainCam.transparencySortAxis = sortAxis;

        Debug.Log($"카메라 깊이 정렬 설정 완료: {sortAxis}");
        Debug.Log("Y값이 낮을수록 앞에 렌더링됩니다.");
    }

    // 런타임에 설정 변경 가능
    [ContextMenu("Apply Depth Settings")]
    void ApplySettings()
    {
        SetupCameraDepthSorting();
    }
}