using UnityEngine;
using System.Collections.Generic;

public class BackgroundSpawner : MonoBehaviour
{
    [Header("Background Settings")]
    [SerializeField] private GameObject backgroundPrefab; 
    [SerializeField] private float backgroundWidth = 20f; 
    [SerializeField] private int poolSize = 5;
    [SerializeField] private float spawnAheadDistance = 30f; // Spawn when player is this close

    [Header("References")]
    [SerializeField] private Transform player; // Player transform

    private Queue<GameObject> backgroundPool;
    private List<GameObject> activeBackgrounds;
    private float nextSpawnX;
    private float despawnBehindDistance = 20f;

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        // Auto-calculate background width from prefab
        if (backgroundPrefab != null)
        {
            SpriteRenderer sr = backgroundPrefab.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                backgroundWidth = sr.sprite.bounds.size.x;
            }
        }

        InitializePool();

        // Spawn initial backgrounds
        nextSpawnX = 0f;
        for (int i = 0; i < Mathf.Min(3, poolSize); i++)
        {
            SpawnBackground(nextSpawnX);
            nextSpawnX += backgroundWidth;
        }
    }

    void InitializePool()
    {
        backgroundPool = new Queue<GameObject>();
        activeBackgrounds = new List<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject bg = Instantiate(backgroundPrefab, transform);
            bg.name = "Background_Pooled_" + i;
            bg.SetActive(false);
            backgroundPool.Enqueue(bg);
        }
    }

    void Update()
    {
        if (player == null) return;

        if (player.position.x + spawnAheadDistance >= nextSpawnX)
        {
            SpawnBackground(nextSpawnX);
            nextSpawnX += backgroundWidth;
        }

        RecycleOldBackgrounds();
    }

    void SpawnBackground(float xPosition)
    {
        GameObject bg = GetPooledBackground();
        if (bg == null) return;

        bg.transform.position = new Vector3(xPosition, backgroundPrefab.transform.position.y, backgroundPrefab.transform.position.z);
        bg.SetActive(true);
        activeBackgrounds.Add(bg);
    }

    GameObject GetPooledBackground()
    {
        if (backgroundPool.Count > 0)
        {
            return backgroundPool.Dequeue();
        }

        Debug.LogWarning("Background pool exhausted! Consider increasing pool size.");
        GameObject newBg = Instantiate(backgroundPrefab, transform);
        newBg.name = "Background_Pooled_Extra";
        return newBg;
    }

    void RecycleOldBackgrounds()
    {
        for (int i = activeBackgrounds.Count - 1; i >= 0; i--)
        {
            GameObject bg = activeBackgrounds[i];

            if (bg.transform.position.x < player.position.x - despawnBehindDistance)
            {
                // Return to pool
                bg.SetActive(false);
                backgroundPool.Enqueue(bg);
                activeBackgrounds.RemoveAt(i);
            }
        }
    }


    public void ResetSpawner()
    {
        for (int i = activeBackgrounds.Count - 1; i >= 0; i--)
        {
            GameObject bg = activeBackgrounds[i];
            bg.SetActive(false);
            backgroundPool.Enqueue(bg);
        }
        activeBackgrounds.Clear();

        nextSpawnX = 0f;

        for (int i = 0; i < Mathf.Min(3, poolSize); i++)
        {
            SpawnBackground(nextSpawnX);
            nextSpawnX += backgroundWidth;
        }
    }
}