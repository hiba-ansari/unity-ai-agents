using Globals;
using SteeringCalcs;
using UnityEngine;

public class Snake : MonoBehaviour
{
    public static SnakeState State;

    // Obstacle avoidance parameters (see the assignment spec for an explanation).
    public AvoidanceParams AvoidParams;

    // Steering parameters.
    public float MaxSpeed;
    public float MaxAccel;
    public float AccelTime;

    // Use this as the arrival radius for all states where the steering behaviour == arrive.
    public float ArriveRadius;

    // Parameters controlling transitions in/out of the Aggro state.
    public float AggroRange;
    public float DeAggroRange;

    // Reference to the frog (the target for the Aggro state).
    public GameObject Frog;

    // The patrol point (the target for the PatrolAway state).
    public Transform PatrolPoint;

    // The current target of the snake (see the assignment spec for an explanation).
    private Vector2 _target;

    // The snake's initial position (the target for the PatrolHome and Harmless states).
    private Vector2 _home;

    // Debug rendering config
    private float _debugHomeOffset = 0.3f;

    // References for gameobject controls
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private Animator _animator;

    // Bubble
    public GameObject Bubble;

    // Speed/Aggro range timers
    public float totalTime = 3f;
    public static bool speedTimerActive = false;
    public static bool rangeTimerActive = false;
    private float speedTimer;
    private float rangeTimer;
    private float currentSpeed;
    public float currentAggroRange;
    private float reducedSpeed;
    private float increasedAggroRange;

    // Snake FSM states (don't edit this enum).
    public enum SnakeState : int
    {
        PatrolAway = 0,
        PatrolHome = 1,
        Attack = 2,
        Benign = 3,
        Fleeing = 4
    }

    // Snake FSM events (don't edit this enum).
    public enum SnakeEvent : int
    {
        FrogInRange = 0,
        FrogOutOfRange = 1,
        BitFrog = 2,
        ReachedTarget = 3,
        HitByBubble = 4,
        NotScared = 5
    }

    // Direction IDs used by the snake animator (don't edit these).
    private enum Direction : int
    {
        Up = 0,
        Left = 1,
        Down = 2,
        Right = 3
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        _home = transform.position;

        SetState(SnakeState.PatrolAway);

        speedTimer = totalTime;
        rangeTimer = totalTime;
        currentSpeed = MaxSpeed;
        currentAggroRange = AggroRange;

        reducedSpeed = currentSpeed * 0.3f;
        increasedAggroRange = currentAggroRange * 2f;
    }

    void Update()
    {
        // Speed timer
        if (speedTimerActive)
        {
            speedTimer -= Time.deltaTime;

            if (speedTimer > 0)
            {
                currentSpeed = reducedSpeed;
            }
            else if (speedTimer <= 0)
            {
                // Reset all timer-related values
                currentSpeed = MaxSpeed;
                speedTimer = totalTime;
                speedTimerActive = false;
            }
        }

        // Range timer
        if (rangeTimerActive)
        {
            rangeTimer -= Time.deltaTime;

            if (rangeTimer > 0)
            {
                currentAggroRange = increasedAggroRange;
            }
            else if (rangeTimer <= 0)
            {
                // Reset all timer-related values
                currentAggroRange = AggroRange;
                rangeTimer = totalTime;
                rangeTimerActive = false;
            }
        }
    }

    void FixedUpdate()
    {
        // Events triggered by each fixed update tick
        FixedUpdateEvents();

        // Update the Fly behaviour based on the current FSM state
        FSM_State();

        // Configure final appearance of the snake
        UpdateAppearance();
    }

    // Trigger Events for each fixed update tick, using a trigger first FSM implementation
    void FixedUpdateEvents()
    {
        if (State == SnakeState.PatrolHome ||
            State == SnakeState.PatrolAway)
        {
            if (InAggroRange() && Frog.GetComponent<Frog>().Health > 0)
            {
                HandleEvent(SnakeEvent.FrogInRange);
            }
            else if (AtTarget())
            {
                HandleEvent(SnakeEvent.ReachedTarget);
            }
        }
        else if (State == SnakeState.Attack)
        {
            if (OutOfAggroRange())
            {
                HandleEvent(SnakeEvent.FrogOutOfRange);
            }
        }
        else if (State == SnakeState.Benign)
        {
            if (AtTarget())
            {
                HandleEvent(SnakeEvent.ReachedTarget);
            }

        }
        else if (State == SnakeState.Fleeing)
        {
            if (OutOfAggroRange())
            {
                HandleEvent(SnakeEvent.NotScared);
            }
        }
    }


    // Process the current FSM state, using an event first FSM implementation
    void FSM_State()
    {
        UpdateSnakeTargetPos();

        Vector2 desiredVel = Vector2.zero;

        if (State == SnakeState.PatrolAway ||
            State == SnakeState.PatrolHome ||
            State == SnakeState.Benign)
        {
            desiredVel = Steering.Arrive(transform.position, _target, currentSpeed, ArriveRadius, AvoidParams);
        }
        else if (State == SnakeState.Attack)
        {
            desiredVel = Steering.Seek(transform.position, _target, currentSpeed, AvoidParams);
        }
        else if (State == SnakeState.Fleeing)
        {
            desiredVel = Steering.Flee(transform.position, _target, currentSpeed, AvoidParams);
        }

        // Convert the desired velocity to a force, then apply it.
        Vector2 steering = Steering.DesiredVelToForce(desiredVel, _rb, AccelTime, MaxAccel);
        _rb.AddForce(steering);
        if (_rb.linearVelocity.magnitude > currentSpeed)
        {
            _rb.linearVelocity = _rb.linearVelocity.normalized * currentSpeed;
        }
    }

