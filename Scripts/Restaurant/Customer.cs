// Customer.cs
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class Customer : EmployeeBase
{
    [Header("대기·식사 시간 (초)")]
    public float orderWaitTime = 3f;
    public float eatTime = 4f;

    // 외부에서 참조할 수 있도록 프로퍼티 추가
    public int SeatIndex { get; private set; }
    public Recipe MyOrder { get; private set; }
    public bool HasOrdered { get; private set; }
    public bool HasBeenServed { get; private set; }

    private Vector2 exitPosition;

    // 애니메이션 컨트롤러 참조
    private CustomerAnimationController animController;

    protected override void Awake()
    {
        base.Awake();

        // 애니메이션 컨트롤러 참조
        animController = GetComponent<CustomerAnimationController>();
        if (animController == null)
        {
            animController = gameObject.AddComponent<CustomerAnimationController>();
        }
    }

    protected override void Start()
    {
        // 부모의 Start 호출 (2.5D 등록)
        base.Start();
    }

    // 좌석·퇴장 위치 할당
    public void AssignSeat(int index, Vector2 seatPos, Vector2 exitPos)
    {
        SeatIndex = index;
        exitPosition = exitPos;

        // 좌석으로 이동
        SetNewTarget(seatPos);
        OnArrived += OnArrivedAtSeat;
    }

    private void OnArrivedAtSeat()
    {
        OnArrived -= OnArrivedAtSeat;

        // 도착했을 때 idle 애니메이션
        if (animController != null)
        {
            animController.PlayAnimation(CustomerAnimationController.CustomerAnimState.Idle);

            // 좌석 인덱스에 따라 방향 설정
            animController.SetDirectionBySeatIndex(SeatIndex);
        }

        StartCoroutine(OrderRoutine());
    }

    private IEnumerator OrderRoutine()
    {
        yield return new WaitForSeconds(orderWaitTime);

        // 레시피 선택
        var list = OrderManager.Instance.possibleRecipes;
        MyOrder = list[UnityEngine.Random.Range(0, list.Count)]; // UnityEngine.Random 명시
        HasOrdered = true;

        // 주문 생성 (고객 자신을 함께 넘김)
        OrderManager.Instance.CreateOrder(MyOrder, this);
    }

    // 홀 직원이 호출
    public void Serve(Recipe dish)
    {
        if (!HasOrdered || HasBeenServed || dish != MyOrder)
            return;

        HasBeenServed = true;

        // 전표 SoldOut 처리 및 제거
        OrderManager.Instance.MarkOrderComplete(dish);

        // DecrementAssignedCount 호출
        RestaurantManager.Instance.DecrementAssignedCount(dish);

        // 식사 애니메이션 재생
        if (animController != null)
        {
            animController.PlayAnimation(CustomerAnimationController.CustomerAnimState.Eating);
        }

        // 식사 -> 결제 애니메이션
        StartCoroutine(EatAndExit());
    }

    private IEnumerator EatAndExit()
    {
        // 식사 대기
        yield return new WaitForSeconds(eatTime);

        // 결제: 레시피에 설정된 가격만큼 머니 추가
        if (MyOrder != null)
            MoneyManager.Instance.AddMoney(MyOrder.price);

        // 좌석 해제
        if (SeatIndex >= 0)
            CustomerManager.Instance.FreeSeat(SeatIndex);

        // 애니메이션 컨트롤러에게 퇴장 준비를 알림
        if (animController != null)
        {
            animController.PrepareForExit();
        }

        // 퇴장으로 이동 (walk 애니메이션은 CustomerAnimationController에서 처리)
        SetNewTarget(exitPosition);
        OnArrived += OnArrivedAtExit;
    }

    private void OnArrivedAtExit()
    {
        OnArrived -= OnArrivedAtExit;
        Destroy(gameObject);
    }

    // SetNewTarget 메서드 오버라이드하여 애니메이션 처리 추가
    public override void SetNewTarget(Vector2 pos)
    {
        // 부모의 기본 로직 호출
        base.SetNewTarget(pos);

        // 애니메이션 컨트롤러가 있다면 walk 애니메이션 재생
        if (animController != null && isMoving)
        {
            animController.PlayAnimation(CustomerAnimationController.CustomerAnimState.Walking);
        }
    }
}