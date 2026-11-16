using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PCLandscapeResolutionManager : MonoBehaviour
{
    [Header("기준 해상도 (16:9)")]
    [SerializeField] private float referenceWidth = 1920f;
    [SerializeField] private float referenceHeight = 1080f;

    [Header("카메라 설정")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float baseOrthographicSize = 5.4f;
    [SerializeField] private Color letterboxColor = Color.black;

    [Header("디버그 옵션")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool forceAspectRatio = true; // 비율 강제 적용

    [Header("UI 설정")]
    [SerializeField] private bool autoSetupCanvas = true;
    [SerializeField] private List<Canvas> uiCanvases = new List<Canvas>();

    private float targetAspectRatio;
    private Camera topLetterboxCamera;
    private Camera bottomLetterboxCamera;
    private Camera leftLetterboxCamera;
    private Camera rightLetterboxCamera;
    private float lastScreenWidth;
    private float lastScreenHeight;

    void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null && mainCamera.orthographic)
            baseOrthographicSize = mainCamera.orthographicSize;

        targetAspectRatio = referenceWidth / referenceHeight;

        // 레터박스 카메라 생성
        CreateLetterboxCameras();

        // Canvas 자동 찾기
        if (autoSetupCanvas)
        {
            FindAndSetupCanvases();
        }

        // PC 전용 설정
        SetupPCSettings();
    }

    void Start()
    {
        // 초기에 모든 레터박스 숨기기
        HideLetterboxes();
        HideSideLetterboxes();

        UpdateResolution();
    }

    void Update()
    {
        // 해상도 변경 감지
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            UpdateResolution();
        }
    }

    void SetupPCSettings()
    {
        // PC 전용 설정
        Application.targetFrameRate = -1; // VSync 사용

        // Unity Editor에서는 해상도 변경하지 않음
#if !UNITY_EDITOR
        // 빌드에서만 최소 해상도 설정
        if (Screen.width < 1280 || Screen.height < 720)
        {
            Screen.SetResolution(1280, 720, Screen.fullScreen);
        }
#endif
    }

    void CreateLetterboxCameras()
    {
        // 상단 레터박스
        GameObject topObj = new GameObject("TopLetterbox");
        topObj.transform.SetParent(transform);
        topLetterboxCamera = topObj.AddComponent<Camera>();
        topLetterboxCamera.clearFlags = CameraClearFlags.SolidColor;
        topLetterboxCamera.backgroundColor = letterboxColor;
        topLetterboxCamera.cullingMask = 0;
        topLetterboxCamera.depth = mainCamera.depth - 1;
        topLetterboxCamera.orthographic = true;

        // 하단 레터박스
        GameObject bottomObj = new GameObject("BottomLetterbox");
        bottomObj.transform.SetParent(transform);
        bottomLetterboxCamera = bottomObj.AddComponent<Camera>();
        bottomLetterboxCamera.clearFlags = CameraClearFlags.SolidColor;
        bottomLetterboxCamera.backgroundColor = letterboxColor;
        bottomLetterboxCamera.cullingMask = 0;
        bottomLetterboxCamera.depth = mainCamera.depth - 1;
        bottomLetterboxCamera.orthographic = true;

        // 좌측 레터박스
        GameObject leftObj = new GameObject("LeftLetterbox");
        leftObj.transform.SetParent(transform);
        leftLetterboxCamera = leftObj.AddComponent<Camera>();
        leftLetterboxCamera.clearFlags = CameraClearFlags.SolidColor;
        leftLetterboxCamera.backgroundColor = letterboxColor;
        leftLetterboxCamera.cullingMask = 0;
        leftLetterboxCamera.depth = mainCamera.depth - 1;
        leftLetterboxCamera.orthographic = true;

        // 우측 레터박스
        GameObject rightObj = new GameObject("RightLetterbox");
        rightObj.transform.SetParent(transform);
        rightLetterboxCamera = rightObj.AddComponent<Camera>();
        rightLetterboxCamera.clearFlags = CameraClearFlags.SolidColor;
        rightLetterboxCamera.backgroundColor = letterboxColor;
        rightLetterboxCamera.cullingMask = 0;
        rightLetterboxCamera.depth = mainCamera.depth - 1;
        rightLetterboxCamera.orthographic = true;
    }

    void UpdateResolution()
    {
        float screenAspectRatio = (float)Screen.width / Screen.height;

        // Unity Editor에서 세로 화면일 때도 제대로 처리
        if (screenAspectRatio < 1f && forceAspectRatio)
        {
            Debug.LogWarning($"세로 화면 감지! 16:9 비율로 강제 조정합니다. (현재: {screenAspectRatio:F2})");

            // 세로 화면일 때는 화면 중앙에 16:9 비율의 게임 화면을 표시
            float gameHeight = 1f;
            float gameWidth = targetAspectRatio / screenAspectRatio;
            float horizontalLetterbox = (1f - gameWidth) / 2f;

            mainCamera.rect = new Rect(horizontalLetterbox, 0, gameWidth, 1);
            mainCamera.orthographicSize = baseOrthographicSize;

            // 좌우 레터박스 표시를 위한 추가 카메라 필요
            ShowSideLetterboxes(horizontalLetterbox);
            HideLetterboxes(); // 상하 레터박스는 숨김
        }
        else if (screenAspectRatio >= targetAspectRatio)
        {
            // 화면이 기준보다 넓음 - 높이에 맞춤 (좌우 레터박스)
            float viewportWidth = targetAspectRatio / screenAspectRatio;
            float sideBarWidth = (1f - viewportWidth) / 2f;

            mainCamera.rect = new Rect(sideBarWidth, 0, viewportWidth, 1);
            mainCamera.orthographicSize = baseOrthographicSize;

            ShowSideLetterboxes(sideBarWidth);
            HideLetterboxes();
        }
        else
        {
            // 화면이 16:9보다 좁지만 가로 모드 - 너비에 맞춤 (위아래 레터박스)
            float viewportHeight = screenAspectRatio / targetAspectRatio;
            float letterboxHeight = (1f - viewportHeight) / 2f;

            mainCamera.rect = new Rect(0, letterboxHeight, 1, viewportHeight);
            mainCamera.orthographicSize = baseOrthographicSize;

            ShowLetterboxes(letterboxHeight);
            HideSideLetterboxes();
        }

        // UI Canvas 업데이트
        UpdateCanvases(screenAspectRatio);

        Debug.Log($"해상도 업데이트: {Screen.width}x{Screen.height} (비율: {screenAspectRatio:F2})");
    }

    void ShowLetterboxes(float height)
    {
        if (topLetterboxCamera != null)
        {
            topLetterboxCamera.gameObject.SetActive(true);
            topLetterboxCamera.rect = new Rect(0, 1f - height, 1, height);
        }

        if (bottomLetterboxCamera != null)
        {
            bottomLetterboxCamera.gameObject.SetActive(true);
            bottomLetterboxCamera.rect = new Rect(0, 0, 1, height);
        }
    }

    void HideLetterboxes()
    {
        if (topLetterboxCamera != null)
            topLetterboxCamera.gameObject.SetActive(false);

        if (bottomLetterboxCamera != null)
            bottomLetterboxCamera.gameObject.SetActive(false);
    }

    void ShowSideLetterboxes(float width)
    {
        if (leftLetterboxCamera != null)
        {
            leftLetterboxCamera.gameObject.SetActive(true);
            leftLetterboxCamera.rect = new Rect(0, 0, width, 1);
        }

        if (rightLetterboxCamera != null)
        {
            rightLetterboxCamera.gameObject.SetActive(true);
            rightLetterboxCamera.rect = new Rect(1f - width, 0, width, 1);
        }
    }

    void HideSideLetterboxes()
    {
        if (leftLetterboxCamera != null)
            leftLetterboxCamera.gameObject.SetActive(false);

        if (rightLetterboxCamera != null)
            rightLetterboxCamera.gameObject.SetActive(false);
    }

    void FindAndSetupCanvases()
    {
        Canvas[] allCanvases = FindObjectsOfType<Canvas>(true); // 비활성 Canvas도 포함
        uiCanvases.Clear();

        foreach (Canvas canvas in allCanvases)
        {
            // World Space Canvas는 제외
            if (canvas.renderMode != RenderMode.WorldSpace)
            {
                uiCanvases.Add(canvas);
                SetupCanvasScaler(canvas);
            }
        }

        Debug.Log($"{uiCanvases.Count}개의 UI Canvas를 찾아 설정했습니다.");
    }

    void SetupCanvasScaler(Canvas canvas)
    {
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(referenceWidth, referenceHeight);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        // 초기 설정은 중간값으로
        float currentRatio = (float)Screen.width / Screen.height;
        if (currentRatio < 1f)
        {
            scaler.matchWidthOrHeight = 0f; // 세로일 때는 너비 기준
        }
        else
        {
            scaler.matchWidthOrHeight = 0.5f; // 가로일 때는 중간값
        }

        // Anchor 프리셋 자동 설정
        SetupUIAnchors(canvas.transform);
    }

    void UpdateCanvases(float currentAspectRatio)
    {
        foreach (Canvas canvas in uiCanvases)
        {
            if (canvas == null) continue;

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                // 세로 화면일 때도 16:9 기준으로 스케일링
                if (currentAspectRatio < 1f)
                {
                    // 세로 화면일 때는 너비 기준으로 강제
                    scaler.matchWidthOrHeight = 0f;
                }
                else if (currentAspectRatio > targetAspectRatio)
                {
                    // 화면이 더 넓을 때는 높이 기준
                    scaler.matchWidthOrHeight = 1f;
                }
                else
                {
                    // 화면이 더 좁을 때는 너비 기준
                    scaler.matchWidthOrHeight = 0f;
                }
            }
        }
    }

    void SetupUIAnchors(Transform canvasTransform)
    {
        // UI 이름 기반 앵커 자동 설정
        RectTransform[] allUI = canvasTransform.GetComponentsInChildren<RectTransform>(true);

        foreach (RectTransform rt in allUI)
        {
            // Canvas 자체는 건너뛰기
            if (rt == canvasTransform) continue;

            string lowerName = rt.name.ToLower();

            // 이름에 따른 앵커 설정
            if (lowerName.Contains("top") || lowerName.Contains("header") || lowerName.Contains("timer"))
            {
                // 상단 고정
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
            }
            else if (lowerName.Contains("bottom") || lowerName.Contains("footer"))
            {
                // 하단 고정
                rt.anchorMin = new Vector2(0.5f, 0f);
                rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
            }
            else if (lowerName.Contains("left"))
            {
                // 좌측 고정
                rt.anchorMin = new Vector2(0f, 0.5f);
                rt.anchorMax = new Vector2(0f, 0.5f);
                rt.pivot = new Vector2(0f, 0.5f);
            }
            else if (lowerName.Contains("right"))
            {
                // 우측 고정
                rt.anchorMin = new Vector2(1f, 0.5f);
                rt.anchorMax = new Vector2(1f, 0.5f);
                rt.pivot = new Vector2(1f, 0.5f);
            }
            else if (lowerName.Contains("corner"))
            {
                // 코너 UI 처리
                if (lowerName.Contains("topleft"))
                {
                    rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
                    rt.pivot = new Vector2(0f, 1f);
                }
                else if (lowerName.Contains("topright"))
                {
                    rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
                    rt.pivot = new Vector2(1f, 1f);
                }
                else if (lowerName.Contains("bottomleft"))
                {
                    rt.anchorMin = rt.anchorMax = new Vector2(0f, 0f);
                    rt.pivot = new Vector2(0f, 0f);
                }
                else if (lowerName.Contains("bottomright"))
                {
                    rt.anchorMin = rt.anchorMax = new Vector2(1f, 0f);
                    rt.pivot = new Vector2(1f, 0f);
                }
            }
        }
    }

    // 현재 게임 뷰포트의 월드 좌표 범위 가져오기
    public Bounds GetGameViewBounds()
    {
        float height = mainCamera.orthographicSize * 2f;
        float width = height * targetAspectRatio; // 항상 목표 비율 사용

        return new Bounds(mainCamera.transform.position, new Vector3(width, height, 0));
    }

    void OnGUI()
    {
        if (!showDebugInfo) return;
        if (!Application.isEditor) return;

        float currentRatio = (float)Screen.width / Screen.height;

        GUILayout.BeginArea(new Rect(10, 10, 300, 170));
        GUILayout.BeginVertical("box");
        GUILayout.Label("=== PC 가로모드 해상도 관리 ===");
        GUILayout.Label($"현재 해상도: {Screen.width}x{Screen.height}");
        GUILayout.Label($"현재 비율: {currentRatio:F2}");
        GUILayout.Label($"목표 비율: {targetAspectRatio:F2} (16:9)");
        GUILayout.Label($"카메라 Size: {mainCamera.orthographicSize:F2}");

        if (currentRatio < 1f)
        {
            GUI.color = Color.red;
            GUILayout.Label("⚠️ 세로 모드 감지!");
            GUILayout.Label("16:9 비율로 강제 조정 중");
            GUI.color = Color.white;
        }
        else if (currentRatio < targetAspectRatio)
        {
            GUI.color = Color.yellow;
            GUILayout.Label("상하 레터박스 표시 중");
            GUI.color = Color.white;
        }
        else if (currentRatio > targetAspectRatio)
        {
            GUI.color = Color.yellow;
            GUILayout.Label("좌우 레터박스 표시 중");
            GUI.color = Color.white;
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    [ContextMenu("Force Update All")]
    public void ForceUpdateAll()
    {
        FindAndSetupCanvases();
        UpdateResolution();
    }

    [ContextMenu("Debug - Show Canvas Info")]
    void DebugCanvasInfo()
    {
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        Debug.Log($"=== 캔버스 정보 ===");
        Debug.Log($"총 {allCanvases.Length}개의 Canvas 발견");

        foreach (Canvas canvas in allCanvases)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                Debug.Log($"Canvas: {canvas.name}");
                Debug.Log($"  - Render Mode: {canvas.renderMode}");
                Debug.Log($"  - UI Scale Mode: {scaler.uiScaleMode}");
                Debug.Log($"  - Reference Resolution: {scaler.referenceResolution}");
                Debug.Log($"  - Match: {scaler.matchWidthOrHeight}");
            }
        }
    }

    [ContextMenu("Fix UI Anchors for All Canvases")]
    public void FixAllUIAnchors()
    {
        Canvas[] allCanvases = FindObjectsOfType<Canvas>(true);

        foreach (Canvas canvas in allCanvases)
        {
            if (canvas.renderMode == RenderMode.WorldSpace) continue;

            // Canvas Scaler 확인 및 설정
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(referenceWidth, referenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            Debug.Log($"Canvas '{canvas.name}' 설정 완료");
        }

        Debug.Log("모든 UI 앵커 수정 완료!");
    }
}