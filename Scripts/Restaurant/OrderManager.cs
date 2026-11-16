using UnityEngine;
using System.Collections.Generic;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance { get; private set; }

    [Header("UI")]
    public Transform orderPanel;
    public GameObject orderTicketPrefab;

    [Header("Recipes")]
    public List<Recipe> possibleRecipes;

    // Recipe → 주문 고객 FIFO
    private Dictionary<Recipe, Queue<Customer>> orderQueues = new Dictionary<Recipe, Queue<Customer>>();
    // 화면에 떠 있는 전표 리스트
    private List<OrderTicket> activeTickets = new List<OrderTicket>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 고객이 주문할 때 호출
    /// </summary>
    public void CreateOrder(Recipe recipe, Customer customer)
    {
        if (!orderQueues.ContainsKey(recipe))
            orderQueues[recipe] = new Queue<Customer>();
        orderQueues[recipe].Enqueue(customer);

        // UI티켓 생성
        var go = Instantiate(orderTicketPrefab, orderPanel);
        var ticket = go.GetComponent<OrderTicket>();
        ticket.Initialize(recipe);
        activeTickets.Add(ticket);
    }

    /// <summary>
    /// 다음 서빙할 고객을 미리 조회
    /// </summary>
    public Customer PeekNextCustomer(Recipe recipe)
    {
        if (orderQueues.TryGetValue(recipe, out var q) && q.Count > 0)
            return q.Peek();
        return null;
    }

    /// <summary>
    /// 실제 서빙 완료 시 해당 고객을 큐에서 제거
    /// </summary>
    public void RemoveCustomer(Recipe recipe, Customer customer)
    {
        if (!orderQueues.TryGetValue(recipe, out var q) || q.Count == 0)
            return;

        // 현재 큐를 리스트로 복사
        var list = new List<Customer>(q);
        // 선택된 customer만 제거
        list.Remove(customer);
        // 큐 재구성
        orderQueues[recipe] = new Queue<Customer>(list);
    }

    /// <summary>
    /// UI 티켓만 FIFO 순서대로 SoldOut 처리
    /// </summary>
    public void MarkOrderComplete(Recipe recipe)
    {
        // 서빙 시점에 호출: recipe와 매칭되는 첫 전표만 삭제(원래는 서빙한 손님이 주문한 전표를 삭제해야 하지만 일단은 이렇게 만들어놈)
        int idx = activeTickets.FindIndex(t => t.Recipe == recipe);
        if (idx < 0) return;
        var ticket = activeTickets[idx];
        ticket.MarkSoldOut();
        activeTickets.RemoveAt(idx);
    }

    /// <summary>
    /// 이 레시피를 주문하고 아직 서빙되지 않은 고객 리스트를 들어온 순서대로 반환
    /// </summary>
    public List<Customer> GetPendingCustomers(Recipe recipe)
    {
        if (orderQueues.TryGetValue(recipe, out var q))
            return new List<Customer>(q);
        return new List<Customer>();
    }

    /// <summary>
    /// 전체 대기 주문 리스트 반환
    /// (필요 시 사용)
    /// </summary>
    public List<Recipe> GetPendingRecipes()
    {
        var list = new List<Recipe>();
        foreach (var kv in orderQueues)
            foreach (var _ in kv.Value)
                list.Add(kv.Key);
        return list;
    }
}
