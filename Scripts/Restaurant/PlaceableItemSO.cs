using UnityEngine;

[CreateAssetMenu(menuName = "Placement/Placeable Item")]
public class PlaceableItemSO : ScriptableObject
{
    [Header("식당에 배치할 아이템 고유ID")]
    public string id;
    [Header("UI에 표시할 이름")]
    public string displayName;
    [Header("배치할 Prefab (씬에 인스턴스될 오브젝트)")]
    public GameObject prefab;
    [Header("보관함 기본 개수")]
    public int startingCount = 1;
}
