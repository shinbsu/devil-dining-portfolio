using UnityEngine;

public class CookingStation : MonoBehaviour
{
    [Header("이 스테이션 위에서 직원이 움직일 목표 지점")]
    public Transform workPoint;

    [Header("요리 완료 시 완성품을 놓을 선반")]
    public Transform dropOffPoint;

    public bool isAvailable { get; private set; } = true;

    public bool Reserve()
    {
        if (!isAvailable) return false;
        isAvailable = false;
        return true;
    }

    public void Release()
    {
        isAvailable = true;
    }

    /// <summary>
    /// 직원이 이동해야 할 좌표 반환
    /// </summary>
    public Vector3 GetWorkPosition()
    {
        return workPoint != null
            ? workPoint.position
            : transform.position;
    }

    /// <summary>
    /// 완성품을 내려놓을 포지션
    /// </summary>
    public Vector3 GetDropOffPosition()
    {
        return dropOffPoint != null
            ? dropOffPoint.position
            : transform.position;
    }
}
