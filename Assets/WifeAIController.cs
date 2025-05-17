using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class WifeAIController : MonoBehaviour
{
    public enum AIState { Patrolling, Investigating, Chasing, Searching }

    [Header("General Settings")]
    public AIState currentState = AIState.Patrolling;
    public float waitTime = 2f;
    public float movementSpeed = 3.5f;
    public float stuckCheckTime = 2f;
    public float arrivalThreshold = 0.3f;
    public LayerMask obstacleMask;

    [Header("Detection")]
    public Transform player;
    public Transform eyePoint; // 👁️ NEW: Set this in inspector
    public float sightRange = 20f;
    public float fieldOfView = 110f;

    [Header("Footsteps")]
    public AudioClip[] footstepClips;
    public float stepInterval = 0.5f;

    [Header("Patrol Points")]
    public Transform[] patrolPoints;

    private int currentPatrolIndex = 0;
    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource footstepAudio;

    private float waitTimer;
    private float stepTimer;
    private float stuckTimer;
    private float investigateTimer;

    private Vector3 lastPos;
    private Vector3 investigateTarget;
    private bool isOpeningDoor;

    private Coroutine overrideSearchRoutine;
    private bool isTrackingPlayer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        footstepAudio = gameObject.AddComponent<AudioSource>();
        footstepAudio.spatialBlend = 1f;

        agent.speed = movementSpeed;
        agent.stoppingDistance = arrivalThreshold;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (PlayerActivityTracker.Instance != null)
        {
            PlayerActivityTracker.Instance.OnActivityAlert += HandleActivityAlert;
        }

        if (patrolPoints.Length > 0)
        {
            MoveToNextPatrolPoint();
        }

        lastPos = transform.position;
    }

    void Update()
    {
        if (isOpeningDoor || player == null) return;
        if (player == null || !player.gameObject.activeInHierarchy)
{
    GameObject found = GameObject.FindGameObjectWithTag("Player");
    if (found != null && found.activeInHierarchy)
    {
        player = found.transform;
        if (isTrackingPlayer)
        {
            Debug.Log("[WifeAI] Player reactivated. Resuming chase.");
            currentState = AIState.Chasing;
        }
    }
}

        switch (currentState)
        {
            case AIState.Chasing:
                ChasePlayer();
                break;
            case AIState.Investigating:
                Investigate();
                break;
            case AIState.Patrolling:
                Patrol();
                break;
            case AIState.Searching:
                // Search logic handled in coroutine
                break;
        }

        HandleFootsteps();
        CheckIfStuck();
        TryOpenDoors();
        // If player reappears and was being tracked before, resume chasing
if (isTrackingPlayer && player.gameObject.activeInHierarchy && currentState != AIState.Chasing)
{
    Debug.Log("[WifeAI] Player reappeared, resuming chase.");
    currentState = AIState.Chasing;
}

    }

    void Patrol()
    {
        if (!agent.pathPending &&
            (!agent.hasPath || agent.remainingDistance <= arrivalThreshold || agent.velocity.sqrMagnitude < 0.01f))
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTime)
            {
                waitTimer = 0f;
                MoveToNextPatrolPoint();
            }
        }

        if (CanSeePlayer())
        {
            currentState = AIState.Chasing;
        }
    }

    void Investigate()
    {
        agent.SetDestination(investigateTarget);

        if (agent.remainingDistance <= arrivalThreshold)
        {
            investigateTimer -= Time.deltaTime;
            if (investigateTimer <= 0f)
            {
                currentState = AIState.Patrolling;
                MoveToNextPatrolPoint();
            }
        }

        if (CanSeePlayer())
        {
            currentState = AIState.Chasing;
        }
    }

