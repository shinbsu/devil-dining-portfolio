using UnityEngine;

public class Depth25DMovement : MonoBehaviour
{
    [Header("2.5D 이동 속도 설정")]
    [SerializeField] public float verticalSpeedMultiplier = 0.6f;   // Y축(깊이) 이동 속도 배율
    [SerializeField] private float horizontalSpeedMultiplier = 1.0f; // X축 이동 속도 배율
    [SerializeField] private bool enableDepthPerspective = true;     // 깊이 원근감 활성화

    [Header("원근감 설정")]
    [SerializeField] private float perspectiveStrength = 0.5f;       // 원근감 강도
    [SerializeField] private float referenceY = 0f;                  // 기준 Y 좌표

    private Vector3 lastPosition;
    private bool isMoving = false;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        // 이동 감지
        Vector3 currentPosition = transform.position;
        float distance = Vector3.Distance(lastPosition, currentPosition);

        if (distance > 0.001f)
        {
            if (!isMoving)
            {
                isMoving = true;
                OnStartMoving();
            }

            HandleMovementEffects(currentPosition - lastPosition);
            lastPosition = currentPosition;
        }
        else
        {
            if (isMoving)
            {
                isMoving = false;
                OnStopMoving();
            }
        }
    }

    void HandleMovementEffects(Vector3 movement)
    {
        // Y축 이동에 따른 시각적 효과
        if (Mathf.Abs(movement.y) > 0.001f)
        {
            ApplyDepthMovementFeedback(movement.y);
        }
    }

    void ApplyDepthMovementFeedback(float yMovement)
    {
        // Y축 이동 시 미묘한 시각적 피드백
        if (enableDepthPerspective)
        {
            // 깊이 이동에 따른 원근감 효과
            float currentY = transform.position.y;
            float perspectiveFactor = 1.0f + ((currentY - referenceY) * perspectiveStrength * 0.01f);

            // Pseudo3DConverter에서 이미 스케일 처리를 하고 있으므로
            // 여기서는 추가적인 미세 조정만 수행
        }
    }

    void OnStartMoving()
    {
        // 이동 시작 시 효과 (예: 먼지 파티클, 사운드 등)
        Debug.Log($"{gameObject.name} 이동 시작");
    }

    void OnStopMoving()
    {
        // 이동 종료 시 효과
        Debug.Log($"{gameObject.name} 이동 종료");
    }

    // EmployeeBase에서 사용할 수 있는 이동 속도 계산 메서드
    public Vector2 CalculateAdjustedMovement(Vector2 targetDirection, float baseSpeed)
    {
        Vector2 adjustedDirection = targetDirection;

        // Y축 이동 속도 조절 (깊이 이동이므로 느리게)
        adjustedDirection.y *= verticalSpeedMultiplier;

        // X축 이동 속도 조절
        adjustedDirection.x *= horizontalSpeedMultiplier;

        // 원근감에 따른 속도 조절
        if (enableDepthPerspective)
        {
            float currentY = transform.position.y;
            float perspectiveFactor = 1.0f + ((currentY - referenceY) * perspectiveStrength * 0.1f);
            adjustedDirection *= perspectiveFactor;
        }

        return adjustedDirection * baseSpeed;
    }

    // 2.5D에 최적화된 이동 메서드
    public Vector3 Move2_5D(Vector3 currentPos, Vector3 targetPos, float speed, float deltaTime)
    {
        Vector3 direction = (targetPos - currentPos).normalized;

        // 조정된 이동 계산
        Vector2 adjustedMovement = CalculateAdjustedMovement(direction, speed * deltaTime);

        // Z값은 Pseudo3DConverter가 관리하므로 여기서는 X, Y만 처리
        Vector3 newPosition = currentPos + new Vector3(adjustedMovement.x, adjustedMovement.y, 0);

        return newPosition;
    }

    // 깊이에 따른 이동 속도 배율 반환
    public float GetDepthSpeedMultiplier(float yPosition)
    {
        if (!enableDepthPerspective) return 1.0f;

        // Y 위치에 따른 속도 배율 계산
        // 뒤쪽(Y가 높음)일수록 느리게, 앞쪽(Y가 낮음)일수록 빠르게
        float distanceFromReference = yPosition - referenceY;
        float speedMultiplier = 1.0f - (distanceFromReference * perspectiveStrength * 0.05f);

        return Mathf.Clamp(speedMultiplier, 0.5f, 1.5f);
    }
}