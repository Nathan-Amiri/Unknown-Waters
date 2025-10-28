using UnityEngine;

public class EventTrigger : MonoBehaviour
{
    protected GameManager gameManager;

    private void Start()
    {
        // Uncomment this when we have sprites in the game
        GetComponent<SpriteRenderer>().enabled = false;

        gameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player"))
            return;

        EventTriggered();
    }

    protected virtual void EventTriggered() { }
}