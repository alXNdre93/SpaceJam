using NUnit.Framework;
using UnityEngine;

public class Enemy : PlayableObject
{
    private string enemyName;
    [SerializeField] protected float speed;
    [SerializeField] protected Transform target;
    [SerializeField] protected bool isBoss;
    private EnemyType enemyType;

    protected virtual void Start()
    {
        if (GameObject.FindWithTag("Player") != null)
            target = GameObject.FindWithTag("Player").transform;
        health = new Health(1,0.1f,1);
    }
    protected virtual void Update()
    {
        if(target != null){
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
        if(health.GetHealth() <= 0){
            Die();
        }
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
        GameManager.GetInstance().NotifyDeath(this);
        Destroy(gameObject);
    }

    public override void Shoot()
    {
        throw new System.NotImplementedException();
    }

    public override int GetPointsValue()
    {
        return base.GetPointsValue();
    }

}
