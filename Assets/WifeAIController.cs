using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class WifeAIController : MonoBehaviour
{
    public float walkRadius = 10f;
    public float waitTime = 2f;
    public float movementSpeed = 3.5f;
    public float doorCheckDistance = 2f; // distance to check for doors in path

    private NavMeshAgent agent;
    private Animator animator;
    private float waitTimer = 0f;
    private bool isWaitingForDoor = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.speed = movementSpeed;
        agent.updatePosition = true;
        agent.updateRotation = true;

        MoveToRandomPosition();
    }

    void Update()
    {
        if (isWaitingForDoor)
        {
            // Currently waiting for door to open, do nothing else
            animator.SetBool("Walking", false);
            animator.SetBool("Idle", true);
            agent.isStopped = true;
            return;
        }

        // Check if next door is nearby and needs to be opened
        if (CheckForDoorInPath())
        {
            return; // Door found and coroutine started; skip rest of Update this frame
        }

        // Normal walking animation control
        bool isWalking = agent.velocity.magnitude > 0.1f;

        animator.SetBool("Walking", isWalking);
        animator.SetBool("Idle", !isWalking);

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
        // Cast a sphere along agent's path to detect nearby doors
        if (agent.hasPath)
        {
            Vector3 nextPos = agent.steeringTarget; // next target position agent is moving toward
            Vector3 direction = (nextPos - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, nextPos);

            RaycastHit[] hits = Physics.SphereCastAll(transform.position + Vector3.up * 1f, 0.5f, direction, Mathf.Min(distance, doorCheckDistance));
            foreach (var hit in hits)
            {
                GameObject go = hit.collider.gameObject;
                if (go.CompareTag("Door"))
                {
                    // Check for scripts ClosetopencloseDoor or opencloseDoor
                    MonoBehaviour doorScript = go.GetComponent("ClosetopencloseDoor") as MonoBehaviour;
                    if (doorScript == null)
                    {
                        doorScript = go.GetComponent("opencloseDoor") as MonoBehaviour;
                    }

                    if (doorScript != null)
                    {
                        // Found a door with the right script - start coroutine to open it
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
    agent.isStopped = true;

    // Idle for 1 second before opening the door
    animator.SetBool("Walking", false);
    animator.SetBool("Idle", true);
    yield return new WaitForSeconds(1f);

    // Try to call the opening() coroutine on the door
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
            yield return new WaitForSeconds(0.5f);
        }
    }
    else
    {
        Debug.LogWarning("Door script does not have an opening() coroutine method.");
        yield return new WaitForSeconds(0.5f);
    }

    // 🚨 Force the wife to walk forward no matter what
    Debug.Log("FORCING wife to move forward through the door...");

    animator.SetBool("Walking", true);
    animator.SetBool("Idle", false);
    agent.isStopped = false;

    Vector3 forwardTarget = transform.position + transform.forward * 2f;
    agent.SetDestination(forwardTarget);

    // Give her 1 second to move forward
    yield return new WaitForSeconds(1f);

    // Resume normal behavior
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