void ChasePlayer()
{
    if (player == null) return;

    if (!player.gameObject.activeInHierarchy)
    {
        if (isTrackingPlayer)
        {
            Debug.Log("[WifeAI] Lost visual contact, player went inactive.");
        }
        isTrackingPlayer = true; // REMEMBER that she was chasing the player
        return;
    }

    isTrackingPlayer = true;

    if (CanSeePlayer())
    {
        Vector3 playerPos = player.position;

        if (NavMesh.SamplePosition(playerPos, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            // Can't reach exact playerPos; get closest possible point
            if (NavMesh.SamplePosition(playerPos, out NavMeshHit nearest, 10f, NavMesh.AllAreas))
            {
                agent.SetDestination(nearest.position);
                Debug.Log("[WifeAI] Player visible but unreachable. Heading as close as possible.");
            }
        }

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= 2f)
        {
            HitPlayer();
        }
    }
    else
    {
        // Lost sight of the player
        if (isTrackingPlayer)
        {
            investigateTarget = player.position;
            investigateTimer = 4f;
            currentState = AIState.Investigating;
            Debug.Log("[WifeAI] Lost player, investigating last seen location.");
            isTrackingPlayer = false;
        }
    }
}
    bool CanSeePlayer()
    {
        if (player == null || eyePoint == null || !player.gameObject.activeInHierarchy) return false;

        Vector3 dir = player.position - eyePoint.position;
        if (dir.magnitude > sightRange) return false;

        float angle = Vector3.Angle(eyePoint.forward, dir);
        if (angle > fieldOfView / 2f) return false;

        if (!Physics.Raycast(eyePoint.position, dir.normalized, out RaycastHit hit, sightRange, obstacleMask))
            return false;

        return hit.transform == player;
    }

    void HitPlayer()
    {
        Debug.Log("Hit Player");
        // Add more logic later like animations or scene changes
    }

    void HandleFootsteps()
    {
        bool isMoving = agent.velocity.magnitude > 0.1f;
        animator.SetBool("Walking", isMoving);
        animator.SetBool("Idle", !isMoving);

        if (isMoving)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepInterval)
            {
                stepTimer = 0f;
                if (footstepClips.Length > 0)
                {
                    AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
                    footstepAudio.PlayOneShot(clip);
                }
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    void CheckIfStuck()
    {
        float moved = Vector3.Distance(transform.position, lastPos);
        if (moved < 0.05f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > stuckCheckTime)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        lastPos = transform.position;
    }

    void TryOpenDoors()
    {
        if (!agent.hasPath) return;

        Vector3 dir = agent.steeringTarget - transform.position;
        if (Physics.Raycast(transform.position + Vector3.up, dir.normalized, out RaycastHit hit, 2f))
        {
            if (hit.collider.CompareTag("Door"))
            {
                MonoBehaviour doorScript = hit.collider.GetComponent("opencloseDoor") as MonoBehaviour;
                if (doorScript != null)
                {
                    if (IsDoorLocked(doorScript))
                    {
                        MoveToNextPatrolPoint();
                    }
                    else if (!IsDoorOpen(doorScript))
                    {
                        StartCoroutine(OpenDoorRoutine(doorScript));
                    }
                }
            }
        }
    }

    IEnumerator OpenDoorRoutine(MonoBehaviour doorScript)
    {
        isOpeningDoor = true;
        agent.isStopped = true;

        yield return new WaitForSeconds(0.5f);

        var method = doorScript.GetType().GetMethod("opening");
        if (method != null)
        {
            IEnumerator doorCoroutine = method.Invoke(doorScript, null) as IEnumerator;
            if (doorCoroutine != null)
                yield return StartCoroutine(doorCoroutine);
            else
                yield return new WaitForSeconds(0.5f);
        }

        agent.isStopped = false;
        isOpeningDoor = false;
    }

    bool IsDoorOpen(MonoBehaviour script)
    {
        var field = script.GetType().GetField("open");
        return field != null && (bool)field.GetValue(script);
    }

    bool IsDoorLocked(MonoBehaviour script)
    {
        var field = script.GetType().GetField("isLocked");
        return field != null && (bool)field.GetValue(script);
    }

    void MoveToNextPatrolPoint()
    {
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }

    public void OverrideToSearchPlayer(Vector3 lastKnownPlayerPosition)
    {
        if (overrideSearchRoutine != null)
            StopCoroutine(overrideSearchRoutine);

        overrideSearchRoutine = StartCoroutine(SearchAtPosition(lastKnownPlayerPosition));
    }

    IEnumerator SearchAtPosition(Vector3 position)
    {
        currentState = AIState.Searching;
        agent.isStopped = false;
        agent.SetDestination(position);

        while (Vector3.Distance(transform.position, position) > arrivalThreshold + 0.2f)
        {
            yield return null;
        }

        float searchTime = 3f;
        while (searchTime > 0f)
        {
            if (CanSeePlayer())
            {
                currentState = AIState.Chasing;
                overrideSearchRoutine = null;
                yield break;
            }

            searchTime -= Time.deltaTime;
            yield return null;
        }

        currentState = AIState.Patrolling;
        MoveToNextPatrolPoint();
        overrideSearchRoutine = null;
    }

    private void HandleActivityAlert(Vector3 alertPosition)
    {
        if (currentState == AIState.Chasing || isOpeningDoor) return;

        float distance = Vector3.Distance(transform.position, alertPosition);
        if (distance > 1f)
        {
            Debug.Log("[WifeAI] Heard activity at: " + alertPosition);
            OverrideToSearchPlayer(alertPosition);
        }
    }
}
