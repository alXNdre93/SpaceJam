using UnityEngine;

public class ShooterEnemy : Enemy
{
    [SerializeField] protected Bullet bulletPrefab;
    [SerializeField] private Transform point;

    protected override void Start()
    {
        base.Start();
        health = new Health(1*gameManager.multiplierEnemyHealth*(isBoss?30:1), 0, 1*(isBoss?30:1));
        pointsValue = 5*(int)gameManager.multiplierPoint*(isBoss?30:1);
        speed *= gameManager.multiplierEnemySpeed;
        InvokeRepeating(nameof(Shoot), 0, attackTime);
        gameObject.transform.localScale = gameObject.transform.localScale * (isBoss?5:1);
    }

    protected override void Update()
    {
        base.Update();
        if (target == null){
            return;
        }

        if (Vector2.Distance(transform.position, target.position) < attackRange){
            canShoot=true;
        }

    }

    public override void Attack(float interval){}

    public override void GetDamage(float damage)
    {
        base.GetDamage(damage);
    }

    public override void Shoot()
    {
        if (canShoot)
            weapon.Shoot(bulletPrefab, point, new string[] {"Player"}, (isBoss?5:0));
    }

    public override int GetPointsValue()
    {
        return pointsValue;
    }
    
}
