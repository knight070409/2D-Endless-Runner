using UnityEngine;
using System.Collections.Generic;

public class PowerUpSpawner : MonoBehaviour
{
    [Header("Power-up Prefabs")]
    [SerializeField] private GameObject shieldPrefab;
    [SerializeField] private GameObject speedBoostPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Spawn Settings")]
    [Range(0f, 100f)]
    [SerializeField] private float levelSpawnChance = 60f; // Chance this level spawns ANY power-ups

    [Range(0f, 100f)]
    [SerializeField] private float perPointSpawnChance = 40f; // Chance each spawn point spawns a power-up

    [Range(0f, 100f)]
    [SerializeField] private float shieldChance = 50f; // 50% shield, 50% speed boost

    [Header("Object Pooling")]
    [SerializeField] private int poolSize = 3; // Pool size per power-up type

    private static Queue<GameObject> shieldPool;
    private static Queue<GameObject> speedBoostPool;
    private static bool poolsInitialized = false;

    private List<GameObject> spawnedPowerUps;
    private bool hasSpawned = false;

    void Awake()
    {
        // Initialize pools once (static, shared across all spawners)
        if (!poolsInitialized)
        {
            InitializePools();
            poolsInitialized = true;
        }

        spawnedPowerUps = new List<GameObject>();
    }

    void Start()
    {
        SpawnPowerUps();
    }

    void InitializePools()
    {
        shieldPool = new Queue<GameObject>();
        speedBoostPool = new Queue<GameObject>();

        // Create pool for shield
        if (shieldPrefab != null)
        {
            for (int i = 0; i < poolSize; i++)
            {
                GameObject powerUp = Instantiate(shieldPrefab);
                powerUp.SetActive(false);
                shieldPool.Enqueue(powerUp);
            }
        }

        // Create pool for speed boost
        if (speedBoostPrefab != null)
        {
            for (int i = 0; i < poolSize; i++)
            {
                GameObject powerUp = Instantiate(speedBoostPrefab);
                powerUp.SetActive(false);
                speedBoostPool.Enqueue(powerUp);
            }
        }
    }

    void SpawnPowerUps()
    {
        if (hasSpawned) return;

        // Check if this level should spawn any power-ups at all
        float levelRoll = Random.Range(0f, 100f);
        if (levelRoll > levelSpawnChance)
        {
            hasSpawned = true;
            return;
        }

        // Check spawn points array
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points assigned to PowerUpSpawner!");
            hasSpawned = true;
            return;
        }

        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint == null) continue;

            // Roll for this specific spawn point
            float pointRoll = Random.Range(0f, 100f);
            if (pointRoll <= perPointSpawnChance)
            {
                // Choose random power-up type
                bool isShield = Random.Range(0f, 100f) < shieldChance;
                GameObject powerUp = GetFromPool(isShield);

                if (powerUp != null)
                {
                    powerUp.transform.position = spawnPoint.position;
                    powerUp.transform.parent = transform;
                    powerUp.SetActive(true);
                    spawnedPowerUps.Add(powerUp);

                    // Re-enable collider
                    Collider2D collider = powerUp.GetComponent<Collider2D>();
                    if (collider != null)
                        collider.enabled = true;
                }
            }
        }

        hasSpawned = true;
    }

    GameObject GetFromPool(bool isShield)
    {
        Queue<GameObject> pool = isShield ? shieldPool : speedBoostPool;
        GameObject prefab = isShield ? shieldPrefab : speedBoostPrefab;

        if (pool.Count > 0)
        {
            return pool.Dequeue();
        }
        else
        {
            return Instantiate(prefab);
        }
    }

    public void ReturnToPool(GameObject powerUp)
    {
        if (powerUp == null) return;

        // Re-enable collider for next use
        Collider2D collider = powerUp.GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = true;

        powerUp.SetActive(false);
        powerUp.transform.parent = null;

        // Determine which pool to return to based on tag
        if (powerUp.CompareTag("Shield"))
        {
            shieldPool.Enqueue(powerUp);
        }
        else if (powerUp.CompareTag("SpeedBoost"))
        {
            speedBoostPool.Enqueue(powerUp);
        }

        // Remove from spawned list
        if (spawnedPowerUps.Contains(powerUp))
            spawnedPowerUps.Remove(powerUp);
    }

    public void DespawnAllPowerUps()
    {
        // Return all spawned power-ups to pool
        for (int i = spawnedPowerUps.Count - 1; i >= 0; i--)
        {
            if (spawnedPowerUps[i] != null)
            {
                ReturnToPool(spawnedPowerUps[i]);
            }
        }
        spawnedPowerUps.Clear();
        hasSpawned = false;
    }


    // Visualize spawn points in editor
    private void OnDrawGizmos()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + Vector3.up * 2f);
            }
        }
    }
}