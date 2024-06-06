using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    public bool isSeeker = false;
    public float moveSpeed = 5f;
    public List<Transform> coverSpots;
    public List<Transform> patrolPoints;
    private NavMeshAgent agent;
    private Transform targetSpot;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (!isSeeker)
        {
            MoveToNewCoverSpot();
        }
    }

    void Update()
    {
        HandleMovement();

        if (Input.GetKeyDown(KeyCode.T))
        {
            isSeeker = !isSeeker;
        }
        
        if (!isSeeker && agent.remainingDistance < 0.5f)
        {
            StartCoroutine(HideRoutine());
        }
    }

    void HandleMovement()
    {
        if (isSeeker)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 movement = new Vector3(horizontal, 0, vertical).normalized * moveSpeed * Time.deltaTime;
            agent.Move(movement);

            DetectHiders();
        }
        else
        {
            // Player as hider, use AI movement to cover spots
            if (agent.remainingDistance < 0.5f)
            {
                MoveToNewCoverSpot();
            }
        }
    }

    void MoveToNewCoverSpot()
    {
        if (coverSpots.Count == 0) return;
        targetSpot = coverSpots[Random.Range(0, coverSpots.Count)];
        agent.SetDestination(targetSpot.position);
    }

    IEnumerator HideRoutine()
    {
        float hideTime = Random.Range(2, 5);
        yield return new WaitForSeconds(hideTime);
        MoveToNewCoverSpot();
    }

    void DetectHiders()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 10f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Hider"))
            {
                // Implement chase logic
            }
        }
    }
}