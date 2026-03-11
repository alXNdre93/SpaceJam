/// <summary>
/// Interface for objects that can be pooled and reused
/// </summary>
public interface IPoolable
{
    /// <summary>
    /// Called when the object is taken from the pool and activated
    /// Use this to reset/initialize the object state
    /// </summary>
    void OnPoolSpawn();

    /// <summary>
    /// Called when the object is returned to the pool
    /// Use this to clean up and prepare for reuse
    /// </summary>
    void OnPoolDespawn();
}