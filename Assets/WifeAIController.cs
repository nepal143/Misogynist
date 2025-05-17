using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class WifeAIController : MonoBehaviour
{
    public enum AIState { Patrolling, Investigating, Chasing }
    public AIState currentState = AIState.Patrolling;

    public float walkRadius = 10f;
    public float waitTime = 2f;
    public float movementSpeed = 3.5f;
    public float doorCheckDistance = 2f;

    private NavMeshAgent agent;
    private Animator animator;
    private float waitTimer = 0f;
    private bool isWaitingForDoor = false;

    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    public float stuckThreshold = 2f;

    [Header("Player Detection")]
    public Transform player;
    public float sightRange = 15f;
    public float fieldOfView = 120f;
    public LayerMask obstacleMask;

    [Header("Footstep Sounds")]
    public AudioClip[] footstepClips;
    private AudioSource footstepAudio;
    private float stepInterval = 0.5f;
    private float stepTimer = 0f;

    [Header("Sound Detection")]
    public float noiseThreshold = 1.0f;
    public float noiseResetTime = 3f;
    private float noiseTimer = 0f;
    private Vector3 investigateTarget;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.speed = movementSpeed;

        lastPosition = transform.position;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        footstepAudio = gameObject.AddComponent<AudioSource>();
        footstepAudio.spatialBlend = 1f;
        footstepAudio.rolloffMode = AudioRolloffMode.Linear;

        Debug.Log("Wife AI initialized. Starting patrol.");
        MoveToRandomPosition();
    }

    void Update()
    {
        if (isWaitingForDoor)
        {
            StopMovement();
            return;
        }

        if (CheckForDoorInPath()) return;

        if (player == null)
        {
            Debug.LogWarning("Player transform not assigned or found!");
            return;
        }

        bool seesPlayer = CanSeePlayer();
        Debug.Log($"[AI] CanSeePlayer: {seesPlayer}, CurrentState: {currentState}");

        if (seesPlayer)
        {
            if (currentState != AIState.Chasing)
            {
                Debug.Log("[AI] Player spotted! Switching to CHASING.");
                currentState = AIState.Chasing;
            }
            agent.SetDestination(player.position);
        }
        else
        {
            if (currentState == AIState.Chasing)
            {
                Debug.Log("[AI] Lost sight of player. Returning to PATROLLING.");
                currentState = AIState.Patrolling;
                MoveToRandomPosition();
            }
            else if (currentState == AIState.Investigating)
            {
                if (noiseTimer > 0f)
                {
                    noiseTimer -= Time.deltaTime;
                }
                else
                {
                    Debug.Log("[AI] Investigation time over. Returning to PATROLLING.");
                    currentState = AIState.Patrolling;
                    MoveToRandomPosition();
                }
            }
        }

        switch (currentState)
        {
            case AIState.Investigating:
                agent.SetDestination(investigateTarget);
                break;

            case AIState.Patrolling:
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    waitTimer += Time.deltaTime;
                    if (waitTimer >= waitTime)
                    {
                        MoveToRandomPosition();
                        waitTimer = 0f;
                    }
                }
                break;
        }

        UpdateAnimationsAndFootsteps();
    }

    bool CanSeePlayer()
    {
        if (!player) return false;

        Vector3 dirToPlayer = player.position - transform.position;
        float distance = dirToPlayer.magnitude;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        bool inRange = distance < sightRange;
        bool inFOV = angle < fieldOfView / 2f;

        bool lineOfSight = !Physics.Linecast(transform.position + Vector3.up, player.position + Vector3.up, obstacleMask);

        Debug.Log($"[AI] CanSeePlayer check - Distance: {distance:F2}, Angle: {angle:F2}, InRange: {inRange}, InFOV: {inFOV}, LineOfSight: {lineOfSight}");

        return inRange && inFOV && lineOfSight;
    }

    void UpdateAnimationsAndFootsteps()
    {
        bool isWalking = agent.velocity.magnitude > 0.1f;
        animator.SetBool("Walking", isWalking);
        animator.SetBool("Idle", !isWalking);

        if (isWalking)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepInterval)
            {
                PlayFootstepSound();
                stepTimer = 0f;
            }

            float movement = Vector3.Distance(transform.position, lastPosition);
            if (movement < 0.02f)
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer >= stuckThreshold)
                {
                    Debug.Log("[AI] Wife is stuck. Choosing new patrol point.");
                    MoveToRandomPosition();
                    stuckTimer = 0f;
                }
            }
            else
            {
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
            stepTimer = 0f;
        }

        lastPosition = transform.position;
    }

    void StopMovement()
    {
        agent.isStopped = true;
        animator.SetBool("Walking", false);
        animator.SetBool("Idle", true);
    }

    void PlayFootstepSound()
    {
        if (footstepClips.Length == 0) return;
        int index = Random.Range(0, footstepClips.Length);
        footstepAudio.PlayOneShot(footstepClips[index]);
    }
