using System.Collections;
using UnityEngine;

public class SpawnEnemy : Enemy
{
    [SerializeField] private float attackRange = 1;
    [SerializeField] private float attackTime = 3f;
    [SerializeField] private Transform point;
    [SerializeField] private MiniEnemy miniEnnemyPrefab;
    [SerializeField] private int nbToSpawn = 10;
    private int totalSpawn = 0;
    private Weapon miniWeapon = new Weapon("mini", 0.1f, 15);
    private bool spawned = false;


    protected override void Start()
    {
        base.Start();
        health = new Health(2, 0, 2);
        pointsValue = 1;
    }

    protected override void Update()
    {
        base.Update();
        if (target == null)
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

    public void SetSpawnerEnemy(float _attackRange, float _attackTime)
    {
        attackRange = _attackRange;
        attackTime = _attackTime;
    }

    IEnumerator SpawnEnemies()
    {
        while (totalSpawn < nbToSpawn)
        {
            Debug.Log("Mini Enemy spawn: " + totalSpawn);
            MiniEnemy tempEnemy = Instantiate(miniEnnemyPrefab, point.position, point.rotation);
            tempEnemy.SetShooterEnemy(5, 2);
            tempEnemy.weapon = miniWeapon;
            tempEnemy.OnDeath += MiniDestroy;
            yield return new WaitForSeconds(attackTime);
            totalSpawn++;
        }
    }

}
