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

    [Header("Patrol Points")]
    public Transform[] patrolPoints;

    [Header("Hit Settings")]
    public GameObject playerGameObject;
    public Camera hitCamera;
    private bool hasHitPlayer = false;

    [Header("Audio Clips Per State")]
    public AudioClip[] patrollingClips;
    public AudioClip[] chasingClips;
    public AudioClip[] investigatingClips;
    public AudioClip[] searchingClips;
    public AudioClip[] hitPlayerClips;

    private AudioSource stateAudioSource;
    private float stateAudioCooldown = 15f;
    private float nextAudioTime = 0f;

    private int currentPatrolIndex = 0;
    private NavMeshAgent agent;
    private Animator animator;

    private float waitTimer;
    private float stuckTimer;
    private float investigateTimer;

    private Vector3 lastPos;
    private Vector3 investigateTarget;
    private bool isOpeningDoor;

    private bool isTrackingPlayer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        stateAudioSource = gameObject.AddComponent<AudioSource>();
        stateAudioSource.loop = false;
        stateAudioSource.spatialBlend = 1f;

        agent.speed = movementSpeed;
        agent.stoppingDistance = arrivalThreshold;

        if (player == null)
        {
            GameObject found = GameObject.FindWithTag("Player");
            if (found != null) player = found.transform;
        }

        if (PlayerActivityTracker.Instance != null)
            PlayerActivityTracker.Instance.OnActivityAlert += HandleActivityAlert;

        if (patrolPoints.Length > 0)
            MoveToNextPatrolPoint();

        lastPos = transform.position;
        PlayStateAudio(currentState);
    }

    void Update()
    {
        if (hasHitPlayer) return;

        if (player == null || !player.gameObject.activeInHierarchy)
        {
            GameObject found = GameObject.FindWithTag("Player");
            if (found != null && found.activeInHierarchy)
            {
                player = found.transform;
                if (isTrackingPlayer)
                {
                    currentState = AIState.Chasing;
                    PlayStateAudio(currentState);
                }
            }
            else
            {
                player = null;
            }
        }

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
                break;
        }

        CheckIfStuck();
        TryOpenDoors();

        if (isTrackingPlayer && currentState != AIState.Chasing)
        {
            currentState = AIState.Chasing;
            PlayStateAudio(currentState);
        }

        // Try playing state clip again if 15s has passed
        if (Time.time >= nextAudioTime && !stateAudioSource.isPlaying)
        {
            PlayStateAudio(currentState);
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
            PlayStateAudio(currentState);
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
                PlayStateAudio(currentState);
                MoveToNextPatrolPoint();
            }
        }

        if (CanSeePlayer())
        {
            isTrackingPlayer = true;
            currentState = AIState.Chasing;
            PlayStateAudio(currentState);
        }
    }

    void ChasePlayer()
    {
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            isTrackingPlayer = false;
            if (investigateTarget != Vector3.zero)
            {
                investigateTimer = 4f;
                currentState = AIState.Investigating;
                PlayStateAudio(currentState);
            }
            else
            {
                currentState = AIState.Patrolling;
                PlayStateAudio(currentState);
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

            if (Vector3.Distance(transform.position, player.position) <= 2f)
                HitPlayer();
        }
        else
        {
            investigateTarget = player.position;
            investigateTimer = 4f;
            currentState = AIState.Investigating;
            PlayStateAudio(currentState);
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
            return hit.transform == player;
        }

        return false;
    }

    void HitPlayer()
    {
        if (hasHitPlayer) return;

        hasHitPlayer = true;

        if (playerGameObject != null)
            playerGameObject.SetActive(false);

        if (hitCamera != null)
            hitCamera.enabled = true;

        if (animator != null)
            animator.SetBool("Hit", true);

        if (agent != null)
            agent.isStopped = true;

        stateAudioSource.Stop();
        PlayOneShotRandom(hitPlayerClips);
    }

    void CheckIfStuck()
    {
        float moved = Vector3.Distance(transform.position, lastPos);
        if (moved < 0.05f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > stuckCheckTime)
            {
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
                opencloseDoor doorScript = hit.collider.GetComponent<opencloseDoor>();
                if (doorScript != null)
                {
                    if (doorScript.isLocked)
                    {
                        MoveToNextPatrolPoint();
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

        BoxCollider boxCollider = doorScript.GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
            yield return new WaitForSeconds(2f);
            boxCollider.enabled = true;
        }

        agent.isStopped = false;
        isOpeningDoor = false;
    }

    void MoveToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }

    void HandleActivityAlert(Vector3 position)
    {
        if (currentState == AIState.Chasing) return;

        investigateTarget = position;
        investigateTimer = 4f;
        currentState = AIState.Investigating;
        PlayStateAudio(currentState);
    }

    void PlayStateAudio(AIState state)
    {
        AudioClip clip = GetRandomClipForState(state);
        if (clip != null)
        {
            stateAudioSource.PlayOneShot(clip);
            nextAudioTime = Time.time + stateAudioCooldown;
        }
    }

    void PlayOneShotRandom(AudioClip[] clips)
    {
        AudioClip clip = GetRandomClip(clips);
        if (clip != null)
            stateAudioSource.PlayOneShot(clip);
    }

    AudioClip GetRandomClipForState(AIState state)
    {
        switch (state)
        {
            case AIState.Patrolling: return GetRandomClip(patrollingClips);
            case AIState.Investigating: return GetRandomClip(investigatingClips);
            case AIState.Chasing: return GetRandomClip(chasingClips);
            case AIState.Searching: return GetRandomClip(searchingClips);
            default: return null;
        }
    }

    AudioClip GetRandomClip(AudioClip[] clips)
    {
        if (clips != null && clips.Length > 0)
            return clips[Random.Range(0, clips.Length)];
        return null;
    }
}
