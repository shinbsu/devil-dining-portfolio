using UnityEngine;

public class ResolutionTestTool : MonoBehaviour
{
    [Header("테스트할 해상도들")]
    [SerializeField]
    private ResolutionPreset[] testResolutions = new ResolutionPreset[]
    {
        new ResolutionPreset("16:9 - FHD", 1920, 1080),
        new ResolutionPreset("16:9 - HD", 1280, 720),
        new ResolutionPreset("16:10", 1920, 1200),
        new ResolutionPreset("4:3", 1024, 768),
        new ResolutionPreset("21:9 울트라와이드", 2560, 1080),
        new ResolutionPreset("19.5:9 iPhone X", 2436, 1125),
        new ResolutionPreset("18:9 Galaxy S8", 2960, 1440),
        new ResolutionPreset("iPad", 2048, 1536)
    };

    [System.Serializable]
    public class ResolutionPreset
    {
        public string name;
        public int width;
        public int height;

        public ResolutionPreset(string n, int w, int h)
        {
            name = n;
            width = w;
            height = h;
        }
    }

    void OnGUI()
    {
        // 에디터에서만 작동
#if UNITY_EDITOR
        GUILayout.BeginArea(new Rect(Screen.width - 310, 10, 300, 400));
        GUILayout.BeginVertical("box");
        GUILayout.Label("=== 해상도 테스트 도구 ===");

        GUILayout.Label($"현재: {Screen.width}x{Screen.height} ({GetAspectRatioText()})");
        GUILayout.Space(10);

        foreach (var res in testResolutions)
        {
            if (GUILayout.Button($"{res.name} ({res.width}x{res.height})"))
            {
                SetGameViewResolution(res.width, res.height);
            }
        }

        GUILayout.Space(10);
        if (GUILayout.Button("게임 뷰 크기 리셋"))
        {
            SetGameViewResolution(1920, 1080);
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();
#endif
    }

    string GetAspectRatioText()
    {
        float ratio = (float)Screen.width / Screen.height;

        if (Mathf.Abs(ratio - 16f / 9f) < 0.01f) return "16:9";
        if (Mathf.Abs(ratio - 16f / 10f) < 0.01f) return "16:10";
        if (Mathf.Abs(ratio - 4f / 3f) < 0.01f) return "4:3";
        if (Mathf.Abs(ratio - 21f / 9f) < 0.01f) return "21:9";

        return $"{ratio:F2}:1";
    }

    void SetGameViewResolution(int width, int height)
    {
#if UNITY_EDITOR
        System.Type gameViewType = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        if (gameViewType != null)
        {
            var gameView = UnityEditor.EditorWindow.GetWindow(gameViewType);
            if (gameView != null)
            {
                var prop = gameViewType.GetProperty("currentGameViewSize",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (prop != null)
                {
                    var gvsize = prop.GetValue(gameView, null);
                    var gvSizeType = gvsize.GetType();

                    gvSizeType.GetProperty("width", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                        .SetValue(gvsize, width, null);
                    gvSizeType.GetProperty("height", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                        .SetValue(gvsize, height, null);
                }
            }
        }

        Debug.Log($"게임 뷰 해상도 변경 시도: {width}x{height}");
#endif
    }
}