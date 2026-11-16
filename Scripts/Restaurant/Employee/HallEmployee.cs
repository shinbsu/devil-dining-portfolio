using UnityEngine;
using System.Collections;
using Spine.Unity;

[RequireComponent(typeof(Collider2D))]
public class HallEmployee : EmployeeBase
{
    public enum State
    {
        Patrol,
        AssignedToHallDoor,
        AssignedToPickupPoint,
        AssignedToKitchenDoor,
        AssignedToCustomer
    }

    [HideInInspector] public State state = State.Patrol;
    public State CurrentState => state;

    [Header("구역 & 문 Transform 할당")]
    public ZoneArea myZone;
    public Transform hallToKitchenDoor;
    public Transform kitchenToHallDoor;

    [Header("대기 시간 설정")]
    public float servingAnimTime = 0.8f;
    public float pickupWait = 0.5f;
    public float idleWaitMin = 2.0f;
    public float idleWaitMax = 4.0f;

    private Recipe currentDish;
    private Transform targetCustomer;
    private ServingStation servingStation;
    private bool overrideWalkAnimation = false;

    private EmployeeAnimationController animController;
    private Coroutine patrolCoroutine;

    protected override void Awake()
    {
        base.Awake();

        animController = GetComponent<EmployeeAnimationController>();
        if (animController == null)
        {
            animController = gameObject.AddComponent<EmployeeAnimationController>();
        }

        if (idleWaitMin > idleWaitMax)
            idleWaitMin = idleWaitMax;
    }

    protected override void Start()
    {
        base.Start();  // 이미 EmployeeBase에서 2.5D 등록을 처리함

        servingStation = FindObjectOfType<ServingStation>();
        if (hallToKitchenDoor == null || kitchenToHallDoor == null)
            Debug.LogError("HallEmployee: Door Transforms not 할당!");

        StartPatrol();
    }

    public override void SetNewTarget(Vector2 pos)
    {
        targetPos = pos;
        isMoving = true;

        if (animController != null)
        {
            if (overrideWalkAnimation)
            {
                animController.PlayAnimation(EmployeeAnimationController.EmployeeAnimState.Serving);
            }
            else
            {
                animController.PlayAnimation(EmployeeAnimationController.EmployeeAnimState.Walking);
            }
        }
    }

    private void StartPatrol()
    {
        state = State.Patrol;

        if (patrolCoroutine != null)
            StopCoroutine(patrolCoroutine);

        patrolCoroutine = StartCoroutine(PatrolRoutine());
    }

    private IEnumerator PatrolRoutine()
    {
        while (state == State.Patrol)
        {
            if (animController != null)
            {
                animController.PlayAnimation(EmployeeAnimationController.EmployeeAnimState.Idle);
            }

            float waitTime = UnityEngine.Random.Range(idleWaitMin, idleWaitMax);
            yield return new WaitForSeconds(waitTime);

            if (state != State.Patrol) break;

            SetNewTarget(myZone.GetRandomPointInside());
            yield return new WaitUntil(() => !isMoving);
        }
    }

    protected override void Update()
    {
        if (!isMoving)
        {
            HandleStateMachine();
        }
        else
        {
            // EmployeeBase의 Update가 이미 이동과 2.5D 업데이트를 처리하므로
            // 여기서는 추가 처리만 수행
            base.Update();

            // 서빙 애니메이션 유지
            if (overrideWalkAnimation && animController != null)
            {
                animController.PlayAnimation(EmployeeAnimationController.EmployeeAnimState.Serving);
            }
        }
    }

    private void HandleStateMachine()
    {
        switch (state)
        {
            case State.AssignedToHallDoor:
                // 순간이동 로직
                Vector3 doorPos = kitchenToHallDoor.position;
                transform.position = new Vector3(doorPos.x, doorPos.y, transform.position.z);

                // 순간이동 후 2.5D 업데이트 (EmployeeBase와 동일한 방식)
                if (Pseudo3DConverter.Instance != null)
                {
                    Pseudo3DConverter.Instance.OnCharacterMoved(transform);
                }

                SetNewTarget(servingStation.GetPickupPosition());
                state = State.AssignedToPickupPoint;
                break;

            case State.AssignedToPickupPoint:
                if (animController != null)
                {
                    animController.PlayAnimation(EmployeeAnimationController.EmployeeAnimState.Serving);
                }
                StartCoroutine(PickupRoutine());
                break;

            case State.AssignedToKitchenDoor:
                // 순간이동 로직
                Vector3 hallDoorPos = hallToKitchenDoor.position;
                transform.position = new Vector3(hallDoorPos.x, hallDoorPos.y, transform.position.z);

                // 순간이동 후 2.5D 업데이트
                if (Pseudo3DConverter.Instance != null)
                {
                    Pseudo3DConverter.Instance.OnCharacterMoved(transform);
                }

                overrideWalkAnimation = true;

                if (animController != null)
                {
                    animController.PlayAnimation(EmployeeAnimationController.EmployeeAnimState.Serving);
                }

                if (targetCustomer != null)
                {
                    SetNewTarget(targetCustomer.position);
                }
                state = State.AssignedToCustomer;
                break;

            case State.AssignedToCustomer:
                if (targetCustomer == null)
                {
                    StartPatrol();
                    break;
                }

                if (animController != null)
                {
                    animController.PlayAnimation(EmployeeAnimationController.EmployeeAnimState.Serving);
                }

                var cust = targetCustomer.GetComponent<Customer>();
                if (cust != null && cust.HasOrdered && !cust.HasBeenServed && cust.MyOrder == currentDish)
                {
                    cust.Serve(currentDish);
                }

                StartCoroutine(WaitAndReturnToPatrol());
                break;
        }
    }

    private IEnumerator WaitAndReturnToPatrol()
    {
        yield return new WaitForSeconds(servingAnimTime);
        overrideWalkAnimation = false;
        StartPatrol();
    }

    IEnumerator PickupRoutine()
    {
        yield return new WaitForSeconds(pickupWait);
        overrideWalkAnimation = true;

        if (animController != null)
        {
            animController.PlayAnimation(EmployeeAnimationController.EmployeeAnimState.Serving);
        }

        SetNewTarget(kitchenToHallDoor.position);
        state = State.AssignedToKitchenDoor;
    }

    public bool AssignServing(Recipe dish, Customer customer)
    {
        if (state != State.Patrol) return false;
        if (!servingStation.RemoveSpecificDish(dish)) return false;

        if (patrolCoroutine != null)
            StopCoroutine(patrolCoroutine);

        currentDish = dish;
        targetCustomer = customer.transform;
        SetNewTarget(hallToKitchenDoor.position);
        state = State.AssignedToHallDoor;
        return true;
    }
}