using UnityEngine;
using UnityEngine.AI;
public class SeekerAI : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform player;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        //player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        agent.SetDestination(player.position);
    }
}