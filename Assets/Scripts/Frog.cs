using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SteeringCalcs;
using Globals;
using System;
using UnityEngine.UI;
using TMPro;

public class Frog : MonoBehaviour
{
    // Frog status.
    public int Health;
    public int MaxFlies;
    public int FliesCaught;

    // Steering parameters.
    public float MaxSpeed;
    private float currentSpeed;
    public float MaxAccel;
    public float AccelTime;

    // The arrival radius is set up to be dynamic, depending on how far away
    // the player right-clicks from the frog. See the logic in Update().
    public float ArrivePct;
    public float MinArriveRadius;
    public float MaxArriveRadius;
    private float _arriveRadius;

    // Turn this off to make it easier to see overshooting when seek is used
    // instead of arrive.
    public bool HideFlagOnceReached;

    // References to various objects in the scene that we want to be able to modify.
    private Transform _flag;
    private SpriteRenderer _flagSr;
    private DrawGUI _drawGUIScript;
    private Animator _animator;
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;

    // Stores the last position that the player right-clicked. Initially null.
    private Vector2? _lastClickPos;

    // For pathfinding
    private Node[] path;

    // Stores the current target position for wandering or other purposes.
    private Vector2 _currentTarget;

    //WEEK6
    //Used by DTs to make decision
    public float scaredRange;
    public float huntRange;
    private Fly closestFly;
    private Snake closestSnake;
    private float distanceToClosestFly;
    private float distanceToClosestSnake;
    public float anchorWeight;
    public Vector2 AnchorDims;

    public float dangerZone = 2.0f;
    private List<Transform> snakes = new List<Transform>();
    private float _stuckTimer = 0f;
    private Vector2 _lastPosition;
    public GameObject Bubble;
    private FruitsBehaviour _fruitsBehaviour;


    // Define whether a Human or AI controls the frog
    public enum ControlType
    {
        Human,
        AI
    }
    public Toggle toggleControl;
    public ControlType controlType;


    void Start()
    {
        // Initialise the various object references.
        _flag = GameObject.Find("Flag").transform;
        _flagSr = _flag.GetComponent<SpriteRenderer>();
        _flagSr.enabled = false;
        _sr = GetComponent<SpriteRenderer>();
        _fruitsBehaviour = FindFirstObjectByType<FruitsBehaviour>();

        GameObject uiManager = GameObject.Find("UIManager");
        if (uiManager != null)
        {
            _drawGUIScript = uiManager.GetComponent<DrawGUI>();
        }

        _animator = GetComponent<Animator>();

        _rb = GetComponent<Rigidbody2D>();

        _lastClickPos = null;
        _arriveRadius = MinArriveRadius;

        path = new Node[0];

        // Set default to AI control type
        controlType = ControlType.AI;
        toggleControl.onValueChanged.AddListener(ToggleControlType);
        toggleControl.isOn = false;

        _lastPosition = transform.position;
    }

