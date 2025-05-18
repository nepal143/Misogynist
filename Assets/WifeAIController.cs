using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using SojaExiles;
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
    public Transform eyePoint;
    public float sightRange = 20f;
    public float fieldOfView = 110f;

    [Header("Footsteps")]
    public AudioClip[] footstepClips;
    public float stepInterval = 0.5f;

    [Header("Patrol Points")]
    public Transform[] patrolPoints;

    [Header("Hit Settings")]
public GameObject playerGameObject;  // assign your player GameObject in inspector
public Camera hitCamera;    
private bool hasHitPlayer = false;

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

        // Assign player by tag at start if not already assigned
        if (player == null)
        {
            GameObject found = GameObject.FindWithTag("Player");
            if (found != null)
            {
                player = found.transform;
                Debug.Log("[WifeAI] Player found on Start.");
            }
        }

        if (PlayerActivityTracker.Instance != null)
            PlayerActivityTracker.Instance.OnActivityAlert += HandleActivityAlert;

        if (patrolPoints.Length > 0)
            MoveToNextPatrolPoint();

        lastPos = transform.position;
    }

    void Update()
    {
         if (hasHitPlayer)
    {
        // Stop any further AI logic after hitting player
        return;
    }
        // 1. Reacquire player if missing or inactive
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            GameObject found = GameObject.FindWithTag("Player");
            if (found != null && found.activeInHierarchy)
            {
                player = found.transform;
                Debug.Log("[WifeAI] Player reacquired.");

                if (isTrackingPlayer)
                {
                    Debug.Log("[WifeAI] Resuming chase.");
                    currentState = AIState.Chasing;
                }
            }
            else
            {
                // Player still missing - DON'T return here, keep AI working on patrol, etc.
                player = null; // ensure explicitly null
            }
        }

        // 2. Process AI states & behaviors
        switch (currentState)
        {
            case AIState.Patrolling:
                Patrol();
                break;
            case AIState.Investigating:
                Investigate();
                break;
            case AIState.Chasing:
                ChasePlayer();
                break;
            case AIState.Searching:
                // Searching state handled by coroutine; do nothing special here
                break;
        }

        // 3. Always handle footsteps, stuck checks, and door opening regardless of player presence
        HandleFootsteps();
        CheckIfStuck();
        TryOpenDoors();

        // 4. If tracking player but state changed away from chasing (e.g. lost sight), log it
        if (isTrackingPlayer && currentState != AIState.Chasing)
        {
            Debug.Log("[WifeAI] Player visible again? Resuming chase.");
            currentState = AIState.Chasing;
        }
    }

    void Patrol()
    {
        if (!agent.pathPending && (!agent.hasPath || agent.remainingDistance <= arrivalThreshold))
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
            isTrackingPlayer = true;
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
                Debug.Log("[WifeAI] Done investigating. Returning to patrol.");
                currentState = AIState.Patrolling;
                MoveToNextPatrolPoint();
            }
        }

        if (CanSeePlayer())
        {
            isTrackingPlayer = true;
            currentState = AIState.Chasing;
        }
    }

    void ChasePlayer()
    {
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            Debug.Log("[WifeAI] Player vanished. Switching to investigate.");
            isTrackingPlayer = false; // lost player, stop tracking
            // Switch to investigating the last known position only if we have it
            if (investigateTarget != Vector3.zero)
            {
                investigateTimer = 4f;
                currentState = AIState.Investigating;
            }
            else
            {
                // No known position? Go back to patrol
                currentState = AIState.Patrolling;
                MoveToNextPatrolPoint();
            }
            return;
        }

        isTrackingPlayer = true;

        if (CanSeePlayer())
        {
            if (NavMesh.SamplePosition(player.position, out NavMeshHit hit, 1f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
            else if (NavMesh.SamplePosition(player.position, out hit, 10f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
            else
                Debug.LogWarning("[WifeAI] Cannot find NavMesh near player.");

            if (Vector3.Distance(transform.position, player.position) <= 2f)
                HitPlayer();
        }
        else
        {
            investigateTarget = player.position;
            investigateTimer = 4f;
            currentState = AIState.Investigating;
            Debug.Log("[WifeAI] Lost sight of player. Investigating last seen location.");
            isTrackingPlayer = false;
        }
    }

    bool CanSeePlayer()
    {
        if (player == null || eyePoint == null || !player.gameObject.activeInHierarchy) return false;

        Vector3 dir = player.position - eyePoint.position;
        float dist = dir.magnitude;
        float angle = Vector3.Angle(eyePoint.forward, dir);

        if (dist > sightRange || angle > fieldOfView / 2f) return false;

        if (Physics.Raycast(eyePoint.position, dir.normalized, out RaycastHit hit, sightRange, ~0, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawRay(eyePoint.position, dir.normalized * hit.distance, Color.green, 0.2f);
            return hit.transform == player;
        }

        return false;
    }

void HitPlayer()
{
    if (hasHitPlayer) return; // prevent double hitting

    Debug.Log("[WifeAI] Hit Player!");

    hasHitPlayer = true;

    if (playerGameObject != null)
        playerGameObject.SetActive(false);

    if (hitCamera != null)
        hitCamera.enabled = true;

    if (animator != null)
        animator.SetBool("Hit", true);  // set the bool parameter "Hit" to true

    if (agent != null)
        agent.isStopped = true;
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
                    footstepAudio.PlayOneShot(footstepClips[Random.Range(0, footstepClips.Length)]);
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
                Debug.Log("[WifeAI] Stuck detected. Switching patrol point.");
                MoveToNextPatrolPoint();
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
    if (!agent.hasPath || isOpeningDoor) return;

    Vector3 dir = agent.steeringTarget - transform.position;
    if (Physics.Raycast(transform.position + Vector3.up, dir.normalized, out RaycastHit hit, 2f))
    {
        if (hit.collider.CompareTag("Door"))
        {
            // Get your door script of the correct type (opencloseDoor)
            opencloseDoor doorScript = hit.collider.GetComponent<opencloseDoor>();
            if (doorScript != null)
            {
                if (doorScript.isLocked)
                {
                    Debug.Log("[WifeAI] Door is locked. Switching patrol point.");
                    MoveToNextPatrolPoint();  // change patrol point if door locked
                    return;
                }
                else if (!doorScript.open)
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
            IEnumerator coroutine = method.Invoke(doorScript, null) as IEnumerator;
            if (coroutine != null)
                yield return StartCoroutine(coroutine);
        }

        // Disable BoxCollider after opening door
        BoxCollider boxCollider = doorScript.GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
            Debug.Log("[WifeAI] Disabled BoxCollider on door.");

            // Wait 2 seconds before enabling the collider again
            yield return new WaitForSeconds(2f);
            boxCollider.enabled = true;
            Debug.Log("[WifeAI] Re-enabled BoxCollider on door.");
        }

        agent.isStopped = false;
        isOpeningDoor = false;
    }

    bool IsDoorLocked(MonoBehaviour doorScript)
    {
        var prop = doorScript.GetType().GetProperty("isLocked");
        return prop != null && (bool)prop.GetValue(doorScript);
    }

    bool IsDoorOpen(MonoBehaviour doorScript)
    {
        var prop = doorScript.GetType().GetProperty("isOpen");
        return prop != null && (bool)prop.GetValue(doorScript);
    }

    void MoveToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        Debug.Log("[WifeAI] Moving to patrol point " + currentPatrolIndex);
    }

    void HandleActivityAlert(Vector3 position)
    {
        if (currentState == AIState.Chasing) return; // Ignore if already chasing

        investigateTarget = position;
        investigateTimer = 4f;
        currentState = AIState.Investigating;
        Debug.Log("[WifeAI] Investigating noise at " + position);
    }
}
