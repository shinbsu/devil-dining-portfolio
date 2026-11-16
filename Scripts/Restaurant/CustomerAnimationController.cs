using Spine.Unity;
using UnityEngine;
using System.Collections;

public class CustomerAnimationController : MonoBehaviour
{
    // Spine 애니메이션 컴포넌트
    private SkeletonAnimation skeletonAnimation;

    // 현재 애니메이션 상태
    private string currentAnimation = "idle";
    private bool isAnimationLooping = true;

    // 고객 컴포넌트 참조
    private Customer customer;

    // 마지막 위치 저장 (이동 방향 계산용)
    private Vector2 lastPosition;

    // 좌석에 앉았는지 여부 (방향 업데이트 제어용)
    private bool isSeated = false;

    // 애니메이션 상태 열거형
    public enum CustomerAnimState
    {
        Idle,
        Walking,
        Eating
    }

    private void Awake()
    {
        // Spine 애니메이션 컴포넌트 가져오기
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        if (skeletonAnimation == null)
        {
            skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
        }

        if (skeletonAnimation == null)
        {
            Debug.LogError($"[{gameObject.name}] SkeletonAnimation 컴포넌트가 없습니다!");
            return;
        }

        // Customer 컴포넌트 참조
        customer = GetComponent<Customer>();
        if (customer == null)
        {
            Debug.LogError($"[{gameObject.name}] Customer 컴포넌트가 없습니다!");
            return;
        }

        // 초기 위치 저장
        lastPosition = transform.position;

        // 초기 애니메이션 설정
        PlayAnimation(CustomerAnimState.Idle);
    }

    private void Update()
    {
        if (skeletonAnimation == null) return;

        // 이미 좌석에 앉아있다면 방향 업데이트를 건너뜀
        if (isSeated) return;

        // 위치가 변경되었는지 확인하여 이동 여부 판단
        Vector2 currentPosition = transform.position;
        float moveDistance = Vector2.Distance(lastPosition, currentPosition);

        // 위치 변경이 있으면 walking 애니메이션 (최소 거리 임계값 줄임)
        if (moveDistance > 0.001f) // 더 작은 값으로 변경하여 미세한 움직임도 감지
        {
            // Walk 애니메이션이 아니면 설정
            if (currentAnimation != "walk")
            {
                PlayAnimation(CustomerAnimState.Walking);
            }

            // 이동 방향에 따른 좌우 반전 처리
            float xDelta = currentPosition.x - lastPosition.x;
            UpdateFlipX(xDelta);
        }
        else if (currentAnimation == "walk")
        {
            // 이동이 멈추면 idle 애니메이션
            PlayAnimation(CustomerAnimState.Idle);
        }

        // 현재 위치 저장
        lastPosition = currentPosition;
    }

    // 외부에서 호출할 수 있는 애니메이션 재생 메서드
    public void PlayAnimation(CustomerAnimState animState, float timeScale = 1f)
    {
        if (skeletonAnimation == null) return;

        string animationName;
        bool loop = true;

        // 상태에 따른 애니메이션 이름 결정
        switch (animState)
        {
            case CustomerAnimState.Walking:
                animationName = "walk";
                loop = true;
                break;
            case CustomerAnimState.Eating:
                animationName = "eating";
                loop = true;
                break;
            case CustomerAnimState.Idle:
            default:
                animationName = "sit_idle"; // "idle"에서 "sit_idle"로 변경
                loop = true;
                break;
        }

        // 같은 애니메이션이면 무시
        if (currentAnimation == animationName && isAnimationLooping == loop)
            return;

        // 애니메이션 재생
        var trackEntry = skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
        trackEntry.TimeScale = timeScale;

        // 현재 애니메이션 상태 업데이트
        currentAnimation = animationName;
        isAnimationLooping = loop;

        // 디버그 로그
        Debug.Log($"[{gameObject.name}] 애니메이션 변경: {animationName}, 반복: {loop}");
    }

    // 이동 방향에 따른 캐릭터 좌우 반전 처리
    public void UpdateFlipX(float xDirection)
    {
        if (skeletonAnimation == null) return;

        // x 방향이 의미있는 값일 때만 반전 적용
        if (Mathf.Abs(xDirection) > 0.01f)
        {
            // 방향 로직: 오른쪽으로 이동하면 반전(-1), 왼쪽으로 이동하면 정상(1)
            skeletonAnimation.skeleton.ScaleX = xDirection > 0 ? -1 : 1;
        }
    }

    public void SetDirectionBySeatIndex(int seatIndex)
    {
        if (skeletonAnimation == null) return;

        bool isEvenSeat = seatIndex % 2 == 0;

        // 짝수 좌석이면 오른쪽 바라보기, 홀수 좌석이면 왼쪽 바라보기
        skeletonAnimation.skeleton.ScaleX = isEvenSeat ? -1 : 1;

        // 좌석에 앉았음을 표시
        isSeated = true;

        Debug.Log($"좌석 {seatIndex}번에 앉은 손님, 방향: {(isEvenSeat ? "오른쪽" : "왼쪽")}");
    }

    // 퇴장 준비 메서드 (Customer.cs에서 호출)
    public void PrepareForExit()
    {
        isSeated = false;
        Debug.Log($"{gameObject.name} 퇴장 준비");
    }
}