    void Update()
    {
        toggleControl.onValueChanged.AddListener(ToggleControlType);

        snakes.Clear();
        foreach (Snake snake in FindObjectsByType<Snake>(FindObjectsSortMode.None))
        {
            snakes.Add(snake.transform);
        }

        // Check if the player right-clicked (mouse button #1).
        if (Input.GetMouseButtonDown(1) && controlType == ControlType.Human)
        {
            _lastClickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Set the arrival radius dynamically.
            _arriveRadius = Mathf.Clamp(ArrivePct * ((Vector2)_lastClickPos - (Vector2)transform.position).magnitude, MinArriveRadius, MaxArriveRadius);

            _flag.position = (Vector2)_lastClickPos + new Vector2(0.55f, 0.55f);
            _flagSr.enabled = true;

            path = Pathfinding.RequestPath(transform.position, (Vector2)_lastClickPos);

            // Change the world position of the final path node to the actual clicked position,
            // since the centre of the final node might be off somewhat.
            if (path.Length > 0)
            {
                Node fixedFinalNode = path[path.Length - 1].Clone();
                fixedFinalNode.worldPosition = (Vector2)_lastClickPos;
                path[path.Length - 1] = fixedFinalNode;
            }
        }
        else // show the relevant info about fly and snake
        {
            path = Pathfinding.RequestPath(transform.position, _currentTarget);

            if (closestFly != null)
                Debug.DrawLine(transform.position, closestFly.transform.position, Color.yellow);
            if (closestSnake != null)
                Debug.DrawLine(transform.position, closestSnake.transform.position, Color.red);
        }

        // Check if we're launching a bubble
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Bubble != null)
            {
                // Bubble moves based on frog's facing direction
                Vector2 shootDirection = transform.up.normalized;
                Vector3 spawnPointOffset = shootDirection * 0.75f;
                float bubbleSpeed = 12.0f;

                // Instantiate and apply velocity to bubble
                GameObject newBubble = Instantiate(Bubble, transform.position + spawnPointOffset, Quaternion.identity);
                newBubble.name = "Bubble";
                newBubble.GetComponent<Rigidbody2D>().linearVelocity = shootDirection * bubbleSpeed;

                // Add Bubble Collision logic to check for collisions
                newBubble.AddComponent<Bubble>();

                // Destroy bubbles after 2 seconds if they don’t collide with anything
                Destroy(newBubble, 2.0f);
            }
        }
    }

    void FixedUpdate()
    {
        _lastPosition = transform.position;

        if (controlType == ControlType.AI && IsObstacleNearPath())
        {
            Pathfinding.grid.UpdateDynamicObstacles(snakes, dangerZone);
            path = Pathfinding.RequestPath(transform.position, _currentTarget);
            // Debug.Log("AI recalculating path due to nearby snake");
        }

        Vector2 desiredVel = decideMovement();
        Debug.DrawLine((Vector2)transform.position, (Vector2)transform.position + desiredVel, Color.blue);
        Vector2 steering = Steering.DesiredVelToForce(desiredVel, _rb, AccelTime, MaxAccel);
        _rb.AddForce(steering);

        // If the last-clicked position is non-null, move there. Otherwise do nothing.
        if (_lastClickPos != null)
        {
            // Draw the path found by the A* algorithm.
            if (path.Length > 0)
            {
                for (int i = 1; i < path.Length; i++)
                {
                    Debug.DrawLine(path[i - 1].worldPosition, path[i].worldPosition, Color.black);
                }
            }
        }

        UpdateAppearance();
    }

    private bool IsObstacleNearPath()
    {
        foreach (Snake snake in FindObjectsByType<Snake>(FindObjectsSortMode.None))
        {
            foreach (Node node in path)
            {
                if (Vector2.Distance(snake.transform.position, node.worldPosition) < dangerZone)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void UpdateAppearance()
    {
        if (_rb.linearVelocity.magnitude > Constants.MIN_SPEED_TO_ANIMATE)
        {
            _animator.SetBool("Walking", true);
            transform.up = _rb.linearVelocity;
        }
        else
        {
            _animator.SetBool("Walking", false);
        }
    }

    public void TakeDamage()
    {
        if (Health > 0)
        {
            Health--;
        }
    }

    public Vector2 GetCurrentTarget()
    {
        return _currentTarget;
    }

    //TODO Implement the following Decision Tree
    // no health <= 0 --> set speed to 0 and color to red (1, 0.2, 0.2)
    // user clicked --> go to that click
    // nearby/outside of screen --> go towards screen (similar to flies)
    // closest snake nearby --> flee from snake within the screen
    // closest fly within screen --> go towards that fly
    // otherwise --> go to center of the screen

    //TODO SUGGESTED IMPROVEMENTS:
    //go to the center of mass of flies within screen
    //if 2 snake nearby -> freeze
    //Handle shooting bubbles
    //Come up with a better DT, for example: find flies that are within a circle around the frog that doesnt include any snake
    //Extra0 shoot bubble?
    //Extra1 update your code so that: 
    //Extra2 update your code with a better DT (find flies that are within a circle around the frog that doesnt include any snake)
    //Gameplay: tweak speed, range, acceleration and anchoring
    private Vector2 decideMovement()
    {
        findClosestSnake();
        findClosestFly();

        // Change speed according to node terrain type
        currentSpeed = MaxSpeed;
        Node currentNode = Pathfinding.grid.NodeFromWorldPoint(transform.position);
        if (currentNode.isMud)
        {
            currentSpeed = MaxSpeed / 2.0f;
            // Debug.Log("mud speed: " + currentSpeed);
        }
        else if (currentNode.isWater)
        {
            currentSpeed = MaxSpeed * 1.5f;
            // Debug.Log("water speed: " + currentSpeed);
        }

        // If the frog is dead, set speed to 0 and color to red
        if (Health <= 0)
        {
            _rb.linearVelocity = Vector2.zero;
            _sr.color = new Color(1.0f, 0.2f, 0.2f);
            return (Vector2.zero);
        }
        // Human control
        else if (controlType == ControlType.Human)
        {
            // If user clicks, to to that click
            if (_lastClickPos != null)
            {
                return (getVelocityTowardsFlag());
            }

            return (Vector2.zero);
        }
        // AI control
        else
        {
            if (_rb.linearVelocity == Vector2.zero && Vector2.Distance(transform.position, _lastPosition) < 0.01f)
            {
                _stuckTimer += Time.deltaTime;
            }
            else
            {
                _stuckTimer = 0f;
            }

            if (_stuckTimer >= 1.0f)
            {
                float positionX = UnityEngine.Random.Range(-Pathfinding.grid.gridWorldSize.x, Pathfinding.grid.gridWorldSize.x);
                float positionY = UnityEngine.Random.Range(-Pathfinding.grid.gridWorldSize.y, Pathfinding.grid.gridWorldSize.y);
                _currentTarget = new Vector2(positionX, positionY);
                _stuckTimer = 0f;

                return (Steering.SeekDirect(transform.position, _currentTarget, currentSpeed));
            }
            else
            {
                // If the frog is out of screen, go back to screen center
                if (isOutOfScreen(transform))
                {
                    _currentTarget = Vector2.zero;
                    return (Steering.SeekDirect(transform.position, _currentTarget, currentSpeed));
                }
                // If the frog is close to a snake, flee from it
                else if (closestSnake != null && distanceToClosestSnake <= scaredRange)
                {
                    _currentTarget = (transform.position - closestSnake.transform.position).normalized * currentSpeed;
                    return (Steering.FleeDirect(transform.position, closestSnake.transform.position, currentSpeed));
                }
                // If the frog is close to a fly, go towards it
                else if (closestFly != null && (distanceToClosestFly <= huntRange) && (distanceToClosestFly < Vector2.Distance(closestSnake.transform.position, closestFly.transform.position)))
                {
                    _currentTarget = closestFly.transform.position;
                    return (Steering.SeekDirect(transform.position, _currentTarget, currentSpeed));
                }
                else
                {
                    // If the frog is close enough to its target, pick a new random target
                    float distanceToTarget = Vector2.Distance(transform.position, _currentTarget);
                    if (distanceToTarget <= Constants.TARGET_REACHED_TOLERANCE)
                    {
                        float positionX = UnityEngine.Random.Range(-Pathfinding.grid.gridWorldSize.x, Pathfinding.grid.gridWorldSize.x);
                        float positionY = UnityEngine.Random.Range(-Pathfinding.grid.gridWorldSize.y, Pathfinding.grid.gridWorldSize.y);
                        _currentTarget = new Vector2(positionX, positionY);
                    }

                    return Steering.SeekDirect(transform.position, _currentTarget, currentSpeed);
                }
            }
        }
    }

    private Vector2 getVelocityTowardsFlag()
    {
        Vector2 desiredVel = Vector2.zero;
        if (_lastClickPos != null)
        {
            if (((Vector2)_lastClickPos - (Vector2)gameObject.transform.position).magnitude > Constants.TARGET_REACHED_TOLERANCE)
            {
                desiredVel = Steering.ArriveDirect(gameObject.transform.position, (Vector2)_lastClickPos, _arriveRadius, currentSpeed);
            }
            else
            {
                _lastClickPos = null;

                if (HideFlagOnceReached)
                {
                    _flagSr.enabled = false;
                }
            }

        }
        return desiredVel;
    }

    private void findClosestFly()
    {
        distanceToClosestFly = Mathf.Infinity;

        foreach (Fly fly in (Fly[])GameObject.FindObjectsByType(typeof(Fly), FindObjectsSortMode.None))
        {
            float distanceToFly = (fly.transform.position - transform.position).magnitude;
            if (fly.GetComponent<Fly>().State != Fly.FlyState.Dead)
            {
                if (distanceToFly < distanceToClosestFly)
                {
                    closestFly = fly;
                    distanceToClosestFly = distanceToFly;

                }
            }

        }
    }

    //TODO See findClosestFly for inspiration
    private void findClosestSnake()
    {
        distanceToClosestSnake = Mathf.Infinity;

        foreach (Snake snake in (Snake[])GameObject.FindObjectsByType(typeof(Snake), FindObjectsSortMode.None))
        {
            float distanceToSnake = (snake.transform.position - transform.position).magnitude;
            if (distanceToSnake < distanceToClosestSnake)
            {
                closestSnake = snake;
                distanceToClosestSnake = distanceToSnake;
            }
        }
    }

    //TODO Check wether the current transform is out of screen (true) or not (false)
    private bool isOutOfScreen(Transform transform)
    {
        // Get the screen bounds
        Vector2 screenBounds = new Vector2(Screen.width, Screen.height);
        Vector2 maxBounds = Camera.main.ScreenToWorldPoint(screenBounds);
        Vector2 minBounds = Camera.main.ScreenToWorldPoint(Vector2.zero);

        // Check if the transform is out of screen bounds
        if (transform.position.x < minBounds.x || transform.position.x > maxBounds.x ||
            transform.position.y < minBounds.y || transform.position.y > maxBounds.y)
        {
            return true;
        }
        return false;
    }

    // Toggle between Human and AI
    private void ToggleControlType(bool isOn)
    {
        controlType = isOn ? ControlType.Human : ControlType.AI;
    }

    // Eat fruits
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "red-fruit")
        {
            // Restore a life if health is below 3
            if (Health < 3)
            {
                Health++;
            }

            collision.GetComponent<SpriteRenderer>().enabled = false;
            _fruitsBehaviour.RegenerateFruit("red");
        }
        else if (collision.gameObject.name == "orange-fruit")
        {
            // Increase snake aggro range
            Snake.increaseAggroRange();

            collision.GetComponent<SpriteRenderer>().enabled = false;
            _fruitsBehaviour.RegenerateFruit("orange");
        }
        else if (collision.gameObject.name == "yellow-fruit")
        {
            // Slow snake down
            Snake.slowDown();

            collision.GetComponent<SpriteRenderer>().enabled = false;
            _fruitsBehaviour.RegenerateFruit("yellow");
        }
    }
}
