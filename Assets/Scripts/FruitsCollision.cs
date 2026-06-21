using UnityEngine;

public class FruitsCollision : MonoBehaviour
{
    private SpriteRenderer sr;
    private FruitsBehaviour fruitManager;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        fruitManager = FindFirstObjectByType<FruitsBehaviour>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Frog"))
        {
            sr.enabled = false;

            if (fruitManager != null)
            {
                fruitManager.Invoke("generateFruit", 3f);
            }
        }
    }
    
}