    // Choose the target of the snake, this depends on the FSM state
    private void UpdateSnakeTargetPos()
    {
        if (State == SnakeState.PatrolAway)
        {
            _target = PatrolPoint.position;
        }
        else if (State == SnakeState.PatrolHome)
        {
            _target = _home;
        }
        else if (State == SnakeState.Attack || State == SnakeState.Fleeing)
        {
            _target = Frog.transform.position;
        }
        else if (State == SnakeState.Benign)
        {
            _target = _home;
        }
        else
        {
            _target = _home;
        }
    }
    private void SetState(SnakeState newState)
    {
        if (newState != State)
        {
            // Can uncomment this for debugging purposes.
            // Debug.Log(name + " switching state to " + newState.ToString());

            State = newState;
        }
    }


    //TODO update so that the snake attack only alive frogs
    private void HandleEvent(SnakeEvent e)
    {
        if (State == SnakeState.Fleeing)
        {
            if (e == SnakeEvent.NotScared)
            {
                SetState(SnakeState.PatrolHome);
            }
        }
        else
        {
            if (e == SnakeEvent.HitByBubble)
            {
                SetState(SnakeState.Fleeing);
            }

            else if (State == SnakeState.PatrolAway)
            {
                if (e == SnakeEvent.FrogInRange && Frog.GetComponent<Frog>().Health > 0)
                {
                    SetState(SnakeState.Attack);
                }
                else if (e == SnakeEvent.ReachedTarget)
                {
                    SetState(SnakeState.PatrolHome);
                }
            }
            else if (State == SnakeState.PatrolHome)
            {
                if (e == SnakeEvent.FrogInRange && Frog.GetComponent<Frog>().Health > 0)
                {
                    SetState(SnakeState.Attack);
                }
                else if (e == SnakeEvent.ReachedTarget)
                {
                    SetState(SnakeState.PatrolAway);
                }
            }
            else if (State == SnakeState.Attack)
            {
                if (e == SnakeEvent.BitFrog)
                {
                    SetState(SnakeState.Benign);
                }
                else if (e == SnakeEvent.FrogOutOfRange)
                {
                    SetState(SnakeState.PatrolHome);
                }
            }
            else if (State == SnakeState.Benign)
            {
                if (e == SnakeEvent.ReachedTarget)
                {
                    SetState(SnakeState.PatrolHome);
                }
            }
        }
    }
    private void UpdateAppearance()
    {
        // Update the snake's colour to provide a visual indication of its state.
        if (State == SnakeState.PatrolAway)
        {
            _sr.color = new Color(0.3f, 0.3f, 0.3f);
        }
        if (State == SnakeState.PatrolHome)
        {
            _sr.color = new Color(1.0f, 1.0f, 1.0f);
        }
        else if (State == SnakeState.Attack)
        {
            _sr.color = new Color(1.0f, 0.2f, 0.2f);
        }
        else if (State == SnakeState.Benign || State == SnakeState.Fleeing)
        {
            _sr.color = new Color(0.20f, 0.94f, 0.23f);
        }

        // Update the Snake visual based on the direction it's moving
        if (_rb.linearVelocity.magnitude > Constants.MIN_SPEED_TO_ANIMATE)
        {
            // Determine the bearing of the snake in degrees (between -180 and 180)
            float angle = Mathf.Atan2(_rb.linearVelocity.y, _rb.linearVelocity.x) * Mathf.Rad2Deg;

            if (angle > -135.0f && angle <= -45.0f) // Down
            {
                transform.up = new Vector2(0.0f, -1.0f);
                _animator.SetInteger("Direction", (int)Direction.Down);
            }
            else if (angle > -45.0f && angle <= 45.0f) // Right
            {
                transform.up = new Vector2(1.0f, 0.0f);
                _animator.SetInteger("Direction", (int)Direction.Right);
            }
            else if (angle > 45.0f && angle <= 135.0f) // Up
            {
                transform.up = new Vector2(0.0f, 1.0f);
                _animator.SetInteger("Direction", (int)Direction.Up);
            }
            else // Left
            {
                transform.up = new Vector2(-1.0f, 0.0f);
                _animator.SetInteger("Direction", (int)Direction.Left);
            }
        }

        // Display the Snake home position as a cross
        Debug.DrawLine(_home + new Vector2(-_debugHomeOffset, -_debugHomeOffset), _home + new Vector2(_debugHomeOffset, _debugHomeOffset), Color.magenta);
        Debug.DrawLine(_home + new Vector2(-_debugHomeOffset, _debugHomeOffset), _home + new Vector2(_debugHomeOffset, -_debugHomeOffset), Color.magenta);
    }

    // Events for 2D collisions
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (State == SnakeState.Attack && collision.gameObject == Frog)
        {
            HandleEvent(SnakeEvent.BitFrog);
            Frog.GetComponent<Frog>().TakeDamage();
        }

        // Note: Collision logic for when the snake is hit by a bubble is triggered by the Bubble
        if (collision.gameObject.CompareTag("Bubble"))
        {
            HandleEvent(SnakeEvent.HitByBubble);
        }
    }


    // Helper to check if we're at the target position
    private bool AtTarget()
    {
        return ((Vector2)transform.position - _target).magnitude < Constants.TARGET_REACHED_TOLERANCE;
    }

    private bool InAggroRange()
    {
        return (transform.position - Frog.transform.position).magnitude < currentAggroRange;
    }

    private bool OutOfAggroRange()
    {
        return (transform.position - Frog.transform.position).magnitude > DeAggroRange;
    }

    public static void slowDown()
    {
        speedTimerActive = true;
    }

    public static void increaseAggroRange()
    {
        rangeTimerActive = true;
    }
}
