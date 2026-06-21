using System.Collections.Generic;
using UnityEngine;

public class FruitsBehaviour : MonoBehaviour
{
    /*
        NOTES
        - one appears at a time
        - red = restore a frog life (if health below 3)
        - orange = increase snake aggro range
        - yellow = snake slows down (3 sec)
    */

    private Transform _redFruit;
    private Transform _orangeFruit;
    private Transform _yellowFruit;
    private SpriteRenderer _redFruitSr;
    private SpriteRenderer _orangeFruitSr;
    private SpriteRenderer _yellowFruitSr;
    public static List<Node> fruitNodes = new List<Node>();

    void Start()
    {
        _redFruit = GameObject.Find("red-fruit").transform;
        _orangeFruit = GameObject.Find("orange-fruit").transform;
        _yellowFruit = GameObject.Find("yellow-fruit").transform;

        _redFruitSr = _redFruit.GetComponent<SpriteRenderer>();
        _orangeFruitSr = _orangeFruit.GetComponent<SpriteRenderer>();
        _yellowFruitSr = _yellowFruit.GetComponent<SpriteRenderer>();

        _redFruitSr.enabled = false;
        _orangeFruitSr.enabled = false;
        _yellowFruitSr.enabled = false;

        generateFruit();
    }

    public void generateFruit()
    {
        // Randomly generate next fruit's location on a walkable node
        List<Node> spawnNodes = new List<Node>();
        while (spawnNodes.Count < 3)
        {
            Node randomNode = fruitNodes[Random.Range(0, fruitNodes.Count)];
            if (!spawnNodes.Contains(randomNode))
            {
                spawnNodes.Add(randomNode);
            }
        }

        _redFruit.transform.position = new Vector2(spawnNodes[0].worldPosition.x, spawnNodes[0].worldPosition.y); ;
        _redFruitSr.enabled = true;
        _orangeFruit.transform.position = new Vector2(spawnNodes[1].worldPosition.x, spawnNodes[1].worldPosition.y); ;
        _orangeFruitSr.enabled = true;
        _yellowFruit.transform.position = new Vector2(spawnNodes[2].worldPosition.x, spawnNodes[2].worldPosition.y); ;
        _yellowFruitSr.enabled = true;
    }

    public void RegenerateFruit(string color)
    {
        Node node = fruitNodes[Random.Range(0, fruitNodes.Count)];
        Vector3 pos = new Vector3(node.worldPosition.x, node.worldPosition.y, 0);

        if (color == "red")
        {
            _redFruit.position = pos;
            _redFruitSr.enabled = true;
        }
        else if (color == "orange")
        {
            _orangeFruit.position = pos;
            _orangeFruitSr.enabled = true;
        }
        else if (color == "yellow")
        {
            _yellowFruit.position = pos;
            _yellowFruitSr.enabled = true;
        }
    }
}
