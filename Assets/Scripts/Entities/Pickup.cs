using UnityEngine;

public class Pickup : MonoBehaviour, IPoolable
{
    public virtual void OnPicked()
    {
        ReturnToPool();
    }

    #region IPoolable Implementation
    public virtual void OnPoolSpawn()
    {
        // Reset pickup state when taken from pool
        // Individual pickup types can override this for specific reset logic
    }

    public virtual void OnPoolDespawn()
    {
        // Clean up when returned to pool
        // Individual pickup types can override this for specific cleanup 
    }

    protected void ReturnToPool()
    {
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.ReturnPickup(this);
        }
        else
        {
            Debug.LogError("PoolManager.Instance is null! Pickup pooling is not set up correctly. Pickup will be destroyed as fallback.");
            // Fallback if pool manager doesn't exist
            Destroy(gameObject);
        }
    }
    #endregion
}
