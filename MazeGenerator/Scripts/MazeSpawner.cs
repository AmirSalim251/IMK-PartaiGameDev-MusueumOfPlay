using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.AI.Navigation;

public class MazeSpawner : MonoBehaviour
{
    public enum MazeGenerationAlgorithm
    {
        PureRecursive,
        RecursiveTree,
        RandomTree,
        OldestTree,
        RecursiveDivision,
    }

    public MazeGenerationAlgorithm Algorithm = MazeGenerationAlgorithm.PureRecursive;
    public bool FullRandom = false;
    public int RandomSeed = 12345;
    public GameObject Floor = null;
    public GameObject Wall = null;
    public GameObject Pillar = null;
    public int Rows = 5;
    public int Columns = 5;
    public float CellWidth = 5;
    public float CellHeight = 5;
    public bool AddGaps = false; // Ensure gaps are removed
    public GameObject GoalPrefab = null;
    public GameObject EnemyPrefab = null;
    public Transform player;
    public int enemyCount = 5;
    public float detectionRadius = 10f;
    public float fieldOfViewAngle = 110f;

    private BasicMazeGenerator mMazeGenerator = null;
    private List<NavMeshAgent> enemies = new List<NavMeshAgent>();
    private NavMeshSurface navMeshSurface;

    void Start()
    {
        if (!FullRandom)
        {
            Random.seed = RandomSeed;
        }
        switch (Algorithm)
        {
            case MazeGenerationAlgorithm.PureRecursive:
                mMazeGenerator = new RecursiveMazeGenerator(Rows, Columns);
                break;
            case MazeGenerationAlgorithm.RecursiveTree:
                mMazeGenerator = new RecursiveTreeMazeGenerator(Rows, Columns);
                break;
            case MazeGenerationAlgorithm.RandomTree:
                mMazeGenerator = new RandomTreeMazeGenerator(Rows, Columns);
                break;
            case MazeGenerationAlgorithm.OldestTree:
                mMazeGenerator = new OldestTreeMazeGenerator(Rows, Columns);
                break;
            case MazeGenerationAlgorithm.RecursiveDivision:
                mMazeGenerator = new DivisionMazeGenerator(Rows, Columns);
                break;
        }
        mMazeGenerator.GenerateMaze();
        GenerateMaze();
        BakeNavMesh();
    }

    void Update()
    {
        UpdateEnemyMovement();
    }

    private void GenerateMaze()
    {
        for (int row = 0; row < Rows; row++)
        {
            for (int column = 0; column < Columns; column++)
            {
                float x = column * CellWidth;
                float z = row * CellHeight;
                MazeCell cell = mMazeGenerator.GetMazeCell(row, column);
                GameObject tmp;

                // Overlap floor tiles slightly to cover gaps
                float floorOverlap = 0.1f;
                tmp = Instantiate(Floor, new Vector3(x, 0, z), Quaternion.identity) as GameObject;
                tmp.transform.localScale = new Vector3(CellWidth + floorOverlap, 1, CellHeight + floorOverlap);
                tmp.transform.parent = transform;

                if (cell.WallRight)
                {
                    tmp = Instantiate(Wall, new Vector3(x + CellWidth / 2, 0, z), Quaternion.Euler(0, 90, 0)) as GameObject; // right
                    tmp.transform.parent = transform;
                    AddNavMeshObstacle(tmp);
                }
                if (cell.WallFront)
                {
                    tmp = Instantiate(Wall, new Vector3(x, 0, z + CellHeight / 2), Quaternion.identity) as GameObject; // front
                    tmp.transform.parent = transform;
                    AddNavMeshObstacle(tmp);
                }
                if (cell.WallLeft)
                {
                    tmp = Instantiate(Wall, new Vector3(x - CellWidth / 2, 0, z), Quaternion.Euler(0, 270, 0)) as GameObject; // left
                    tmp.transform.parent = transform;
                    AddNavMeshObstacle(tmp);
                }
                if (cell.WallBack)
                {
                    tmp = Instantiate(Wall, new Vector3(x, 0, z - CellHeight / 2), Quaternion.Euler(0, 180, 0)) as GameObject; // back
                    tmp.transform.parent = transform;
                    AddNavMeshObstacle(tmp);
                }
                if (cell.IsGoal && GoalPrefab != null)
                {
                    tmp = Instantiate(GoalPrefab, new Vector3(x, 1, z), Quaternion.identity) as GameObject;
                    tmp.transform.parent = transform;
                }
            }
        }
        if (Pillar != null)
        {
            for (int row = 0; row < Rows + 1; row++)
            {
                for (int column = 0; column < Columns + 1; column++)
                {
                    float x = column * CellWidth;
                    float z = row * CellHeight;
                    GameObject tmp = Instantiate(Pillar, new Vector3(x - CellWidth / 2, 0, z - CellHeight / 2), Quaternion.identity) as GameObject;
                    tmp.transform.parent = transform;
                    AddNavMeshObstacle(tmp);
                }
            }
        }
    }

