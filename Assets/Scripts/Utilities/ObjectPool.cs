using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic object pool for reusing GameObjects to avoid frequent instantiation and destruction
/// </summary>
/// <typeparam name="T">Type that implements IPoolable</typeparam>
public class ObjectPool<T> where T : MonoBehaviour, IPoolable
{
    private readonly Stack<T> _pool = new Stack<T>();
    private readonly T _prefab;
    private readonly Transform _parent;
    private readonly int _maxSize;

    /// <summary>
    /// Creates a new object pool
    /// </summary>
    /// <param name="prefab">The prefab to pool</param>
    /// <param name="parent">Parent transform for pooled objects (optional)</param>
    /// <param name="initialSize">Initial number of objects to create</param>
    /// <param name="maxSize">Maximum pool size (0 = unlimited)</param>
    public ObjectPool(T prefab, Transform parent = null, int initialSize = 10, int maxSize = 0)
    {
        _prefab = prefab;
        _parent = parent;
        _maxSize = maxSize;

        // Pre-populate the pool
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewObject();
        }
    }

    /// <summary>
    /// Get an object from the pool
    /// </summary>
    /// <returns>Ready-to-use pooled object</returns>
    public T Get()
    {
        T item = null;

        // Keep trying to get a valid object from the pool
        while (_pool.Count > 0 && item == null)
        {
            T candidate = _pool.Pop();
            
            // Check if the pooled object is still valid (not destroyed)
            if (candidate != null && candidate.gameObject != null)
            {
                item = candidate;
            }
            // If the object is destroyed, skip it and try the next one
        }

        // If no valid object found in pool, create a new one
        if (item == null)
        {
            item = CreateNewObject();
        }

        item.gameObject.SetActive(true);
        item.OnPoolSpawn();
        return item;
    }

    /// <summary>
    /// Return an object to the pool
    /// </summary>
    /// <param name="item">Object to return</param>
    public void Return(T item)
    {
        if (item == null) return;

        item.OnPoolDespawn();
        item.gameObject.SetActive(false);

        // Always return to pool - no arbitrary size limits
        // If pool gets too large, it's better to keep objects than destroy/recreate them
        _pool.Push(item);
        
        // Optional: Log if pool is getting very large (might indicate a leak)
        if (_maxSize > 0 && _pool.Count > _maxSize * 2)
        {
            Debug.LogWarning($"Pool for {typeof(T).Name} is getting large: {_pool.Count} objects (max was {_maxSize}). This might indicate a pooling issue.");
        }
    }

    /// <summary>
    /// Get the current number of objects in the pool
    /// </summary>
    public int PoolCount => _pool.Count;

    /// <summary>
    /// Clear all objects from the pool
    /// </summary>
    public void Clear()
    {
        while (_pool.Count > 0)
        {
            T item = _pool.Pop();
            if (item != null)
                Object.Destroy(item.gameObject);
        }
    }

    private T CreateNewObject()
    {
        T newItem = Object.Instantiate(_prefab, _parent);
        newItem.gameObject.SetActive(false);
        return newItem;
    }
}