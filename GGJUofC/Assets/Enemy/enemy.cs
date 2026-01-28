using UnityEngine;
using UnityEngine.AI;

public class EnemyAIController : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Animator animator;
    public Transform player; // assign in inspector (recommended)

    [Header("Patrol")]
    public Transform[] waypoints;
    public float waitAtPoint = 1.5f;

    [Header("Movement")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 6.5f;

    [Header("Detection")]
    public float aggroRadius = 15f;
    public float loseRadius = 18f;

    [Header("Attack")]
    public float attackRange = 3f;
    public float attackCooldown = 2f;
    public float attackDamage = 20f; // matches PlayerHealth.TakeDamage(float)
    public float faceSpeed = 8f;

    [Header("Animator Params")]
    public string animMoveBool = "IsMoving";
    public string animSpeedFloat = "Speed";
    public string animAttackTrigger = "Attack";

    int _wpIndex = 0;
    float _waitTimer = 0f;
    float _attackTimer = 0f;

    enum State { Patrol, Chase, Attack }
    State _state = State.Patrol;

    PlayerHealth _playerHealth;

    void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        // Auto-find player by tag if not assigned (optional convenience)
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player != null)
            _playerHealth = player.GetComponent<PlayerHealth>();
    }

    void Start()
    {
        if (agent == null) return;

        agent.speed = walkSpeed;

        if (waypoints != null && waypoints.Length > 0 && waypoints[0] != null)
            agent.SetDestination(waypoints[0].position);
    }

    void Update()
    {
        if (agent == null) return;

        if (player == null)
        {
            Patrol();
            UpdateAnim();
            return;
        }

        _attackTimer -= Time.deltaTime;

        float dist = Vector3.Distance(transform.position, player.position);

        bool seesPlayer =
            (_state == State.Patrol) ? (dist <= aggroRadius) : (dist <= loseRadius);

        // State switching
        if (!seesPlayer)
            _state = State.Patrol;
        else if (dist <= attackRange)
            _state = State.Attack;
        else
            _state = State.Chase;

        // Behavior
        if (_state == State.Patrol) Patrol();
        else if (_state == State.Chase) Chase();
        else Attack();

        UpdateAnim();
    }

    void Patrol()
    {
        agent.speed = walkSpeed;
        agent.stoppingDistance = 0f;
        agent.isStopped = false;

        if (waypoints == null || waypoints.Length == 0)
        {
            SetMoving(false);
            return;
        }

        if (waypoints[_wpIndex] == null)
        {
            _wpIndex = (_wpIndex + 1) % waypoints.Length;
            return;
        }

        if (!agent.hasPath)
            agent.SetDestination(waypoints[_wpIndex].position);

        if (!agent.pathPending && agent.remainingDistance <= 0.2f)
        {
            SetMoving(false);
            _waitTimer += Time.deltaTime;

            if (_waitTimer >= waitAtPoint)
            {
                _waitTimer = 0f;
                _wpIndex = (_wpIndex + 1) % waypoints.Length;

                if (waypoints[_wpIndex] != null)
                    agent.SetDestination(waypoints[_wpIndex].position);

                SetMoving(true);
            }
        }
        else
        {
            SetMoving(true);
        }
    }

    void Chase()
    {
        agent.speed = runSpeed;
        agent.stoppingDistance = attackRange * 0.9f;
        agent.isStopped = false;

        agent.SetDestination(player.position);
        SetMoving(true);
    }

    void Attack()
    {
        agent.isStopped = true;
        agent.ResetPath();
        SetMoving(false);

        Face(player.position);

        // IMPORTANT CHANGE:
        // We only TRIGGER the animation here.
        // Damage will be applied by an Animation Event calling DealDamageToPlayer().
        if (_attackTimer <= 0f)
        {
            _attackTimer = attackCooldown;

            if (animator != null)
                animator.SetTrigger(animAttackTrigger);
        }
    }

    // IMPORTANT CHANGE:
    // Call this from an Animation Event on the Claw Attack clip at the HIT frame.
    public void DealDamageToPlayer()
    {
        if (player == null) return;

        if (_playerHealth == null)
            _playerHealth = player.GetComponent<PlayerHealth>();

        if (_playerHealth != null)
            _playerHealth.TakeDamage(attackDamage);
    }

    void Face(Vector3 targetPos)
    {
        Vector3 dir = targetPos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion look = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * faceSpeed);
    }

    void SetMoving(bool moving)
    {
        if (animator == null) return;
        animator.SetBool(animMoveBool, moving);
    }

    void UpdateAnim()
    {
        if (animator == null || agent == null) return;
        animator.SetFloat(animSpeedFloat, agent.velocity.magnitude);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
