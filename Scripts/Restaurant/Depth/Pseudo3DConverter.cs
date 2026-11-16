using UnityEngine;
using System.Collections.Generic;
using Spine.Unity;

public class Pseudo3DConverter : MonoBehaviour
{
    public static Pseudo3DConverter Instance { get; private set; }

    [Header("2.5D 핵심 설정")]
    [SerializeField] private bool enablePseudo3D = true;
    [SerializeField] private float depthScale = 100f;     // Y좌표를 sortingOrder로 변환하는 스케일
    [SerializeField] private int baseOrder = 10000;       // 기본 sortingOrder (음수 방지)

    [Header("카메라 설정")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private bool autoSetupCamera = true;
    [SerializeField] private float cameraAngle = 0f;

    [Header("깊이 스케일링")]
    [SerializeField] private bool enableDepthScaling = false; // 기본값을 false로 변경
    [SerializeField] private float scaleMultiplier = 0.01f;
    [SerializeField] private float minScale = 0.8f;
    [SerializeField] private float maxScale = 1.2f;

    [Header("디버그")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private KeyCode debugKey = KeyCode.D;

    private Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>();
    private List<Transform> registeredObjects = new List<Transform>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (autoSetupCamera)
        {
            SetupCameraFor2_5D();
        }

        if (enablePseudo3D)
        {
            ConvertAllTo2_5D();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(debugKey))
        {
            ToggleDebugInfo();
        }

        // 등록된 오브젝트들의 깊이 업데이트
        if (enablePseudo3D)
        {
            UpdateAllDepths();
        }
    }

    void SetupCameraFor2_5D()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera != null)
        {
            // Orthographic 카메라로 설정
            targetCamera.orthographic = true;

            // 카메라 회전
            if (cameraAngle != 0)
            {
                targetCamera.transform.rotation = Quaternion.Euler(cameraAngle, 0, 0);
            }

            // TransparencySortMode를 Default로 설정 (sortingOrder 우선)
            targetCamera.transparencySortMode = TransparencySortMode.Default;

            Debug.Log($"카메라가 2.5D 모드로 설정되었습니다. SortingOrder 기반 정렬 활성화");
        }
    }

    void ConvertAllTo2_5D()
    {
        // 모든 SpriteRenderer 오브젝트 찾기
        SpriteRenderer[] allSpriteRenderers = FindObjectsOfType<SpriteRenderer>(true);
        foreach (var renderer in allSpriteRenderers)
        {
            RegisterAndConvertObject(renderer.transform);
        }

        // 모든 SkeletonAnimation (Spine) 오브젝트 찾기
        SkeletonAnimation[] allSkeletonAnimations = FindObjectsOfType<SkeletonAnimation>(true);
        foreach (var skeletonAnim in allSkeletonAnimations)
        {
            RegisterAndConvertObject(skeletonAnim.transform);
        }

        // 모든 SkeletonGraphic (UI Spine) 오브젝트 찾기
        SkeletonGraphic[] allSkeletonGraphics = FindObjectsOfType<SkeletonGraphic>(true);
        foreach (var skeletonGraphic in allSkeletonGraphics)
        {
            RegisterAndConvertObject(skeletonGraphic.transform);
        }

        Debug.Log($"{registeredObjects.Count}개 오브젝트가 2.5D로 변환되었습니다.");
    }

    public void RegisterAndConvertObject(Transform objTransform)
    {
        if (objTransform == null) return;

        // 이미 등록된 오브젝트인지 확인
        if (registeredObjects.Contains(objTransform))
        {
            UpdateObjectDepth(objTransform);
            return;
        }

        // 원본 스케일 저장
        if (!originalScales.ContainsKey(objTransform))
        {
            originalScales[objTransform] = objTransform.localScale;
        }

        // 등록
        registeredObjects.Add(objTransform);

        // 2.5D 변환
        ConvertObjectTo2_5D(objTransform.gameObject);

        Debug.Log($"2.5D 등록: {objTransform.name} (Type: {GetRendererType(objTransform.gameObject)})");
    }

    string GetRendererType(GameObject obj)
    {
        if (obj.GetComponent<SpriteRenderer>() != null) return "Sprite";
        if (obj.GetComponent<SkeletonAnimation>() != null) return "Spine";
        if (obj.GetComponent<SkeletonGraphic>() != null) return "SpineUI";
        if (obj.GetComponent<MeshRenderer>() != null) return "Mesh";
        return "Unknown";
    }

    void ConvertObjectTo2_5D(GameObject obj)
    {
        UpdateObjectDepth(obj.transform);
    }

    // Y좌표 기반 sortingOrder 설정
    public void UpdateObjectDepth(Transform objTransform)
    {
        if (!enablePseudo3D || objTransform == null) return;

        // Z축은 0으로 유지
        Vector3 pos = objTransform.position;
        objTransform.position = new Vector3(pos.x, pos.y, 0);

        // Y 위치에 따른 sortingOrder 계산 (Y가 낮을수록 앞에 렌더링)
        int sortOrder = baseOrder - Mathf.RoundToInt(pos.y * depthScale);

        // SpriteRenderer 처리
        SpriteRenderer[] spriteRenderers = objTransform.GetComponentsInChildren<SpriteRenderer>();
        foreach (var renderer in spriteRenderers)
        {
            renderer.sortingLayerName = "Default";
            renderer.sortingOrder = sortOrder;
        }

        // Spine MeshRenderer 처리
        SkeletonAnimation skeletonAnim = objTransform.GetComponent<SkeletonAnimation>();
        if (skeletonAnim != null)
        {
            MeshRenderer meshRenderer = skeletonAnim.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.sortingLayerName = "Default";
                meshRenderer.sortingOrder = sortOrder;
            }
        }

        // 깊이에 따른 스케일 조정
        if (enableDepthScaling)
        {
            UpdateObjectScale(objTransform, pos.y);
        }
    }

    void UpdateObjectScale(Transform objTransform, float yPosition)
    {
        if (!originalScales.ContainsKey(objTransform)) return;

        Vector3 originalScale = originalScales[objTransform];

        // Y 위치에 따른 스케일 계산
        float scaleModifier = 1.0f + (yPosition * -scaleMultiplier);
        scaleModifier = Mathf.Clamp(scaleModifier, minScale, maxScale);

        objTransform.localScale = originalScale * scaleModifier;
    }

    void UpdateAllDepths()
    {
        // null 체크 및 정리
        for (int i = registeredObjects.Count - 1; i >= 0; i--)
        {
            if (registeredObjects[i] == null)
            {
                registeredObjects.RemoveAt(i);
                continue;
            }

            UpdateObjectDepth(registeredObjects[i]);
        }
    }

    // 새로 생성되는 오브젝트를 위한 메서드
    public void OnObjectCreated(GameObject obj)
    {
        if (enablePseudo3D)
        {
            RegisterAndConvertObject(obj.transform);
        }
    }

    // 캐릭터 이동 시 호출될 메서드
    public void OnCharacterMoved(Transform character)
    {
        if (enablePseudo3D)
        {
            UpdateObjectDepth(character);
        }
    }

    void ToggleDebugInfo()
    {
        showDebugGizmos = !showDebugGizmos;
        Debug.Log($"2.5D 디버그 모드: {showDebugGizmos}");

        // 현재 등록된 오브젝트 상태 출력
        Debug.Log($"등록된 오브젝트 수: {registeredObjects.Count}");
        foreach (var obj in registeredObjects)
        {
            if (obj != null)
            {
                Vector3 pos = obj.position;
                string type = GetRendererType(obj.gameObject);

                // 현재 sortingOrder 확인
                int currentOrder = 0;
                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    currentOrder = sr.sortingOrder;
                }
                else
                {
                    SkeletonAnimation sa = obj.GetComponent<SkeletonAnimation>();
                    if (sa != null)
                    {
                        MeshRenderer mr = sa.GetComponent<MeshRenderer>();
                        if (mr != null) currentOrder = mr.sortingOrder;
                    }
                }

                Debug.Log($"{obj.name} [{type}] - Y: {pos.y:F2}, SortingOrder: {currentOrder}, Scale: {obj.localScale.x:F2}");
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!showDebugGizmos || !enablePseudo3D) return;

        // Y축 가이드라인 그리기
        Gizmos.color = Color.green;
        for (int i = -10; i <= 10; i++)
        {
            float y = i;
            Vector3 start = new Vector3(-20, y, 0);
            Vector3 end = new Vector3(20, y, 0);
            Gizmos.DrawLine(start, end);

            // sortingOrder 값 표시
            int order = baseOrder - Mathf.RoundToInt(y * depthScale);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(new Vector3(-21, y, 0), $"Y:{y} Order:{order}");
#endif
        }

        // 등록된 오브젝트들 위치 표시
        foreach (var obj in registeredObjects)
        {
            if (obj != null)
            {
                // 캐릭터 타입에 따라 다른 색상
                if (obj.GetComponent<Customer>() != null)
                    Gizmos.color = Color.yellow;
                else if (obj.GetComponent<HallEmployee>() != null)
                    Gizmos.color = Color.magenta;
                else if (obj.GetComponent<KitchenEmployee>() != null)
                    Gizmos.color = Color.cyan;
                else
                    Gizmos.color = Color.white;

                Gizmos.DrawWireSphere(obj.position, 0.2f);

                // Y 위치와 sortingOrder 표시
#if UNITY_EDITOR
                int order = baseOrder - Mathf.RoundToInt(obj.position.y * depthScale);
                UnityEditor.Handles.Label(obj.position + Vector3.up * 0.5f, $"Y:{obj.position.y:F1}\nOrder:{order}");
#endif
            }
        }
    }

    [ContextMenu("Convert All to 2.5D")]
    void ConvertAllManual()
    {
        enablePseudo3D = true;
        ConvertAllTo2_5D();
    }

    [ContextMenu("Reset All to 2D")]
    void ResetAllTo2D()
    {
        enablePseudo3D = false;

        foreach (var obj in registeredObjects)
        {
            if (obj != null)
            {
                Vector3 pos = obj.position;
                obj.position = new Vector3(pos.x, pos.y, 0);

                // 원본 스케일 복원
                if (originalScales.ContainsKey(obj))
                {
                    obj.localScale = originalScales[obj];
                }
            }
        }

        registeredObjects.Clear();
        originalScales.Clear();

        Debug.Log("모든 오브젝트가 2D로 리셋되었습니다.");
    }

    [ContextMenu("Force Update All Spine Characters")]
    public void ForceUpdateAllSpineCharacters()
    {
        SkeletonAnimation[] allSpines = FindObjectsOfType<SkeletonAnimation>();
        foreach (var spine in allSpines)
        {
            RegisterAndConvertObject(spine.transform);
            Debug.Log($"Spine 캐릭터 강제 업데이트: {spine.name}");
        }
    }
}