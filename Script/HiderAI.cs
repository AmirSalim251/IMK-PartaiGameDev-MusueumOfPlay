using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HiderAI : MonoBehaviour
{
    public List<Transform> coverSpots;
    private NavMeshAgent agent;
    private Transform currentCoverSpot;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        MoveToNewCoverSpot();
    }

    void MoveToNewCoverSpot()
    {
        if (coverSpots.Count == 0) return;
        currentCoverSpot = coverSpots[Random.Range(0, coverSpots.Count)];
        agent.SetDestination(currentCoverSpot.position);
    }

    void Update()
    {
        if (agent.remainingDistance < 0.5f)
        {
            StartCoroutine(HideRoutine());
        }
    }

    IEnumerator HideRoutine()
    {
        float hideTime = Random.Range(2, 5);
        yield return new WaitForSeconds(hideTime);
        MoveToNewCoverSpot();
    }
}