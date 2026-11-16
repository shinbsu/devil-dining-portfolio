using Spine.Unity;
using UnityEngine;
using System;

// 기존 상태 유지
public enum EmployeeState
{
    Idle,
    Assigned,
    MovingToTarget,
    Working,
    Returning
}

// 애니메이션 상태 추가 (Spine 애니메이션과 직접 연결)
public enum EmployeeAnimState
{
    Idle,
    Walking,
    Cooking,
    Cooking2,
    Serving
}

public abstract class EmployeeBase : MonoBehaviour
{
    [Header("이동 속도")]
    public float moveSpeed = 2f;

    protected Vector2 targetPos;
    protected bool isMoving;

    // Spine 애니메이션 관련
    protected SkeletonAnimation skeletonAnimation;
    protected EmployeeAnimState currentAnimState = EmployeeAnimState.Idle;

    // 목적지 도착 시 호출할 이벤트
    public event Action OnArrived;

    // 2.5D 지원 플래그 (기존 기능 보존용)
    [Header("2.5D 설정")]
    [SerializeField] private bool enable2_5D = true;

    protected virtual void Awake()
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
        }
        else
        {
            // 초기 idle 애니메이션 재생
            PlayAnimState(EmployeeAnimState.Idle);
        }
    }

    protected virtual void Start()
    {
        // 2.5D 시스템 등록 (선택적)
        if (enable2_5D && Pseudo3DConverter.Instance != null)
        {
            Pseudo3DConverter.Instance.RegisterAndConvertObject(transform);

            // Depth25DMovement 컴포넌트 추가 (중복 방지)
            if (GetComponent<Depth25DMovement>() == null)
            {
                gameObject.AddComponent<Depth25DMovement>();
            }
        }
    }

    /// <summary>
    /// 외부에서 목표 지점 지정 - virtual로 변경하여 오버라이드 가능
    /// </summary>
    public virtual void SetNewTarget(Vector2 pos)
    {
        targetPos = pos;
        isMoving = true;

        // 이동 시작할 때 walking 애니메이션 재생
        PlayAnimState(EmployeeAnimState.Walking);

        // 2.5D 등록 (아직 등록되지 않은 경우)
        if (enable2_5D && Pseudo3DConverter.Instance != null)
        {
            Pseudo3DConverter.Instance.RegisterAndConvertObject(transform);
        }
    }

    protected virtual void Update()
    {
        if (!isMoving) return;

        // 2.5D 이동 컴포넌트 확인
        Depth25DMovement depthMovement = GetComponent<Depth25DMovement>();

        Vector3 currentPos = transform.position;
        Vector3 targetPos3D = new Vector3(targetPos.x, targetPos.y, currentPos.z);

        Vector3 nextPos;

        if (enable2_5D && depthMovement != null)
        {
            // 2.5D 최적화된 이동 사용
            nextPos = depthMovement.Move2_5D(currentPos, targetPos3D, moveSpeed, Time.deltaTime);
        }
        else
        {
            // 기존 2D 이동 방식 유지
            Vector2 cur2D = new Vector2(currentPos.x, currentPos.y);
            Vector2 next2D = Vector2.MoveTowards(cur2D, targetPos, moveSpeed * Time.deltaTime);
            nextPos = new Vector3(next2D.x, next2D.y, currentPos.z);
        }

        // 위치 업데이트
        transform.position = nextPos;

        // 2.5D 깊이 업데이트 (선택적)
        if (enable2_5D && Pseudo3DConverter.Instance != null)
        {
            Pseudo3DConverter.Instance.OnCharacterMoved(transform);
        }

        // 이동 방향에 따라 캐릭터 좌우 반전
        Vector2 movementDirection = new Vector2(nextPos.x - currentPos.x, nextPos.y - currentPos.y);
        if (movementDirection.magnitude > 0.001f)
        {
            UpdateFlipX(movementDirection.x);
        }

        // 목적지 도착 처리
        if (Vector2.Distance(new Vector2(nextPos.x, nextPos.y), targetPos) < 0.01f)
        {
            isMoving = false;
            Vector3 finalPos = new Vector3(targetPos.x, targetPos.y, nextPos.z);
            transform.position = finalPos;

            // 최종 위치에서 2.5D 업데이트
            if (enable2_5D && Pseudo3DConverter.Instance != null)
            {
                Pseudo3DConverter.Instance.OnCharacterMoved(transform);
            }

            // 이동 멈추면 idle 애니메이션으로 전환
            PlayAnimState(EmployeeAnimState.Idle);

            // 도착 이벤트 호출 (안전하게)
            InvokeOnArrivedSafely();
        }
    }

    /// <summary>
    /// OnArrived 이벤트를 안전하게 호출하는 메서드
    /// </summary>
    protected void InvokeOnArrivedSafely()
    {
        try
        {
            OnArrived?.Invoke();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"OnArrived 이벤트 실행 중 오류 발생: {e.Message}");
        }
    }

    /// <summary>
    /// 애니메이션 상태 변경
    /// </summary>
    protected void PlayAnimState(EmployeeAnimState animState, float timeScale = 1f)
    {
        if (skeletonAnimation == null) return;

        // 이미 같은 애니메이션이면 무시
        if (currentAnimState == animState &&
            !(animState == EmployeeAnimState.Cooking || animState == EmployeeAnimState.Cooking2))
            return;

        currentAnimState = animState;

        string animationName;
        bool loop = true;

        // 상태에 따른 애니메이션 이름 매핑
        switch (animState)
        {
            case EmployeeAnimState.Walking:
                animationName = "walk";
                break;
            case EmployeeAnimState.Cooking:
                animationName = "cooking";
                break;
            case EmployeeAnimState.Cooking2:
                animationName = "cooking2";
                break;
            case EmployeeAnimState.Serving:
                animationName = "serving";
                loop = true; // 2.5D에서는 서빙도 루프
                break;
            case EmployeeAnimState.Idle:
            default:
                animationName = "idle";
                break;
        }

        // 애니메이션 재생
        var trackEntry = skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
        trackEntry.TimeScale = timeScale;

        // 디버그 로그
        Debug.Log($"[{gameObject.name}] 애니메이션 변경: {animationName}, 반복: {loop}");
    }

    /// <summary>
    /// 이동 방향에 따른 캐릭터 좌우 반전 처리
    /// </summary>
    protected void UpdateFlipX(float xDirection)
    {
        if (skeletonAnimation == null) return;

        // x 방향이 의미있는 값일 때만 반전 적용
        if (Mathf.Abs(xDirection) > 0.01f)
        {
            // 왼쪽으로 이동하면 반전(true), 오른쪽으로 이동하면 정상(false)
            skeletonAnimation.skeleton.ScaleX = xDirection < 0 ? -1 : 1;
        }
    }

    /// <summary>
    /// 요리 애니메이션 랜덤 선택 (cooking 또는 cooking2)
    /// </summary>
    protected void PlayRandomCookingAnim()
    {
        PlayAnimState(UnityEngine.Random.value > 0.5f ?
            EmployeeAnimState.Cooking : EmployeeAnimState.Cooking2);
    }
}