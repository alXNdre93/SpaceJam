using UnityEngine;
using System.Collections;

public class AsteroidHazard : MonoBehaviour, IDamageable, IPoolable
{
    [SerializeField] private float maxHealth = 25f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float damageRadius = 1.5f;
    [SerializeField] private float impactDamage = 15f;
    [SerializeField] private int pointsValue = 10;
    [SerializeField] private bool isMoving = true;
    
    private float currentHealth;
    private Vector2 moveDirection;
    private Transform target;
    private bool hasImpacted = false;
    private Coroutine lifetimeCoroutine;
    private GameManager gameManager;
    private PickupManager pickupManager;
    
    private void Awake()
    {
        currentHealth = maxHealth;
        // Set appropriate tag so bullets can target this asteroid
        gameObject.tag = "Asteroid";
    }
    
    private void Start()
    {
        gameManager = GameManager.GetInstance();
        pickupManager = FindObjectOfType<PickupManager>();
        
        if (isMoving)
        {
            // Find player to move towards
            Player player = gameManager?.Getplayer();
            if (player != null)
            {
                target = player.transform;
                SetMoveDirection();
            }
        }
    }
    
    private void Update()
    {
        if (isMoving && !hasImpacted && target != null)
        {
            Move();
            CheckForCollisions();
        }
    }
    
    private void SetMoveDirection()
    {
        if (target != null)
        {
            moveDirection = (target.position - transform.position).normalized;
        }
    }
    
    private void Move()
    {
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }
    
    private void CheckForCollisions()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, damageRadius);
        
        foreach (Collider2D collider in colliders)
        {
            if (hasImpacted) break;
            
            // Check for player
            if (collider.CompareTag("Player"))
            {
                Player player = collider.GetComponent<Player>();
                if (player != null)
                {
                    player.GetDamage(impactDamage);
                    Debug.Log("Asteroid hit player!");
                    Impact();
                }
            }
            
            // Check for enemies
            if (collider.CompareTag("Enemy"))
            {
                Enemy enemy = collider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.GetDamage(impactDamage);
                    Debug.Log("Asteroid hit enemy!");
                    Impact();
                }
            }
        }
    }
    
    private void Impact()
    {
        if (hasImpacted) return;
        
        hasImpacted = true;
        
        // Create impact effect if available in EventSystem
        EventSystem eventSystem = gameManager?.GetEventSystem();
        if (eventSystem != null)
        {
            eventSystem.CreateImpactEffect(transform.position);
        }
        
        ReturnToPool();
    }
    
    public void SetAsMoving(Vector2 spawnPosition, Transform playerTarget)
    {
        isMoving = true;
        target = playerTarget;
        transform.position = spawnPosition;
        SetMoveDirection();
        hasImpacted = false;
        currentHealth = maxHealth;
    }
    
    public void SetAsStatic(Vector2 spawnPosition)
    {
        isMoving = false;
        target = null;
        transform.position = spawnPosition;
        hasImpacted = false;
        currentHealth = maxHealth;
    }
    
    #region IDamageable Implementation
    public void GetDamage(float damage)
    {
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public float GetHealth()
    {
        return currentHealth;
    }
    
    public int GetPointsValue()
    {
        return pointsValue;
    }
    #endregion
    
    private void Die()
    {
        // Add score
        if (gameManager != null && gameManager.scoreManager != null)
        {
            gameManager.scoreManager.IncrementScore(pointsValue);
        }
        
        // Spawn pickup (no experience though)
        if (pickupManager != null)
        {
            pickupManager.SpawnPickup(transform.position);
        }
        
        ReturnToPool();
    }
    
    #region IPoolable Implementation
    public void OnPoolSpawn()
    {
        currentHealth = maxHealth;
        hasImpacted = false;
        gameObject.SetActive(true);
        
        // Start lifetime timer (10 seconds)
        if (lifetimeCoroutine != null)
            StopCoroutine(lifetimeCoroutine);
        lifetimeCoroutine = StartCoroutine(LifetimeTimer());
    }
    
    public void OnPoolDespawn()
    {
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }
        
        hasImpacted = false;
        target = null;
        gameObject.SetActive(false);
    }
    
    private IEnumerator LifetimeTimer()
    {
        yield return new WaitForSeconds(10f);
        ReturnToPool();
    }
    
    private void ReturnToPool()
    {
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.ReturnAsteroid(this);
        }
        else
        {
            Debug.LogWarning("PoolManager not found, destroying asteroid");
            Destroy(gameObject);
        }
    }
    #endregion
    
    private void OnDrawGizmosSelected()
    {
        // Visualize damage radius in editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}