using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using System.Collections;

public class Bullet : MonoBehaviour, IPoolable
{
    [SerializeField] private float speed;
    [SerializeField] private float damage;
    [SerializeField] private float lifeTime = 10f; // Auto-return to pool after this time

    private string[] targetTag;
    private Coroutine lifeTimeCoroutine;
    private Vector3 initialLocalScale;

    private void Awake()
    {
        initialLocalScale = transform.localScale;
    }

    public void SetBullet(float _damage, string[] _targetTag, float _speed = 10f)
    {
        damage = _damage;
        targetTag = _targetTag;
        speed = _speed;
    }

    #region IPoolable Implementation
    public void OnPoolSpawn()
    {
        // Reset bullet state when taken from pool
        transform.rotation = Quaternion.identity;
        transform.localScale = initialLocalScale;
        
        // Start lifetime timer
        if (lifeTimeCoroutine != null)
            StopCoroutine(lifeTimeCoroutine);
        lifeTimeCoroutine = StartCoroutine(LifeTimeTimer());
    }

    public void OnPoolDespawn()
    {
        // Clean up when returned to pool
        if (lifeTimeCoroutine != null)
        {
            StopCoroutine(lifeTimeCoroutine);
            lifeTimeCoroutine = null;
        }

        // Reset mutable runtime state so pooled bullets don't keep stale hitboxes/targets
        transform.localScale = initialLocalScale;
        targetTag = null;
    }

    private IEnumerator LifeTimeTimer()
    {
        yield return new WaitForSeconds(lifeTime);
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.ReturnBullet(this);
        }
        else
        {
            Debug.LogError("PoolManager.Instance is null! This means pooling is not set up correctly. Bullet will be destroyed as fallback.");
            // Fallback if pool manager doesn't exist - this should rarely happen
            Destroy(gameObject);
        }
    }
    #endregion

    private void Update()
    {
        Move();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool rightTarget = false;
        if (collision.CompareTag("Border")){
            ReturnToPool();
            return;
        }

        if (targetTag == null || targetTag.Length == 0)
        {
            ReturnToPool();
            return;
        }

        foreach(string tag in targetTag){
            if (string.IsNullOrEmpty(tag))
                continue;
            if (collision.CompareTag(tag) && rightTarget == false){
                rightTarget = true;
            } 
        }
        if (!rightTarget)
            return;
        
        IDamageable damageable = collision.GetComponent<IDamageable>();
        Damage(damageable);
    }

    void Damage(IDamageable damageable)
    {
        if (damageable != null){
            damageable.GetDamage(damage);

            if(damageable.GetHealth() == 0)
                GameManager.GetInstance().scoreManager.IncrementScore(damageable.GetPointsValue());

            ReturnToPool();
        }
    }

    private void Move()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

}
