using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class WifeAIController : MonoBehaviour
{
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
    public float stuckThreshold = 2f; // seconds of no movement before rerouting

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.speed = movementSpeed;
        agent.updatePosition = true;
        agent.updateRotation = true;

        lastPosition = transform.position;
        MoveToRandomPosition();
    }

   void Update()
{
    if (isWaitingForDoor)
    {
        animator.SetBool("Walking", false);
        animator.SetBool("Idle", true);
        agent.isStopped = true;
        return;
    }

    if (CheckForDoorInPath())
    {
        return;
    }

    bool isWalking = agent.velocity.magnitude > 0.1f;
    animator.SetBool("Walking", isWalking);
    animator.SetBool("Idle", !isWalking);

    // 🚨 Check if agent is stuck while trying to move
    float movement = Vector3.Distance(transform.position, lastPosition);
    if (isWalking)
    {
        if (movement < 0.02f) // almost no movement
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= stuckThreshold)
            {
                Debug.LogWarning("Wife is stuck! Changing path.");
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
        stuckTimer = 0f; // reset if not trying to walk
    }

    lastPosition = transform.position;

    if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
    {
        waitTimer += Time.deltaTime;
        if (waitTimer >= waitTime)
        {
            MoveToRandomPosition();
            waitTimer = 0f;
        }
    }
}

    bool CheckForDoorInPath()
    {
        if (agent.hasPath)
        {
            Vector3 nextPos = agent.steeringTarget;
            Vector3 direction = (nextPos - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, nextPos);

            RaycastHit[] hits = Physics.SphereCastAll(transform.position + Vector3.up * 1f, 0.5f, direction, Mathf.Min(distance, doorCheckDistance));
            foreach (var hit in hits)
            {
                GameObject go = hit.collider.gameObject;
                if (go.CompareTag("Door"))
                {
                    MonoBehaviour doorScript = go.GetComponent("ClosetopencloseDoor") as MonoBehaviour;
                    if (doorScript == null)
                        doorScript = go.GetComponent("opencloseDoor") as MonoBehaviour;

                    if (doorScript != null)
                    {
                        StartCoroutine(OpenDoorAndWait(doorScript));
                        return true;
                    }
                }
            }
        }
        return false;
    }

IEnumerator OpenDoorAndWait(MonoBehaviour doorScript)
{
    isWaitingForDoor = true;
    agent.isStopped = true;IEnumerator OpenDoorAndWait(MonoBehaviour doorScript)
{
    isWaitingForDoor = true;
    agent.isStopped = true;

    animator.SetBool("Walking", false);
    animator.SetBool("Idle", true);
    yield return new WaitForSeconds(1f);

    // Call door's opening coroutine
    var method = doorScript.GetType().GetMethod("opening");
    if (method != null)
    {
        IEnumerator doorOpeningCoroutine = method.Invoke(doorScript, null) as IEnumerator;
        if (doorOpeningCoroutine != null)
        {
            yield return StartCoroutine(doorOpeningCoroutine);
        }
        else
        {
            Debug.LogWarning("Door script's opening() did not return IEnumerator.");
            yield return new WaitForSeconds(1f); // small wait just in case
        }
    }
    else
    {
        Debug.LogWarning("Door script does not have an opening() coroutine method.");
        yield return new WaitForSeconds(1f);
    }

    // ✅ Door is open, continue walking
    animator.SetBool("Walking", true);
    animator.SetBool("Idle", false);

    agent.isStopped = false;
    MoveToRandomPosition(); // Continue pathfinding

    isWaitingForDoor = false;
}


    animator.SetBool("Walking", false);
    animator.SetBool("Idle", true);
    yield return new WaitForSeconds(1f);

    // Call door's opening coroutine
    var method = doorScript.GetType().GetMethod("opening");
    if (method != null)
    {
        IEnumerator doorOpeningCoroutine = method.Invoke(doorScript, null) as IEnumerator;
        if (doorOpeningCoroutine != null)
        {
            yield return StartCoroutine(doorOpeningCoroutine);
        }
        else
        {
            Debug.LogWarning("Door script's opening() did not return IEnumerator.");
            yield return new WaitForSeconds(1f); // small wait just in case
        }
    }
    else
    {
        Debug.LogWarning("Door script does not have an opening() coroutine method.");
        yield return new WaitForSeconds(1f);
    }

    // ✅ Door is open, continue walking
    animator.SetBool("Walking", true);
    animator.SetBool("Idle", false);

    agent.isStopped = false;
    MoveToRandomPosition(); // Continue pathfinding

    isWaitingForDoor = false;
}


    void MoveToRandomPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, walkRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            Debug.Log("Moving to: " + hit.position);
        }
        else
        {
            Debug.LogWarning("No valid NavMesh point found.");
        }
    }
}
