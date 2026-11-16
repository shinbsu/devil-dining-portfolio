using UnityEngine;
using Spine.Unity;

public class EmployeeSpawner : MonoBehaviour
{
    public ZoneArea kitchenZone;
    public ZoneArea hallZone;
    public GameObject kitchenEmployeePrefab;
    public GameObject hallEmployeePrefab;

    [Header("스폰 개수")]
    public int kitchenCount = 3;
    public int hallCount = 2;

    [Header("주방 직원 스켈레톤 데이터")]
    [Tooltip("주방 직원에게 랜덤 적용할 스켈레톤 데이터")]
    public SkeletonDataAsset[] kitchenEmployeeSkeletons;

    [Header("홀 직원 스켈레톤 데이터")]
    [Tooltip("홀 직원에게 랜덤 적용할 스켈레톤 데이터")]
    public SkeletonDataAsset[] hallEmployeeSkeletons;

    private void Start()
    {
        SpawnWithRandomSkeletons(kitchenZone, kitchenEmployeePrefab, kitchenCount, kitchenEmployeeSkeletons);
        SpawnWithRandomSkeletons(hallZone, hallEmployeePrefab, hallCount, hallEmployeeSkeletons);
    }

    private void SpawnWithRandomSkeletons(ZoneArea zone, GameObject prefab, int count, SkeletonDataAsset[] skeletonDataAssets)
    {
        for (int i = 0; i < count; i++)
        {
            // 해당 Zone 안의 랜덤 위치에 스폰 (기존 로직 유지)
            Vector2 pos = zone.GetRandomPointInside();
            GameObject go = Instantiate(prefab, pos, Quaternion.identity, transform);

            // 랜덤 스켈레톤 데이터 적용 (기존 로직 유지)
            if (skeletonDataAssets != null && skeletonDataAssets.Length > 0)
            {
                var skeletonAnim = go.GetComponent<SkeletonAnimation>() ?? go.GetComponentInChildren<SkeletonAnimation>();
                if (skeletonAnim != null)
                {
                    try
                    {
                        // 랜덤 스켈레톤 데이터 선택
                        int randomIndex = Random.Range(0, skeletonDataAssets.Length);
                        SkeletonDataAsset randomData = skeletonDataAssets[randomIndex];

                        // 스켈레톤 데이터 적용
                        skeletonAnim.skeletonDataAsset = randomData;
                        skeletonAnim.Initialize(true);

                        // 초기 애니메이션 설정 (idle)
                        skeletonAnim.AnimationState.SetAnimation(0, "idle", true);

                        Debug.Log($"직원 {go.name}에게 스켈레톤 데이터 적용: {randomData.name}");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"스켈레톤 데이터 적용 오류: {e.Message}");
                    }
                }
            }

            // *** 2.5D 등록 추가 (새로운 부분) ***
            if (Pseudo3DConverter.Instance != null)
            {
                Pseudo3DConverter.Instance.OnObjectCreated(go);
            }

            // EmployeeBase 컴포넌트 가져오기 (기존 로직 유지)
            EmployeeBase empBase = go.GetComponent<EmployeeBase>();
            if (empBase == null)
            {
                Debug.LogError($"{prefab.name}에 EmployeeBase 컴포넌트가 없음");
                continue;
            }

            // Zone 타입에 따라 myZone 할당 (기존 로직 유지)
            if (empBase is KitchenEmployee kitchenEmp)
            {
                kitchenEmp.myZone = kitchenZone;
            }
            else if (empBase is HallEmployee hallEmp)
            {
                hallEmp.myZone = hallZone;
            }
            else
            {
                Debug.LogWarning($"알 수 없는 직원 타입: {empBase.GetType().Name}");
            }
        }
    }
}