bool CheckForDoorInPath()
{
    if (agent.hasPath)
    {
        Vector3 direction = (agent.steeringTarget - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, agent.steeringTarget);

        RaycastHit[] hits = Physics.SphereCastAll(transform.position + Vector3.up, 0.5f, direction, Mathf.Min(distance, doorCheckDistance));
        foreach (var hit in hits)
        {
            GameObject go = hit.collider.gameObject;
            if (go.CompareTag("Door"))
            {
                MonoBehaviour doorScript = go.GetComponent("ClosetopencloseDoor") as MonoBehaviour ?? go.GetComponent("opencloseDoor") as MonoBehaviour;
                if (doorScript != null)
                {
                    if (IsDoorLocked(doorScript))
                    {
                        Debug.Log("[AI] Found a locked door. Abandoning path.");
                        if (currentState == AIState.Patrolling)
                        {
                            MoveToRandomPosition(); // pick a different patrol point
                        }
                        else if (currentState == AIState.Chasing || currentState == AIState.Investigating)
                        {
                            agent.ResetPath(); // stop moving
                        }
                        return false; // do not continue through this door
                    }

                    if (!IsDoorAlreadyOpen(doorScript))
                    {
                        Debug.Log("[AI] Door detected. Attempting to open.");
                        StartCoroutine(OpenDoorAndWait(doorScript));
                        return true;
                    }
                }
            }
        }
    }
    return false;
}
bool IsDoorLocked(MonoBehaviour doorScript)
{
    var lockedField = doorScript.GetType().GetField("isLocked");
    if (lockedField != null)
        return (bool)lockedField.GetValue(doorScript);

    return false;
}


    bool IsDoorAlreadyOpen(MonoBehaviour doorScript)
    {
        var openProp = doorScript.GetType().GetProperty("isOpen");
        if (openProp != null) return (bool)openProp.GetValue(doorScript);

        var openMethod = doorScript.GetType().GetMethod("GetIsOpen");
        if (openMethod != null) return (bool)openMethod.Invoke(doorScript, null);

        return false;
    }

    IEnumerator OpenDoorAndWait(MonoBehaviour doorScript)
    {
        isWaitingForDoor = true;
        StopMovement();

        yield return new WaitForSeconds(1f);

        var method = doorScript.GetType().GetMethod("opening");
        if (method != null)
        {
            IEnumerator coroutine = method.Invoke(doorScript, null) as IEnumerator;
            if (coroutine != null)
                yield return StartCoroutine(coroutine);
        }

        Collider doorCollider = doorScript.GetComponent<Collider>();
        if (doorCollider != null)
        {
            doorCollider.enabled = false;
            yield return new WaitForSeconds(1.5f);
            doorCollider.enabled = true;
        }

        isWaitingForDoor = false;
        agent.isStopped = false;

        // After opening door, resume previous behavior:
        if (currentState == AIState.Patrolling)
            MoveToRandomPosition();
        else if (currentState == AIState.Chasing)
            agent.SetDestination(player.position);
        else if (currentState == AIState.Investigating)
            agent.SetDestination(investigateTarget);
    }

    void MoveToRandomPosition()
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
            randomDirection.y = 0f;
            Vector3 potentialDestination = transform.position + randomDirection;

            if (NavMesh.SamplePosition(potentialDestination, out NavMeshHit hit, walkRadius, NavMesh.AllAreas))
            {
                float distance = Vector3.Distance(transform.position, hit.position);
                if (distance >= 5f && distance <= 15f)
                {
                    Debug.Log($"[AI] Moving to new patrol position: {hit.position}");
                    agent.SetDestination(hit.position);
                    return;
                }
            }
        }

        Debug.LogWarning("[AI] Failed to find a valid patrol destination.");
    }

    public void OnSoundHeard(Vector3 soundPosition, string sourceTag, float strength)
    {
        Debug.Log($"[AI] Heard noise from {sourceTag} at {soundPosition}, strength: {strength}");

        if (sourceTag == "Player" && !CanSeePlayer() && strength >= noiseThreshold)
        {
            investigateTarget = soundPosition;
            noiseTimer = noiseResetTime;

            if (currentState != AIState.Investigating)
            {
                Debug.Log("[AI] Switching to INVESTIGATING state due to sound.");
                currentState = AIState.Investigating;
            }
        }
        else
        {
            Debug.Log("[AI] Ignored sound (either not player, player visible, or below threshold).");
        }
    }
}
