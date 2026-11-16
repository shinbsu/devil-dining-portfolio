using UnityEngine;

public static class ZoneAreaExtensions
{
    /// <summary>
    /// ZoneArea에 붙은 Collider2D를 이용해,
    /// worldPoint가 그 영역 안에 있는지 검사합니다.
    /// </summary>
    public static bool IsInside(this ZoneArea zone, Vector2 worldPoint)
    {
        if (zone == null) return false;
        var col = zone.GetComponent<Collider2D>();
        return col != null && col.OverlapPoint(worldPoint);
    }
}
