using UnityEngine;

public enum PatrolMode { Loop, PingPong, Random }

[RequireComponent(typeof(Rigidbody))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] points;
    [SerializeField] private PatrolMode mode = PatrolMode.Loop;
    [SerializeField] private float speed = 3.5f;
    [SerializeField] private float waitTime = 1.0f;
    [SerializeField] private float turnSpeed = 360f;

    [Header("Detection & Stealth Settings")]
    [SerializeField] private float chaseSpeed = 5.5f;
    [SerializeField] private float detectionRadius = 10f;
    [Range(0, 360)] [SerializeField] private float fieldOfViewAngle = 90f;
    [SerializeField] private LayerMask obstacleMask;

    private Transform player;
    private Rigidbody rb;
    private int index;
    private int step = 1;
    private float waitUntil;
    private bool isChasing;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player != null && CanSeePlayer())
        {
            isChasing = true;
        }
        else
        {
            if (isChasing)
            {
                isChasing = false;
                waitUntil = Time.time;
            }
        }
    }

    void FixedUpdate()
    {
        if (isChasing && player != null)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        if (points == null || points.Length == 0) return;
        if (Time.time < waitUntil)
        {
            // Keep existing gravity, zero out horizontal velocity while waiting
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        Vector3 target = points[index].position;
        Vector3 currentPos = transform.position;
        Vector3 targetPos = new Vector3(target.x, currentPos.y, target.z);

        Vector3 toTarget = targetPos - currentPos;
        float distance = toTarget.magnitude;

        if (distance > 0.15f)
        {
            Vector3 moveDir = toTarget.normalized;
            // Move horizontally via Rigidbody while preserving gravity on Y
            rb.linearVelocity = new Vector3(moveDir.x * speed, rb.linearVelocity.y, moveDir.z * speed);

            Quaternion lookRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, turnSpeed * Time.fixedDeltaTime);
        }
        else
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            index = NextIndex();
            waitUntil = Time.time + waitTime;
        }
    }

    private void ChasePlayer()
    {
        Vector3 target = player.position;
        Vector3 toTarget = target - transform.position;
        toTarget.y = 0;

        if (toTarget.magnitude > 0.1f)
        {
            Vector3 moveDir = toTarget.normalized;
            // Apply chase velocity while letting gravity pull the enemy down if off a ledge
            rb.linearVelocity = new Vector3(moveDir.x * chaseSpeed, rb.linearVelocity.y, moveDir.z * chaseSpeed);

            Quaternion look = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, look, turnSpeed * Time.fixedDeltaTime);
        }
    }

    private bool CanSeePlayer()
    {
        Vector3 dirToPlayer = (player.position - transform.position);
        float distanceToPlayer = dirToPlayer.magnitude;

        if (distanceToPlayer <= detectionRadius)
        {
            float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer.normalized);
            if (angleToPlayer <= fieldOfViewAngle / 2f)
            {
                Vector3 eyePosition = transform.position + Vector3.up * 0.5f;
                Vector3 playerTargetPos = player.position + Vector3.up * 0.5f;
                Vector3 rayDir = (playerTargetPos - eyePosition).normalized;

                if (!Physics.Raycast(eyePosition, rayDir, distanceToPlayer, obstacleMask))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private int NextIndex()
    {
        if (points == null || points.Length == 0) return 0;

        if (mode == PatrolMode.Loop)
        {
            return (index + 1) % points.Length;
        }

        if (mode == PatrolMode.PingPong)
        {
            if (index + step < 0 || index + step >= points.Length)
            {
                step = -step;
            }
            return Mathf.Clamp(index + step, 0, points.Length - 1);
        }

        int next = Random.Range(0, points.Length);
        return (next == index && points.Length > 1) ? (next + 1) % points.Length : next;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerReset();
        }
    }

    private void OnCollisionEnter(Collision collision)
{
    if (collision.gameObject.CompareTag("Player"))
    {
        Health playerHealth = collision.gameObject.GetComponent<Health>();
        if (playerHealth != null)
        {
            // Passes enemy position and 10f impulse force
            playerHealth.TakeDamage(1, transform.position, 10f);
        }
    }
}

    private void TriggerReset()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReloadCurrentLevel();
        }
    }

    private void OnDrawGizmos()
    {
        if (points != null && points.Length > 1)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i] != null)
                {
                    Gizmos.DrawSphere(points[i].position, 0.2f);
                    Transform next = points[(i + 1) % points.Length];
                    if (next != null && mode == PatrolMode.Loop)
                    {
                        Gizmos.DrawLine(points[i].position, next.position);
                    }
                }
            }
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Vector3 fovLine1 = Quaternion.AngleAxis(fieldOfViewAngle / 2, Vector3.up) * transform.forward;
        Vector3 fovLine2 = Quaternion.AngleAxis(-fieldOfViewAngle / 2, Vector3.up) * transform.forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, fovLine1 * detectionRadius);
        Gizmos.DrawRay(transform.position, fovLine2 * detectionRadius);
    }
}