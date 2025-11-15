using UnityEngine;
using System.Collections.Generic;

public class LevelSpawner : MonoBehaviour
{
    [Header("Level Prefabs")]
    [SerializeField] private GameObject defaultLevelPrefab; // Level 1 (spawns at start)
    [SerializeField] private GameObject[] levelPrefabs; // Level 2, 3, 4 (random spawn)

    [Header("Spawn Settings")]
    [SerializeField] private float levelLength = 60.15f;
    [SerializeField] private int poolSize = 5; // Number of levels to pre-instantiate

    [Header("References")]
    [SerializeField] private Transform player;

    private Queue<GameObject> levelPool;
    private List<GameObject> activeLevels;
    private float nextSpawnPosition;
    private float despawnPosition;

    void Start()
    {
        activeLevels = new List<GameObject>();
        InitializePool();

        // Spawn the default level at (0, 0, 0)
        SpawnLevel(defaultLevelPrefab, 0f);
        nextSpawnPosition = levelLength;
        despawnPosition = -levelLength;
    }

    void Update()
    {
        if (player == null) return;

        // Spawn new level when player gets close
        if (player.position.x > nextSpawnPosition - levelLength * 2)
        {
            SpawnRandomLevel();
        }

        // Despawn levels that are behind the player
        DespawnOldLevels();
    }

    void InitializePool()
    {
        levelPool = new Queue<GameObject>();

        // Pre-instantiate levels for the pool
        for (int i = 0; i < poolSize; i++)
        {
            // Randomly pick a level prefab to add to pool
            GameObject prefab = levelPrefabs[Random.Range(0, levelPrefabs.Length)];
            GameObject level = Instantiate(prefab);
            level.SetActive(false);
            levelPool.Enqueue(level);
        }
    }

    void SpawnLevel(GameObject prefab, float xPosition)
    {
        GameObject level;

        // If spawning default level, instantiate it directly
        if (prefab == defaultLevelPrefab)
        {
            level = Instantiate(prefab);
        }
        else
        {
            // Try to get from pool
            if (levelPool.Count > 0)
            {
                level = levelPool.Dequeue();
                level.SetActive(true);
            }
            else
            {
                // Pool is empty, create new instance
                level = Instantiate(prefab);
            }
        }

        level.transform.position = new Vector3(xPosition, 0f, 0f);
        activeLevels.Add(level);
    }

    void SpawnRandomLevel()
    {
        // Pick a random level from the array (excluding default)
        GameObject randomPrefab = levelPrefabs[Random.Range(0, levelPrefabs.Length)];
        SpawnLevel(randomPrefab, nextSpawnPosition);
        nextSpawnPosition += levelLength;
    }

    void DespawnOldLevels()
    {
        if (activeLevels.Count == 0) return;

        // Check if the first active level is behind the player
        GameObject firstLevel = activeLevels[0];
        if (firstLevel.transform.position.x < player.position.x - levelLength)
        {
            activeLevels.RemoveAt(0);

            // Despawn power-ups in this level before returning to pool
            PowerUpSpawner powerUpSpawner = firstLevel.GetComponent<PowerUpSpawner>();
            if (powerUpSpawner != null)
            {
                powerUpSpawner.DespawnAllPowerUps();
            }


            // Don't pool the default level, destroy it
            if (firstLevel.name.Contains(defaultLevelPrefab.name))
            {
                Destroy(firstLevel);
            }
            else
            {
                // Return to pool
                firstLevel.SetActive(false);
                levelPool.Enqueue(firstLevel);
            }
        }
    }

    public void ResetSpawner()
    {
        // Clear all active levels
        foreach (GameObject level in activeLevels)
        {
            if (level.name.Contains(defaultLevelPrefab.name))
            {
                Destroy(level);
            }
            else
            {
                level.SetActive(false);
                levelPool.Enqueue(level);
            }
        }
        activeLevels.Clear();

        // Reset spawn positions
        nextSpawnPosition = levelLength;
        despawnPosition = -levelLength;

        // Spawn default level again
        SpawnLevel(defaultLevelPrefab, 0f);
    }

    void OnDrawGizmos()
    {
        // Visualize spawn and despawn positions
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(nextSpawnPosition, -1, 0), new Vector3(nextSpawnPosition, 1, 0));

        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(despawnPosition, -1, 0), new Vector3(despawnPosition, 1, 0));
    }
}