using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Placement/Inventory System")]
public class InventorySystem : ScriptableObject
{
    [Header("등록된 배치 가능 아이템 목록")]
    public List<PlaceableItemSO> allPlaceables;

    // runtime에 실제 보유 개수를 저장할 곳
    private Dictionary<string, int> counts = new Dictionary<string, int>();

    /// <summary>
    /// 게임 시작 시(또는 리셋 시) 반드시 호출.
    /// 모든 PlaceableItemSO.startingCount 값으로 counts 초기화.
    /// </summary>
    public void Init()
    {
        counts.Clear();
        foreach (var item in allPlaceables)
        {
            // id가 중복되는 게 없다면 바로 세팅
            counts[item.id] = item.startingCount;
        }
    }

    /// <summary>현재 보유한 개수 리턴</summary>
    public int GetCount(string id)
    {
        return counts.TryGetValue(id, out var c) ? c : 0;
    }

    /// <summary>1개 이상 가지고 있으면 true</summary>
    public bool Has(string id)
    {
        return GetCount(id) > 0;
    }

    /// <summary>1개 소비. 성공하면 true</summary>
    public bool Remove(string id)
    {
        if (!counts.TryGetValue(id, out var c) || c <= 0)
            return false;
        counts[id] = c - 1;
        return true;
    }

    /// <summary>1개 획득</summary>
    public void Add(string id)
    {
        if (counts.ContainsKey(id))
            counts[id]++;
        else
            counts[id] = 1;
    }
}
