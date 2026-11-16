using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ZoneArea : MonoBehaviour
{
    public enum ZoneType { Kitchen, Hall }
    public ZoneType zoneType;
    [Tooltip("직원이 돌아다닐 범위 (BoxCollider2D)")]
    public BoxCollider2D areaCollider;

    private Collider2D _zoneCollider;

    private void Awake()
    {
        _zoneCollider = GetComponent<Collider2D>();
    }

    private void Reset()
    {
        // 기본으로 BoxCollider2D 추가 후 trigger 설정
        areaCollider = GetComponent<BoxCollider2D>();
        areaCollider.isTrigger = true;
    }

    private void OnDrawGizmos()
    {
        if (areaCollider == null) return;
        Gizmos.color = zoneType == ZoneType.Kitchen
            ? new Color(1f, 0.5f, 0.5f, 0.3f)
            : new Color(0.5f, 0.5f, 1f, 0.3f);
        Matrix4x4 m = Matrix4x4.TRS(
            areaCollider.transform.position + (Vector3)areaCollider.offset,
            areaCollider.transform.rotation,
            Vector3.one);
        Gizmos.matrix = m;
        Gizmos.DrawCube(Vector3.zero, areaCollider.size);
    }

    /// <summary>
    /// worldPos 좌표가 이 ZoneArea 콜라이더 안에 있으면 true
    /// </summary>
    public bool IsInside(Vector2 worldPos)
    {
        // OverlapPoint는 Collider2D 위에 점이 있는지 체크해 줍니다.
        return _zoneCollider != null && _zoneCollider.OverlapPoint(worldPos);
    }

    /// <summary>
    /// 이 Zone 안의 랜덤 위치를 반환
    /// </summary>
    public Vector2 GetRandomPointInside()
    {
        var c = areaCollider;
        float x = Random.Range(-c.size.x / 2f, c.size.x / 2f);
        float y = Random.Range(-c.size.y / 2f, c.size.y / 2f);
        Vector2 local = new Vector2(x + c.offset.x, y + c.offset.y);
        return (Vector2)transform.position + local;
    }
}
