using UnityEngine;

public class TreeLayer : MonoBehaviour
{
    private SpriteRenderer sr;
    private Transform player;

    public float baseOffset;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        player = GameObject.FindWithTag("Player").transform;
    }

    private void Update()
    {
        if (player.transform.position.y < transform.position.y - baseOffset)
            sr.sortingOrder = 0;
        else
            sr.sortingOrder = 5;
    }
}