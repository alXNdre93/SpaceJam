using NUnit.Framework;
using UnityEngine;
using System;

public class Enemy : PlayableObject, IPoolable
{
    private string enemyName;
    [SerializeField] protected float speed;
    [SerializeField] protected Transform target;
    [SerializeField] protected bool isBoss;
    [SerializeField] protected float attackRange = 0;
    [SerializeField] protected float attackTime = 1f;
    private EnemyType enemyType;
    protected GameManager gameManager;
    public Action OnDeath, OnBossDeath;
    public Action<float> OnBossHealthUpdate;
    public Action<float> OnBossMaxHealthSet;

    protected virtual void Start()
    {
        // Don't find target in Start() for pooled objects - will be handled in OnPoolSpawn() and Update()
        health = new Health(1, 0.1f, 1);
        gameManager = GameManager.GetInstance();
        
        // If this is a boss, initialize the health bar with max and current health
        if (isBoss)
        {
            if(!gameManager.uIManager.isBossSliderActive())
            {
                gameManager.uIManager.ToggleBossHealth();
            }

            float maxHealth = health.GetMaxHealth();
            gameManager.uIManager.SetBossMaxHealth(maxHealth);
            OnBossHealthUpdate(health.GetHealth());

            Debug.Log($"Enemy.Start(): Boss {gameObject.name} spawned - max health: {maxHealth}");
        }
    }
    protected virtual void Update()
    {
        // Don't update if game is not playing (prevents null reference after player death)
        if (gameManager != null && !gameManager.IsPlaying())
            return;
            
        // Keep trying to find player if target is null (handles pooled enemies spawning before player exists)
        if (target == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                target = playerObj.transform;
        }

        if(target != null && target.gameObject != null){
            Move(target.position);
        }
        else{
            Move(speed);
        }
    }

    public void SetEnemyType(EnemyType _enemyType)
    {
        enemyType = _enemyType;
    }

    public override void Attack(float interval)
    {
        throw new System.NotImplementedException();
    }

    public override void GetDamage(float damage)
    {
        health.DeductHealth(damage);
        if(isBoss)
            OnBossHealthUpdate(health.GetHealth());
        if (health.GetHealth() <= 0)
        {
            Die();
        }
    }
    
    public bool IsBoss(){
        return isBoss;
    }

    public override void Move(Vector2 direction)
    {
        direction.x -= transform.position.x;
        direction.y -= transform.position.y;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    public override void Move(float speed){
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    public override void Move(Vector2 direction, Vector2 target){}

    public override void Die()
    {
        Debug.Log($"Enemy.Die(): {gameObject.name} isBoss={isBoss}");
        GameManager.GetInstance().NotifyDeath(this);
        ReturnToPool();
    }

    public override void Shoot()
    {
        throw new System.NotImplementedException();
    }

    public override int GetPointsValue()
    {
        return base.GetPointsValue();
    }

    public void SetEnemy(float _attackRange, float _attackTime, bool _isBoss = false){
        attackRange = _attackRange;
        attackTime = _attackTime;
        isBoss = _isBoss;
    }

    #region IPoolable Implementation
    public void OnPoolSpawn()
    {
        // Reset enemy state when taken from pool
        // Try to find player but don't worry if it doesn't exist yet - Update() will retry
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            target = playerObj.transform;
        else
            target = null; // Will be found in Update() when player exists
            
        gameManager = GameManager.GetInstance();
        
        // Reset health to full
        health.ResetToMax();
        canShoot = false;
        
        // Reset scale to default (in case this was previously a boss with 5x scale)
        if (transform.localScale.x > 2f) // If scale is larger than normal, reset it
        {
            transform.localScale = Vector3.one;
            Debug.Log($"Enemy.OnPoolSpawn(): Reset scale for {gameObject.name}");
        }

        // Re-enable the first child (contains the actual ship sprite)
        if (transform.childCount > 0)
        {
            transform.GetChild(0).gameObject.SetActive(true);
        }

        // Ensure main collider is re-enabled
        Collider2D mainCollider = GetComponent<Collider2D>();
        if (mainCollider != null)
            mainCollider.enabled = true;
        
        // If this is a boss, setup boss UI
        if (isBoss)
        {
            if(!gameManager.uIManager.isBossSliderActive())
            {
                gameManager.uIManager.ToggleBossHealth();
            }

            float maxHealth = health.GetMaxHealth();
            gameManager.uIManager.SetBossMaxHealth(maxHealth);
            OnBossHealthUpdate?.Invoke(health.GetHealth());

            Debug.Log($"Enemy.OnPoolSpawn(): Boss {gameObject.name} spawned - max health: {maxHealth}");
        }
    }

    public void OnPoolDespawn()
    {
        // Stop any delayed/looped behaviors from previous life before returning to pool
        CancelInvoke();
        StopAllCoroutines();
        canShoot = false;

        // Disable only decorative child objects (skip the first child which is the actual sprite)
        for (int i = 1; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        // Call virtual method for subclasses to override
        OnPoolDespawnInternal();
        
        // Base cleanup - always happens last
        target = null;
        
        // Reset boss flag cleanup - this should trigger before pool return
        if (isBoss)
        {
            Debug.Log($"Enemy.OnPoolDespawn(): Boss {gameObject.name} returned to pool - invoking OnBossDeath");
            OnBossDeath?.Invoke();
            
            // CRITICAL: Reset boss flag so this enemy doesn't spawn as boss next time
            isBoss = false;
            Debug.Log($"Enemy.OnPoolDespawn(): Reset isBoss flag to false for {gameObject.name}");
        }
    }

    /// <summary>
    /// Virtual method for subclasses to override for custom pool cleanup
    /// </summary>
    protected virtual void OnPoolDespawnInternal()
    {
        // Base implementation - can be overridden by subclasses
    }

    private void ReturnToPool()
    {
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.ReturnEnemy(this);
        }
        else
        {
            Debug.LogError("PoolManager.Instance is null! Enemy pooling is not set up correctly. Enemy will be destroyed as fallback.");
            // Fallback if pool manager doesn't exist
            Destroy(gameObject);
        }
    }
    #endregion
    
    protected virtual void OnDestroy()
    {
        // This method is only called when the object is actually destroyed (not pooled)
        // Pool cleanup is handled in OnPoolDespawn()
        Debug.Log($"Enemy.OnDestroy(): {gameObject.name} actually destroyed (not pooled)");
    }

}
