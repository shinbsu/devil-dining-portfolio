using UnityEngine;
using Spine.Unity; // Spine 네임스페이스 추가

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance { get; private set; }

    [Header("손님 스폰 & 퇴장 지점")]
    public Transform entrancePoint;    // 홀 우측 입구 위치
    public Transform exitPoint;        // 같은 위치나 별도 퇴장 위치

    [Header("좌석 위치들 (빈 배열 크기 = 좌석 수)")]
    public Transform[] seatPoints;

    [Header("손님 프리팹 & 스폰 간격")]
    public Customer customerPrefab;
    public float spawnInterval = 5f;

    [Header("다양한 스켈레톤 데이터")]
    public SkeletonDataAsset[] customerSkeletonData;

    private bool[] seatOccupied;
    private float timer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        seatOccupied = new bool[seatPoints.Length];
    }

    private void Update()
    {
        timer += Time.deltaTime;
        // 빈 좌석 남아 있고, 일정 시간 지났다면 스폰
        if (timer >= spawnInterval)
        {
            timer = 0f;
            TrySpawnCustomer();
        }
    }

    private void TrySpawnCustomer()
    {
        for (int i = 0; i < seatPoints.Length; i++)
        {
            if (!seatOccupied[i])
            {
                SpawnCustomer(i);
                return;
            }
        }
    }

    private void SpawnCustomer(int seatIndex)
    {
        seatOccupied[seatIndex] = true;

        // 입구 위치에서 스폰 (기존 로직 유지)
        var go = Instantiate(customerPrefab.gameObject, entrancePoint.position, Quaternion.identity, transform);

        // 랜덤 스켈레톤 데이터 적용 (기존 로직 유지)
        if (customerSkeletonData != null && customerSkeletonData.Length > 0)
        {
            var skeletonAnim = go.GetComponent<SkeletonAnimation>() ?? go.GetComponentInChildren<SkeletonAnimation>();
            if (skeletonAnim != null)
            {
                int randomIndex = Random.Range(0, customerSkeletonData.Length);
                skeletonAnim.skeletonDataAsset = customerSkeletonData[randomIndex];
                skeletonAnim.Initialize(true);

                // 기본 애니메이션 재설정
                skeletonAnim.AnimationState.SetAnimation(0, "sit_idle", true);
                Debug.Log($"손님에게 스켈레톤 데이터 적용: {customerSkeletonData[randomIndex].name}");
            }
        }

        // *** 2.5D 등록 추가 (새로운 부분) ***
        if (Pseudo3DConverter.Instance != null)
        {
            Pseudo3DConverter.Instance.OnObjectCreated(go);
        }

        // 좌석 및 퇴장 지점 할당 (기존 로직 유지)
        var cust = go.GetComponent<Customer>();
        cust.AssignSeat(seatIndex, seatPoints[seatIndex].position, exitPoint.position);
    }

    // 고객이 퇴장할 때 호출
    public void FreeSeat(int seatIndex)
    {
        if (seatIndex >= 0 && seatIndex < seatOccupied.Length)
            seatOccupied[seatIndex] = false;
    }
}