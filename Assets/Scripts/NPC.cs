using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using System.Collections;

public class NPC : MonoBehaviourPunCallbacks
{
    public Transform[] waypoints;
    public float idleTime = 2f; // Time to idle at each waypoint
    private int currentWaypoint = 0;
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private bool isIdle = false;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (waypoints.Length > 0)
        {
            navMeshAgent.SetDestination(waypoints[currentWaypoint].position);
        }
    }

    void Update()
    {
        if (!photonView.IsMine)
            return;

        if (!isIdle && navMeshAgent.remainingDistance < 0.5f)
        {
            StartCoroutine(IdleAndMove());
        }

        UpdateAnimator();
    }

    IEnumerator IdleAndMove()
    {
        isIdle = true;
        animator.SetBool("isWalking", false);

        yield return new WaitForSeconds(idleTime);

        currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        navMeshAgent.SetDestination(waypoints[currentWaypoint].position);

        isIdle = false;
        animator.SetBool("isWalking", true);
    }

    void UpdateAnimator()
    {
        if (navMeshAgent.velocity.magnitude > 0.1f)
        {
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InteractWithPlayer(other.gameObject);
        }
    }

    void InteractWithPlayer(GameObject player)
    {
        // Example interaction code
        Debug.Log("NPC interacting with " + player.name);
    }

    // Method to randomly select the next waypoint
    void SelectRandomWaypoint()
    {
        currentWaypoint = Random.Range(0, waypoints.Length);
        navMeshAgent.SetDestination(waypoints[currentWaypoint].position);
    }

    // Example method to handle NPC damage
    [PunRPC]
    public void TakeDamage(int damage)
    {
        // Handle NPC damage logic here
        Debug.Log("NPC took " + damage + " damage.");
    }
}
