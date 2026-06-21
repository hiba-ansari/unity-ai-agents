using UnityEngine;

public class Bubble : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collider)
    {
        // Destroy bubbles if they collide with a snake, or if they collide with anything on the Obstacle layer
        if (collider.gameObject.tag.Equals("Snake") || collider.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}