    private void BakeNavMesh()
    {
        navMeshSurface = gameObject.AddComponent<NavMeshSurface>();
        navMeshSurface.BuildNavMesh();
        Invoke("SpawnEnemies", 0.1f); // Delay enemy spawning to ensure NavMesh is baked
    }

    private void SpawnEnemies()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            int randomRow = Random.Range(0, Rows);
            int randomColumn = Random.Range(0, Columns);
            float x = randomColumn * CellWidth;
            float z = randomRow * CellHeight;
            GameObject enemy = Instantiate(EnemyPrefab, new Vector3(x, 1, z), Quaternion.identity);
            enemy.transform.parent = transform;
            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                enemies.Add(agent);
            }
        }
    }

    private void UpdateEnemyMovement()
    {
        foreach (NavMeshAgent enemy in enemies)
        {
            float distance = Vector3.Distance(enemy.transform.position, player.position);
            if (distance <= detectionRadius && IsPlayerInFieldOfView(enemy.transform))
            {
                Debug.Log("Player seen by enemy");
                enemy.SetDestination(player.position);
            }
            else
            {
                Patrol(enemy);
            }
        }
    }

   private bool IsPlayerInFieldOfView(Transform enemy)
{
    Vector3 directionToPlayer = (player.position - enemy.position).normalized;
    float angleToPlayer = Vector3.Angle(enemy.forward, directionToPlayer);

    Debug.Log($"Angle to player: {angleToPlayer}, Enemy FOV: {fieldOfViewAngle / 2f}");
    
    if (angleToPlayer < fieldOfViewAngle / 2f)
    {
        Debug.Log("Player within FOV angle");
        
        if (Physics.Raycast(enemy.position, directionToPlayer, out RaycastHit hit, detectionRadius))
        {
            Debug.Log($"Raycast hit: {hit.transform.name}");
            
            if (hit.transform == player)
            {
                Debug.Log("Player detected within FOV and detection radius");
                return true;
            }
        }
        else
        {
            Debug.Log("Raycast did not hit player");
        }
    }
    else
    {
        Debug.Log("Player not within FOV angle");
    }
    
    return false;
}


    private void Patrol(NavMeshAgent enemy)
    {
        // Implement simple patrolling logic here
        // Example: moving to random points within the maze
        if (!enemy.hasPath)
        {
            Vector3 randomDirection = Random.insideUnitSphere * detectionRadius;
            randomDirection += enemy.transform.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, detectionRadius, 1))
            {
                enemy.SetDestination(hit.position);
            }
        }
    }

    private void AddNavMeshObstacle(GameObject obj)
    {
        NavMeshObstacle navObstacle = obj.AddComponent<NavMeshObstacle>();
        navObstacle.carving = true;
    }
}
