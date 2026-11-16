using Spine.Unity;
using UnityEngine;

public class EmployeeAnimationController : MonoBehaviour
{
    // Spine 애니메이션 컴포넌트
    private SkeletonAnimation skeletonAnimation;

    // 현재 애니메이션 상태
    private string currentAnimation = "idle";
    private bool isAnimationLooping = true;

    // 마지막 위치 저장 (이동 방향 계산용)
    private Vector2 lastPosition;

    public enum EmployeeAnimState
    {
        Idle,
        Walking,
        Cooking,
        Cooking2,
        Serving
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
            Debug.LogWarning($"[{gameObject.name}] SkeletonAnimation 컴포넌트가 없습니다. 애니메이션이 작동하지 않습니다.");
            return;
        }

        // 초기 위치 저장
        lastPosition = transform.position;

        // 초기 애니메이션 설정
        PlayAnimation(EmployeeAnimState.Idle);
    }

    private void Update()
    {
        if (skeletonAnimation == null) return;

        // 위치가 변경되었는지 확인하여 이동 여부 판단
        Vector2 currentPosition = transform.position;
        float moveDistance = Vector2.Distance(lastPosition, currentPosition);

        // 위치 변경이 있으면 방향 업데이트
        if (moveDistance > 0.001f) // 임계값 낮춤
        {
            // 이동 방향 계산 (x 좌표 변화)
            float xDelta = currentPosition.x - lastPosition.x;

            // x방향 변화가 의미있는 경우에만 플립
            if (Mathf.Abs(xDelta) > 0.001f)
            {
                // 왼쪽 이동 시 왼쪽, 오른쪽 이동 시 오른쪽 바라보기
                skeletonAnimation.skeleton.ScaleX = xDelta < 0 ? 1 : -1;
            }
        }

        // 현재 위치 저장
        lastPosition = currentPosition;
    }

    // 외부에서 호출할 수 있는 애니메이션 재생 메소드
    public void PlayAnimation(EmployeeAnimState animState, float timeScale = 1f)
    {
        if (skeletonAnimation == null) return;

        string animationName;
        bool loop = true;

        // 상태에 따른 애니메이션 이름 결정
        switch (animState)
        {
            case EmployeeAnimState.Walking:
                animationName = "walk";
                loop = true;
                break;
            case EmployeeAnimState.Cooking:
                animationName = "cooking";
                loop = true;
                break;
            case EmployeeAnimState.Cooking2:
                animationName = "cooking2";
                loop = true;
                break;
            case EmployeeAnimState.Serving:
                animationName = "serving";
                loop = true;
                break;
            case EmployeeAnimState.Idle:
            default:
                animationName = "idle";
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
    }

    // 현재 애니메이션 이름 반환
    public string GetCurrentAnimation()
    {
        return currentAnimation;
    }

    // 랜덤 요리 애니메이션 재생
    public void PlayRandomCookingAnim()
    {
        PlayAnimation(UnityEngine.Random.value > 0.5f ?
            EmployeeAnimState.Cooking : EmployeeAnimState.Cooking2);
    }
}