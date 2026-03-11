using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AsteroidManager : MonoBehaviour
{
    [Header("Static Asteroid Settings")]
    [SerializeField] private GameObject[] asteroidPrefabs;
    [SerializeField] private float spawnDistanceFromPlayer = 20f;
    [SerializeField] private float minSpawnInterval = 30f;
    [SerializeField] private float maxSpawnInterval = 60f;
    [SerializeField] private int maxStaticAsteroids = 5;
    [SerializeField] private Transform asteroidParent;
    
    [Header("Moving Asteroid Settings")]
    [SerializeField] private float movingSpawnMinDistance = 10f;
    [SerializeField] private float movingSpawnMaxDistance = 15f;
    [SerializeField] private float movingSpawnChance = 0.02f; // Per frame chance
    [SerializeField] private int maxMovingAsteroids = 3;
    
    private Player player;
    private GameManager gameManager;
    private List<AsteroidHazard> activeStaticAsteroids = new List<AsteroidHazard>();
    private List<AsteroidHazard> activeMovingAsteroids = new List<AsteroidHazard>();
    private Coroutine staticSpawnCoroutine;
    
    private void Start()
    {
        gameManager = GameManager.GetInstance();
        player = gameManager?.Getplayer();
        
        if (asteroidParent == null)
        {
            GameObject parent = new GameObject("Asteroids");
            asteroidParent = parent.transform;
        }
        
        StartStaticAsteroidSpawning();
    }
    
    private void Update()
    {
        if (gameManager != null && gameManager.IsPlaying())
        {
            // Clean up destroyed asteroids
            CleanupAsteroidLists();
            
            // Try to spawn moving asteroids randomly
            if (Random.Range(0f, 1f) < movingSpawnChance && activeMovingAsteroids.Count < maxMovingAsteroids)
            {
                SpawnMovingAsteroid();
            }
        }
    }
    
    private void StartStaticAsteroidSpawning()
    {
        if (staticSpawnCoroutine != null)
            StopCoroutine(staticSpawnCoroutine);
        
        staticSpawnCoroutine = StartCoroutine(StaticAsteroidSpawnLoop());
    }
    
    private IEnumerator StaticAsteroidSpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minSpawnInterval, maxSpawnInterval));
            
            if (gameManager != null && gameManager.IsPlaying() && activeStaticAsteroids.Count < maxStaticAsteroids)
            {
                SpawnStaticAsteroid();
            }
        }
    }
    
    private void SpawnStaticAsteroid()
    {
        if (player == null || asteroidPrefabs == null || asteroidPrefabs.Length == 0)
            return;
        
        // Get random position around player at specified distance
        Vector2 playerPos = player.transform.position;
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        Vector2 spawnPos = playerPos + (randomDirection * spawnDistanceFromPlayer);
        
        // Select random asteroid prefab
        GameObject asteroidPrefab = asteroidPrefabs[Random.Range(0, asteroidPrefabs.Length)];
        
        AsteroidHazard asteroid = null;
        
        // Try to get from pool first
        if (PoolManager.Instance != null)
        {
            asteroid = PoolManager.Instance.GetAsteroid(asteroidPrefab);
            if (asteroid != null)
            {
                asteroid.transform.position = spawnPos;
                asteroid.transform.rotation = Random.rotation;
            }
        }
        
        // Fallback to instantiation if pool not available or no asteroid available
        if (asteroid == null)
        {
            GameObject asteroidObj = Instantiate(asteroidPrefab, spawnPos, Random.rotation, asteroidParent);
            asteroid = asteroidObj.GetComponent<AsteroidHazard>();
            if (asteroid == null)
            {
                asteroid = asteroidObj.AddComponent<AsteroidHazard>();
            }
        }
        
        // Set as static asteroid
        asteroid.SetAsStatic(spawnPos);
        activeStaticAsteroids.Add(asteroid);
        
        Debug.Log($"Spawned static asteroid at {spawnPos}");
    }
    
    public void SpawnMovingAsteroid()
    {
        if (player == null || asteroidPrefabs == null || asteroidPrefabs.Length == 0)
            return;
        
        // Get random position around player at specified distance range
        Vector2 playerPos = player.transform.position;
        float spawnDistance = Random.Range(movingSpawnMinDistance, movingSpawnMaxDistance);
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        Vector2 spawnPos = playerPos + (randomDirection * spawnDistance);
        
        // Select random asteroid prefab
        GameObject asteroidPrefab = asteroidPrefabs[Random.Range(0, asteroidPrefabs.Length)];
        
        AsteroidHazard asteroid = null;
        
        // Try to get from pool first
        if (PoolManager.Instance != null)
        {
            asteroid = PoolManager.Instance.GetAsteroid(asteroidPrefab);
            if (asteroid != null)
            {
                asteroid.transform.position = spawnPos;
                asteroid.transform.rotation = Random.rotation;
            }
        }
        
        // Fallback to instantiation if pool not available or no asteroid available
        if (asteroid == null)
        {
            GameObject asteroidObj = Instantiate(asteroidPrefab, spawnPos, Random.rotation, asteroidParent);
            asteroid = asteroidObj.GetComponent<AsteroidHazard>();
            if (asteroid == null)
            {
                asteroid = asteroidObj.AddComponent<AsteroidHazard>();
            }
        }
        
        // Set as moving asteroid
        asteroid.SetAsMoving(spawnPos, player.transform);
        activeMovingAsteroids.Add(asteroid);
        
        Debug.Log($"Spawned moving asteroid at {spawnPos}");
    }
    
    private void CleanupAsteroidLists()
    {
        // Remove null/destroyed asteroids from lists
        activeStaticAsteroids.RemoveAll(asteroid => asteroid == null);
        activeMovingAsteroids.RemoveAll(asteroid => asteroid == null);
    }
    
    public void ClearAllAsteroids()
    {
        // Clear all active asteroids (useful for game over/restart)
        foreach (AsteroidHazard asteroid in activeStaticAsteroids)
        {
            if (asteroid != null)
                Destroy(asteroid.gameObject);
        }
        
        foreach (AsteroidHazard asteroid in activeMovingAsteroids)
        {
            if (asteroid != null)
                Destroy(asteroid.gameObject);
        }
        
        activeStaticAsteroids.Clear();
        activeMovingAsteroids.Clear();
    }
    
    private void OnDestroy()
    {
        if (staticSpawnCoroutine != null)
            StopCoroutine(staticSpawnCoroutine);
    }
    
    // Public methods for external control
    public void SetStaticSpawnRate(float minInterval, float maxInterval)
    {
        minSpawnInterval = minInterval;
        maxSpawnInterval = maxInterval;
    }
    
    public void SetMovingSpawnChance(float chance)
    {
        movingSpawnChance = Mathf.Clamp01(chance);
    }
    
    public int GetActiveStaticCount()
    {
        CleanupAsteroidLists();
        return activeStaticAsteroids.Count;
    }
    
    public int GetActiveMovingCount()
    {
        CleanupAsteroidLists();
        return activeMovingAsteroids.Count;
    }
}