using UnityEngine;

public class ShooterEnemy : Enemy
{
    [SerializeField] protected float attackRange = 0;
    [SerializeField] protected float attackTime = 1f;
    [SerializeField] protected Bullet bulletPrefab;
    [SerializeField] private Transform point;
    [SerializeField] private Transform canon;

    protected override void Start()
    {
        base.Start();
        health = new Health(1, 0, 1);
        pointsValue = 5;
        InvokeRepeating(nameof(Shoot), 0, attackTime);
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
            weapon.Shoot(bulletPrefab, point, new string[] {"Player"}, 0);
    }

    public void SetShooterEnemy(float _attackRange, float _attackTime){
        attackRange = _attackRange;
        attackTime = _attackTime;
    }

    public override int GetPointsValue()
    {
        return pointsValue;
    }
}
