using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central manager for all object pools in the game
/// </summary>
public class PoolManager : MonoBehaviour
{
    [Header("Bullet Pooling")]
    [SerializeField] private BulletPoolConfig[] bulletConfigs;
    [SerializeField] private int defaultBulletPoolSize = 50;
    
    [Header("Enemy Pooling")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private int enemyPoolSize = 20;
    
    [Header("Pickup Pooling")]
    [SerializeField] private Pickup[] pickupPrefabs;
    [SerializeField] private int pickupPoolSize = 15;
    
    [Header("Asteroid Pooling")]
    [SerializeField] private GameObject[] asteroidPrefabs;
    [SerializeField] private int asteroidPoolSize = 10;

    [Header("Pool Settings")]
    [SerializeField] private Transform bulletParent;
    [SerializeField] private Transform enemyParent;
    [SerializeField] private Transform pickupParent;
    [SerializeField] private Transform asteroidParent;

    // Pool instances
    private Dictionary<string, ObjectPool<Bullet>> _bulletPools = new Dictionary<string, ObjectPool<Bullet>>();
    private Dictionary<string, ObjectPool<Enemy>> _enemyPools = new Dictionary<string, ObjectPool<Enemy>>();
    private Dictionary<string, ObjectPool<Pickup>> _pickupPools = new Dictionary<string, ObjectPool<Pickup>>();
    private Dictionary<string, ObjectPool<AsteroidHazard>> _asteroidPools = new Dictionary<string, ObjectPool<AsteroidHazard>>();

    // Singleton
    public static PoolManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern with better error handling
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("PoolManager: Multiple instances detected, destroying duplicate");
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("PoolManager: Initializing pools...");
        InitializePools();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Debug.Log("PoolManager: Cleaning up pools on destroy");
            ClearAllPools();
            Instance = null;
        }
    }

    /// <summary>
    /// Initialize all object pools
    /// </summary>
    private void InitializePools()
    {
        try
        {
            // Create parent objects if not assigned
            if (bulletParent == null)
            {
                bulletParent = new GameObject("Bullet Pool").transform;
                bulletParent.SetParent(transform);
            }

            if (enemyParent == null)
            {
                enemyParent = new GameObject("Enemy Pool").transform;
                enemyParent.SetParent(transform);
            }

            if (pickupParent == null)
            {
                pickupParent = new GameObject("Pickup Pool").transform;
                pickupParent.SetParent(transform);
            }

            if (asteroidParent == null)
            {
                asteroidParent = new GameObject("Asteroid Pool").transform;
                asteroidParent.SetParent(transform);
            }

            // Initialize bullet pools
            if (bulletConfigs != null)
            {
                foreach (BulletPoolConfig config in bulletConfigs)
                {
                    if (config.bulletPrefab != null)
                    {
                        string poolKey = config.bulletPrefab.name;
                        int poolSize = config.poolSize > 0 ? config.poolSize : defaultBulletPoolSize;
                        _bulletPools[poolKey] = new ObjectPool<Bullet>(config.bulletPrefab, bulletParent, poolSize);
                        Debug.Log($"PoolManager: Created bullet pool for '{poolKey}' with size {poolSize}");
                    }
                    else
                    {
                        Debug.LogWarning("PoolManager: Bullet config has null prefab reference!");
                    }
                }
            }

            // Initialize enemy pools
            if (enemyPrefabs != null)
            {
                foreach (GameObject enemyPrefab in enemyPrefabs)
                {
                    if (enemyPrefab != null)
                    {
                        Enemy enemyComponent = enemyPrefab.GetComponent<Enemy>();
                        if (enemyComponent != null)
                        {
                            string poolKey = enemyPrefab.name;
                            _enemyPools[poolKey] = new ObjectPool<Enemy>(enemyComponent, enemyParent, enemyPoolSize);
                            Debug.Log($"PoolManager: Created enemy pool for '{poolKey}' with size {enemyPoolSize}");
                        }
                        else
                        {
                            Debug.LogWarning($"PoolManager: Enemy prefab '{enemyPrefab.name}' missing Enemy component!");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("PoolManager: Enemy prefabs array has null reference!");
                    }
                }
            }

            // Initialize pickup pools
            if (pickupPrefabs != null)
            {
                foreach (Pickup pickupPrefab in pickupPrefabs)
                {
                    if (pickupPrefab != null)
                    {
                        string poolKey = pickupPrefab.GetType().Name;
                        _pickupPools[poolKey] = new ObjectPool<Pickup>(pickupPrefab, pickupParent, pickupPoolSize);
                        Debug.Log($"PoolManager: Created pickup pool for '{poolKey}' with size {pickupPoolSize}");
                    }
                    else
                    {
                        Debug.LogWarning("PoolManager: Pickup prefabs array has null reference!");
                    }
                }
            }

            // Initialize asteroid pools
            if (asteroidPrefabs != null)
            {
                foreach (GameObject asteroidPrefab in asteroidPrefabs)
                {
                    if (asteroidPrefab != null)
                    {
                        AsteroidHazard asteroidComponent = asteroidPrefab.GetComponent<AsteroidHazard>();
                        if (asteroidComponent == null)
                        {
                            // Add AsteroidHazard component if it doesn't exist
                            asteroidComponent = asteroidPrefab.AddComponent<AsteroidHazard>();
                        }
                        
                        string poolKey = asteroidPrefab.name;
                        _asteroidPools[poolKey] = new ObjectPool<AsteroidHazard>(asteroidComponent, asteroidParent, asteroidPoolSize);
                        Debug.Log($"PoolManager: Created asteroid pool for '{poolKey}' with size {asteroidPoolSize}");
                    }
                    else
                    {
                        Debug.LogWarning("PoolManager: Asteroid prefabs array has null reference!");
                    }
                }
            }

            Debug.Log("PoolManager: All pools initialized successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"PoolManager: Error during pool initialization: {ex.Message}");
        }
    }

    #region Bullet Pool Methods
    /// <summary>
    /// Get a bullet from the pool by prefab name
    /// </summary>
    public Bullet GetBullet(string bulletName)
    {
        if (_bulletPools.TryGetValue(bulletName, out ObjectPool<Bullet> pool))
        {
            return pool.Get();
        }

        Debug.LogWarning($"Bullet pool for '{bulletName}' not found!");
        return null;
    }

    /// <summary>
    /// Get a bullet from the pool by Bullet prefab reference
    /// </summary>
    public Bullet GetBullet(Bullet bulletPrefab)
    {
        return GetBullet(bulletPrefab.name);
    }

    /// <summary>
    /// Return a bullet to the pool
    /// </summary>
    public void ReturnBullet(Bullet bullet)
    {
        if (bullet == null) return;

        // Try multiple name variations to find the correct pool
        string originalName = bullet.name.Replace("(Clone)", "").Trim();
        string[] namesToTry = { originalName, bullet.name.Trim() };
        
        ObjectPool<Bullet> pool = null;
        string matchedKey = null;
        
        foreach (string nameToTry in namesToTry)
        {
            if (_bulletPools.TryGetValue(nameToTry, out pool))
            {
                matchedKey = nameToTry;
                break;
            }
        }
        
        // Also try finding by checking all pools for matching prefab name
        if (pool == null)
        {
            foreach (var kvp in _bulletPools)
            {
                // This is a more lenient match - compare against the pool key
                if (originalName.Contains(kvp.Key) || kvp.Key.Contains(originalName))
                {
                    pool = kvp.Value;
                    matchedKey = kvp.Key;
                    Debug.Log($"Found bullet pool using partial match: '{originalName}' matched with '{kvp.Key}'");
                    break;
                }
            }
        }
        
        if (pool != null)
        {
            pool.Return(bullet);
            Debug.Log($"Bullet '{originalName}' successfully returned to pool '{matchedKey}'");
        }
        else
        {
            Debug.LogError($"No pool found for bullet '{originalName}'. Available pools: {string.Join(", ", _bulletPools.Keys)}. This bullet will be destroyed but shouldn't be!");
            // Last resort - still destroy, but this indicates a configuration issue
            Destroy(bullet.gameObject);
        }
    }
    #endregion

    #region Enemy Pool Methods
    /// <summary>
    /// Get an enemy from the pool by prefab name
    /// </summary>
    public Enemy GetEnemy(string enemyName)
    {
        if (_enemyPools.TryGetValue(enemyName, out ObjectPool<Enemy> pool))
        {
            return pool.Get();
        }

        Debug.LogWarning($"Enemy pool for '{enemyName}' not found!");
        return null;
    }

    /// <summary>
    /// Get an enemy from the pool by GameObject reference
    /// </summary>
    public Enemy GetEnemy(GameObject enemyPrefab)
    {
        return GetEnemy(enemyPrefab.name);
    }

    /// <summary>
    /// Return an enemy to the pool
    /// </summary>
    public void ReturnEnemy(Enemy enemy)
    {
        if (enemy == null) return;

        // Try multiple name variations to find the correct pool
        string originalName = enemy.name.Replace("(Clone)", "").Trim();
        string[] namesToTry = { originalName, enemy.name.Trim() };
        
        ObjectPool<Enemy> pool = null;
        string matchedKey = null;
        
        foreach (string nameToTry in namesToTry)
        {
            if (_enemyPools.TryGetValue(nameToTry, out pool))
            {
                matchedKey = nameToTry;
                break;
            }
        }
        
        // Also try finding by checking all pools for matching prefab name
        if (pool == null)
        {
            foreach (var kvp in _enemyPools)
            {
                if (originalName.Contains(kvp.Key) || kvp.Key.Contains(originalName))
                {
                    pool = kvp.Value;
                    matchedKey = kvp.Key;
                    Debug.Log($"Found enemy pool using partial match: '{originalName}' matched with '{kvp.Key}'");
                    break;
                }
            }
        }
        
        if (pool != null)
        {
            pool.Return(enemy);
            Debug.Log($"Enemy '{originalName}' successfully returned to pool '{matchedKey}'");
        }
        else
        {
            Debug.LogError($"No pool found for enemy '{originalName}'. Available pools: {string.Join(", ", _enemyPools.Keys)}. This enemy will be destroyed but shouldn't be!");
            // Last resort - still destroy, but this indicates a configuration issue
            Destroy(enemy.gameObject);
        }
    }
    #endregion

    #region Pickup Pool Methods
    /// <summary>
    /// Get a pickup from the pool by type name
    /// </summary>
    public Pickup GetPickup(string pickupTypeName)
    {
        if (_pickupPools.TryGetValue(pickupTypeName, out ObjectPool<Pickup> pool))
        {
            return pool.Get();
        }

        Debug.LogWarning($"Pickup pool for '{pickupTypeName}' not found!");
        return null;
    }

    /// <summary>
    /// Get a pickup from the pool by Pickup reference
    /// </summary>
    public Pickup GetPickup(Pickup pickupPrefab)
    {
        return GetPickup(pickupPrefab.GetType().Name);
    }

    /// <summary>
    /// Return a pickup to the pool
    /// </summary>
    public void ReturnPickup(Pickup pickup)
    {
        if (pickup == null) return;

        // Try both the type name and GameObject name for matching
        string typeName = pickup.GetType().Name;
        string gameObjectName = pickup.name.Replace("(Clone)", "").Trim();
        string[] namesToTry = { typeName, gameObjectName, pickup.name.Trim() };
        
        ObjectPool<Pickup> pool = null;
        string matchedKey = null;
        
        foreach (string nameToTry in namesToTry)
        {
            if (_pickupPools.TryGetValue(nameToTry, out pool))
            {
                matchedKey = nameToTry;
                break;
            }
        }
        
        // Also try finding by checking all pools for matching names
        if (pool == null)
        {
            foreach (var kvp in _pickupPools)
            {
                if (gameObjectName.Contains(kvp.Key) || kvp.Key.Contains(gameObjectName) ||
                    typeName.Contains(kvp.Key) || kvp.Key.Contains(typeName))
                {
                    pool = kvp.Value;
                    matchedKey = kvp.Key;
                    Debug.Log($"Found pickup pool using partial match: '{typeName}/{gameObjectName}' matched with '{kvp.Key}'");
                    break;
                }
            }
        }
        
        if (pool != null)
        {
            pool.Return(pickup);
            Debug.Log($"Pickup '{typeName}' ('{gameObjectName}') successfully returned to pool '{matchedKey}'");
        }
        else
        {
            Debug.LogError($"No pool found for pickup '{typeName}' (GameObject: '{gameObjectName}'). Available pools: {string.Join(", ", _pickupPools.Keys)}. This pickup will be destroyed but shouldn't be!");
            // Last resort - still destroy, but this indicates a configuration issue
            Destroy(pickup.gameObject);
        }
    }
    #endregion

    #region Asteroid Pool Methods
    /// <summary>
    /// Get an asteroid from the pool by prefab name
    /// </summary>
    public AsteroidHazard GetAsteroid(string asteroidName)
    {
        if (_asteroidPools.TryGetValue(asteroidName, out ObjectPool<AsteroidHazard> pool))
        {
            return pool.Get();
        }

        Debug.LogWarning($"Asteroid pool for '{asteroidName}' not found!");
        return null;
    }

    /// <summary>
    /// Get an asteroid from the pool by GameObject reference
    /// </summary>
    public AsteroidHazard GetAsteroid(GameObject asteroidPrefab)
    {
        return GetAsteroid(asteroidPrefab.name);
    }

    /// <summary>
    /// Return an asteroid to the pool
    /// </summary>
    public void ReturnAsteroid(AsteroidHazard asteroid)
    {
        if (asteroid == null) return;

        string gameObjectName = asteroid.name.Replace("(Clone)", "").Trim();
        
        ObjectPool<AsteroidHazard> pool = null;
        string matchedKey = null;
        
        // Try direct match first
        if (_asteroidPools.TryGetValue(gameObjectName, out pool))
        {
            matchedKey = gameObjectName;
        }
        
        // Also try finding by checking all pools for matching names
        if (pool == null)
        {
            foreach (var kvp in _asteroidPools)
            {
                if (gameObjectName.Contains(kvp.Key) || kvp.Key.Contains(gameObjectName))
                {
                    pool = kvp.Value;
                    matchedKey = kvp.Key;
                    Debug.Log($"Found asteroid pool using partial match: '{gameObjectName}' matched with '{kvp.Key}'");
                    break;
                }
            }
        }
        
        if (pool != null)
        {
            pool.Return(asteroid);
            Debug.Log($"Asteroid '{gameObjectName}' successfully returned to pool '{matchedKey}'");
        }
        else
        {
            Debug.LogError($"No pool found for asteroid '{gameObjectName}'. Available pools: {string.Join(", ", _asteroidPools.Keys)}. This asteroid will be destroyed but shouldn't be!");
            // Last resort - still destroy, but this indicates a configuration issue
            Destroy(asteroid.gameObject);
        }
    }
    #endregion

    #region Utility Methods
    /// <summary>
    /// Get pool statistics for debugging
    /// </summary>
    public string GetPoolStats()
    {
        string stats = "=== Pool Statistics ===\n";
        
        foreach (var bulletPool in _bulletPools)
            stats += $"Bullet '{bulletPool.Key}': {bulletPool.Value.PoolCount} in pool\n";
        
        foreach (var enemyPool in _enemyPools)
            stats += $"Enemy '{enemyPool.Key}': {enemyPool.Value.PoolCount} in pool\n";
        
        foreach (var pickupPool in _pickupPools)
            stats += $"Pickup '{pickupPool.Key}': {pickupPool.Value.PoolCount} in pool\n";
        
        foreach (var asteroidPool in _asteroidPools)
            stats += $"Asteroid '{asteroidPool.Key}': {asteroidPool.Value.PoolCount} in pool\n";
        
        return stats;
    }

    /// <summary>
    /// Clear all pools (useful for scene changes)
    /// </summary>
    public void ClearAllPools()
    {
        try
        {
            if (_bulletPools != null)
            {
                foreach (var pool in _bulletPools.Values)
                    pool?.Clear();
                _bulletPools.Clear();
            }
            
            if (_enemyPools != null)
            {
                foreach (var pool in _enemyPools.Values)
                    pool?.Clear();
                _enemyPools.Clear();
            }
            
            if (_pickupPools != null)
            {
                foreach (var pool in _pickupPools.Values)
                    pool?.Clear();
                _pickupPools.Clear();
            }
            
            if (_asteroidPools != null)
            {
                foreach (var pool in _asteroidPools.Values)
                    pool?.Clear();
                _asteroidPools.Clear();
            }
            
            Debug.Log("PoolManager: All pools cleared successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"PoolManager: Error during pool cleanup: {ex.Message}");
        }
    }
    #endregion

    #if UNITY_EDITOR
    [ContextMenu("Print Pool Stats")]
    private void PrintPoolStats()
    {
        Debug.Log(GetPoolStats());
    }
    #endif
}

[System.Serializable]
public struct BulletPoolConfig
{
    public Bullet bulletPrefab;
    public int poolSize;
}