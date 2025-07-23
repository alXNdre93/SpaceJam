using System.Collections;
using UnityEngine;

public class LaserEnemy : Enemy
{
    [SerializeField] private float attackRange = 0;
    [SerializeField] private float attackTime = 1f;
    [SerializeField] private GameObject lasers;
    private bool lasering, cooldown = false;

    protected override void Start()
    {
        base.Start();
        health = new Health(1, 0, 1);
        pointsValue = 2;
    }

    protected override void Update()
    {
        base.Update();
        if (target == null)
        {
            return;
        }

        if (Vector2.Distance(transform.position, target.position) < attackRange && !lasering)
        {
            lasering = true;
            StartCoroutine(Laser());
        }

        if (cooldown)
            StartCoroutine(Cooldown());
    }

    public override void Attack(float interval)
    { }

    public override void GetDamage(float damage)
    {
        base.GetDamage(damage);
    }

    public override void Shoot()
    { }

    public void SetLaserEnemy(float _attackRange, float _attackTime)
    {
        attackRange = _attackRange;
        attackTime = _attackTime;
    }

    public override int GetPointsValue()
    {
        return pointsValue;
    }

    IEnumerator Laser()
    {
        lasers.SetActive(true);
        yield return new WaitForSeconds(attackTime);
        lasers.SetActive(false);
        cooldown = true;

    }

    IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(attackTime);
        lasering = false;
        cooldown = false;
        
    }
}
