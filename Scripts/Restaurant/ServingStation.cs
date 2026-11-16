using UnityEngine;
using System.Collections.Generic;

public class ServingStation : MonoBehaviour
{
    [Header("직원이 요리를 집어갈 지점")]
    public Transform pickupPoint;

    private List<Recipe> dishes = new List<Recipe>();

    public bool HasDish => dishes.Count > 0;
    public void AddDish(Recipe r) => dishes.Add(r);
    public List<Recipe> GetAllDishes() => new List<Recipe>(dishes);

    public bool RemoveSpecificDish(Recipe r)
    {
        return dishes.Remove(r);
    }

    /// <summary>
    /// 직원이 선반으로 이동해야 할 좌표
    /// </summary>
    public Vector3 GetPickupPosition()
    {
        return pickupPoint != null
            ? pickupPoint.position
            : transform.position;
    }
}
