using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class RandomSpriteAssigner : MonoBehaviour
{
    [Header("스프라이트 추가")]
    [SerializeField] private List<Sprite> possibleSprites = new List<Sprite>();

    void Awake()
    {
        if (possibleSprites == null || possibleSprites.Count == 0)
            return;

        var sr = GetComponent<SpriteRenderer>();
        int idx = Random.Range(0, possibleSprites.Count);
        sr.sprite = possibleSprites[idx];
    }
}
