using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SimpleResolutionManager : MonoBehaviour
{
    [Header("기준 해상도")]
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920, 1080);

    [Header("카메라 설정")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private bool maintainAspectRatio = true;
    [SerializeField] private Color backgroundColor = Color.black;

    private float baseOrthographicSize;
    private float baseAspectRatio;
    private float currentAspectRatio;

    // 백그라운드 카메라 (레터박스용)
    private Camera backgroundCamera;

    void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        // 기본값 저장
        baseOrthographicSize = mainCamera.orthographicSize;
        baseAspectRatio = referenceResolution.x / referenceResolution.y;

        // 백그라운드 카메라 생성
        CreateBackgroundCamera();

        // 모든 Canvas 설정
        SetupAllCanvases();
    }

    void Start()
    {
        AdjustCamera();
    }

    void Update()
    {
        // 화면 비율 변경 감지
        float newAspectRatio = (float)Screen.width / Screen.height;
        if (Mathf.Abs(newAspectRatio - currentAspectRatio) > 0.01f)
        {
            currentAspectRatio = newAspectRatio;
            AdjustCamera();
        }
    }

    void CreateBackgroundCamera()
    {
        GameObject bgCamObj = new GameObject("BackgroundCamera");
        bgCamObj.transform.parent = transform;

        backgroundCamera = bgCamObj.AddComponent<Camera>();
        backgroundCamera.clearFlags = CameraClearFlags.SolidColor;
        backgroundCamera.backgroundColor = backgroundColor;
        backgroundCamera.cullingMask = 0; // 아무것도 렌더링하지 않음
        backgroundCamera.depth = -100; // 메인 카메라보다 뒤에
        backgroundCamera.orthographic = true;
    }

    void AdjustCamera()
    {
        currentAspectRatio = (float)Screen.width / Screen.height;

        if (maintainAspectRatio)
        {
            // 화면 비율에 따라 카메라 조정
            if (currentAspectRatio >= baseAspectRatio)
            {
                // 화면이 더 넓음 (좌우 레터박스)
                float difference = currentAspectRatio / baseAspectRatio;
                mainCamera.orthographicSize = baseOrthographicSize;

                // 뷰포트 조정으로 레터박스 효과
                float viewportWidth = baseAspectRatio / currentAspectRatio;
                mainCamera.rect = new Rect((1 - viewportWidth) / 2, 0, viewportWidth, 1);
            }
            else
            {
                // 화면이 더 좁음 (위아래 레터박스)
                float difference = baseAspectRatio / currentAspectRatio;
                mainCamera.orthographicSize = baseOrthographicSize * difference;

                // 뷰포트 조정
                float viewportHeight = currentAspectRatio / baseAspectRatio;
                mainCamera.rect = new Rect(0, (1 - viewportHeight) / 2, 1, viewportHeight);
            }
        }
        else
        {
            // 비율 유지하지 않고 전체 화면 사용
            mainCamera.rect = new Rect(0, 0, 1, 1);
            mainCamera.orthographicSize = baseOrthographicSize;
        }

        // UI 캔버스들 업데이트
        UpdateAllCanvases();

        Debug.Log($"카메라 조정 완료: {Screen.width}x{Screen.height} (비율: {currentAspectRatio:F2})");
    }

    void SetupAllCanvases()
    {
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();

        foreach (Canvas canvas in allCanvases)
        {
            // RenderMode가 World Space인 캔버스는 건드리지 않음
            if (canvas.renderMode == RenderMode.WorldSpace)
                continue;

            // Canvas Scaler 설정
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f; // 중간값으로 설정

            // RectTransform 앵커 확인
            CheckAndFixUIAnchors(canvas.transform);
        }
    }

    void UpdateAllCanvases()
    {
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();

        foreach (Canvas canvas in allCanvases)
        {
            if (canvas.renderMode == RenderMode.WorldSpace)
                continue;

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                // 화면 비율에 따라 Match 값 조정
                if (currentAspectRatio > baseAspectRatio)
                {
                    // 화면이 넓을 때는 높이 기준
                    scaler.matchWidthOrHeight = 1f;
                }
                else
                {
                    // 화면이 좁을 때는 너비 기준
                    scaler.matchWidthOrHeight = 0f;
                }
            }
        }
    }

    void CheckAndFixUIAnchors(Transform canvasTransform)
    {
        // 모든 UI 요소의 앵커 설정 확인
        RectTransform[] allRects = canvasTransform.GetComponentsInChildren<RectTransform>();

        foreach (RectTransform rect in allRects)
        {
            // 화면 가장자리에 붙어야 하는 UI 판별
            if (rect.name.Contains("Top") || rect.name.Contains("Header"))
            {
                // 상단 고정
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(1, 1);
                rect.pivot = new Vector2(0.5f, 1);
            }
            else if (rect.name.Contains("Bottom") || rect.name.Contains("Footer"))
            {
                // 하단 고정
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(1, 0);
                rect.pivot = new Vector2(0.5f, 0);
            }
            else if (rect.name.Contains("Left"))
            {
                // 좌측 고정
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 0.5f);
            }
            else if (rect.name.Contains("Right"))
            {
                // 우측 고정
                rect.anchorMin = new Vector2(1, 0);
                rect.anchorMax = new Vector2(1, 1);
                rect.pivot = new Vector2(1, 0.5f);
            }
        }
    }

    // 월드 좌표가 화면에 보이는지 확인
    public bool IsVisibleInCamera(Vector3 worldPosition)
    {
        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(worldPosition);
        return viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
               viewportPoint.y >= 0 && viewportPoint.y <= 1 &&
               viewportPoint.z > 0;
    }

    // 디버그 정보 표시
    void OnGUI()
    {
        if (!Application.isEditor) return;

        GUI.Box(new Rect(10, 10, 260, 100), "Resolution Info");
        GUI.Label(new Rect(20, 30, 240, 20), $"Screen: {Screen.width}x{Screen.height}");
        GUI.Label(new Rect(20, 50, 240, 20), $"Aspect: {currentAspectRatio:F2} (Base: {baseAspectRatio:F2})");
        GUI.Label(new Rect(20, 70, 240, 20), $"Camera Size: {mainCamera.orthographicSize:F2}");
    }

    [ContextMenu("Force Update")]
    public void ForceUpdate()
    {
        SetupAllCanvases();
        AdjustCamera();
    }
}