using UnityEngine;
using System.Collections;
using Spine.Unity;

[RequireComponent(typeof(Collider2D))]
public class KitchenEmployee : EmployeeBase
{
    [HideInInspector] public ZoneArea myZone;

    [Header("순찰 설정")]
    public float patrolWait = 1.5f;      // 한 지점에서 대기하는 시간
    public float idleWaitMin = 2.0f;     // 최소 대기 시간
    public float idleWaitMax = 4.0f;     // 최대 대기 시간

    // 현재 상태
    [HideInInspector] public EmployeeState state = EmployeeState.Idle;
    public EmployeeState State => state;

    private Recipe currentRecipe;
    private CookingStation currentStation;
    private ServingStation servingStation;

    // 요리 애니메이션 변경 주기
    private float cookAnimSwitchTime = 3f;
    private float cookAnimTimer = 0f;

    // 애니메이션 컨트롤러 참조 추가
    private EmployeeAnimationController animController;

    // 현재 순찰 코루틴 관리용
    private Coroutine patrolCoroutine;

    protected override void Awake()
    {
        base.Awake();

        // 애니메이션 컨트롤러 참조
        animController = GetComponent<EmployeeAnimationController>();
        if (animController == null)
        {
            animController = gameObject.AddComponent<EmployeeAnimationController>();
        }

        // 범위 확인 및 조정
        if (idleWaitMin > idleWaitMax)
            idleWaitMin = idleWaitMax;
    }

    private void Start()
    {
        // 선반 참조
        servingStation = FindObjectOfType<ServingStation>();
        // 첫 순찰 시작
        StartPatrol();
    }

    // SetNewTarget 메서드 오버라이드
    public new void SetNewTarget(Vector2 pos)
    {
        // 기본 이동 로직 실행
        base.SetNewTarget(pos);

        // 애니메이션 설정 - 이동 중일 때만 walking
        if (animController != null && isMoving)
        {
            animController.PlayAnimation(EmployeeAnimationController.EmployeeAnimState.Walking);
        }
    }

    private void StartPatrol()
    {
        state = EmployeeState.Idle;

        // 이전 코루틴 정지
        if (patrolCoroutine != null)
            StopCoroutine(patrolCoroutine);

        // 새 순찰 코루틴 시작
        patrolCoroutine = StartCoroutine(PatrolRoutine());
    }

    private IEnumerator PatrolRoutine()
    {
        while (state == EmployeeState.Idle)
        {
            // 1. Idle 상태로 대기 (애니메이션)
            if (animController != null)
            {
                animController.PlayAnimation(EmployeeAnimationController.EmployeeAnimState.Idle);
            }

            // 랜덤 시간 대기
            float waitTime = UnityEngine.Random.Range(idleWaitMin, idleWaitMax);
            yield return new WaitForSeconds(waitTime);

            // 대기 후에도 Idle 상태인지 확인
            if (state != EmployeeState.Idle) break;

            // 2. 랜덤 좌표로 이동 (Walk 애니메이션은 SetNewTarget에서 처리)
            SetNewTarget(myZone.GetRandomPointInside());

            // 3. 이동 완료될 때까지 대기
            yield return new WaitUntil(() => !isMoving);

            // 이동 후 잠시 대기
            yield return new WaitForSeconds(patrolWait);
        }
    }

    protected override void Update()
    {
        base.Update();

        // Assigned 상태에서, 작업대에 완전히 도착했을 때만 요리 시작
        if (state == EmployeeState.Assigned && !isMoving)
        {
            state = EmployeeState.Working;

            // 요리 애니메이션 재생 - 랜덤으로 cooking 또는 cooking2
            if (animController != null)
            {
                animController.PlayRandomCookingAnim();
            }

            cookAnimTimer = 0f;

            StartCoroutine(CookRoutine());
        }

        // Working 상태일 때 주기적으로 요리 애니메이션 전환
        if (state == EmployeeState.Working && animController != null)
        {
            cookAnimTimer += Time.deltaTime;
            if (cookAnimTimer >= cookAnimSwitchTime)
            {
                cookAnimTimer = 0f;
                animController.PlayRandomCookingAnim();
            }
        }
    }

    private IEnumerator CookRoutine()
    {
        // 재료 차감
        InventoryManager.Instance.ConsumeIngredients(currentRecipe);

        // 실제 조리 시간 대기
        yield return new WaitForSeconds(currentRecipe.cookingTime);

        // 요리 완료 후 선반(dropOffPosition)으로 이동
        Vector2 dropPos = currentStation.GetDropOffPosition();
        SetNewTarget(dropPos);
        state = EmployeeState.Returning;

        // 선반(dropOffPosition)에 완전히 도착할 때까지 대기
        yield return new WaitUntil(() => !isMoving);

        // 선반에 요리 추가
        servingStation.AddDish(currentRecipe);

        // 작업대 예약 해제
        currentStation.Release();

        // 변수 초기화 후 다시 순찰 상태로
        currentRecipe = null;
        currentStation = null;
        StartPatrol();
    }

    /// <summary>
    /// RestaurantManager에서 호출: Idle 상태일 때만 성공
    /// </summary>
    public bool AssignCooking(Recipe r, CookingStation station)
    {
        if (state != EmployeeState.Idle) return false;
        if (!station.Reserve()) return false;

        // 순찰 코루틴 정지
        if (patrolCoroutine != null)
            StopCoroutine(patrolCoroutine);

        currentRecipe = r;
        currentStation = station;
        SetNewTarget(station.GetWorkPosition());
        state = EmployeeState.Assigned;
        return true;
    }
}