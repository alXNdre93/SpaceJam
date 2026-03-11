using System.Collections;
using UnityEngine;

public class SpawnEnemy : Enemy
{
    [SerializeField] private Transform point;
    [SerializeField] private MiniEnemy miniEnnemyPrefab;
    [SerializeField] private int nbToSpawn = 10;
    private int totalSpawn = 0;
    private Weapon miniWeapon = new Weapon("mini", 0.1f, 15);
    private bool spawned = false;


    protected override void Start()
    {
        base.Start();
        health = new Health(8 * gameManager.multiplierEnemyHealth * (isBoss ? 30 : 1), 0, 8 * gameManager.multiplierEnemyHealth * (isBoss ? 30 : 1));
        pointsValue = 10 * (int)gameManager.multiplierPoint * (isBoss ? 30 : 1);
        speed *= gameManager.multiplierEnemySpeed;
        gameObject.transform.localScale = gameObject.transform.localScale * (isBoss ? 5 : 1);
        if (isBoss)
        {
            FindAnyObjectByType<UIManager>().SetBossMaxHealth(health.GetMaxHealth());
            FindAnyObjectByType<UIManager>().UpdateEnemyHealth(health.GetMaxHealth());
        }
    }

    protected override void Update()
    {
        base.Update();
        if (target == null || target.gameObject == null)
        {
            return;
        }

        if (Vector2.Distance(transform.position, target.position) <= attackRange && !spawned)
        {
            spawned = true;
            StartCoroutine(SpawnEnemies());
        }
    }
    public override void Move(Vector2 direction)
    {
        if (target == null || target.gameObject == null)
            return;
            
        direction.x -= transform.position.x;
        direction.y -= transform.position.y;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        if (Vector2.Distance(transform.position, target.position) > attackRange)
            transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    public override void Attack(float interval) { }

    public override void GetDamage(float damage)
    {
        base.GetDamage(damage);
    }

    public void MiniDestroy()
    {
        totalSpawn--;
        if (totalSpawn == 0)
            spawned = false;
    }

    IEnumerator SpawnEnemies()
    {
        while (totalSpawn < nbToSpawn)
        {
            Debug.Log("Mini Enemy spawn: " + totalSpawn);
            
            // Use pool manager if available, otherwise fallback to instantiate
            MiniEnemy tempEnemy;
            if (PoolManager.Instance != null)
            {
                // Try to get from pool (assuming MiniEnemy is also pooled)
                Enemy enemyFromPool = PoolManager.Instance.GetEnemy(miniEnnemyPrefab.gameObject);
                if (enemyFromPool != null && enemyFromPool is MiniEnemy)
                {
                    tempEnemy = (MiniEnemy)enemyFromPool;
                    tempEnemy.transform.position = point.position;
                    tempEnemy.transform.rotation = point.rotation;
                }
                else
                {
                    tempEnemy = Instantiate(miniEnnemyPrefab, point.position, point.rotation);
                    Debug.LogWarning("MiniEnemy pool not found or returned null, falling back to Instantiate");
                }
            }
            else
            {
                tempEnemy = Instantiate(miniEnnemyPrefab, point.position, point.rotation);
                Debug.LogWarning("PoolManager not found, falling back to Instantiate for MiniEnemy");
            }
            
            tempEnemy.SetEnemy(5, 2, isBoss);
            tempEnemy.weapon = miniWeapon;
            tempEnemy.OnDeath += MiniDestroy;
            yield return new WaitForSeconds(attackTime);
            totalSpawn++;
        }
    }

}
