using System.Collections;
using UnityEngine;

public class BlinderEnemy : Enemy
{
    [SerializeField] private float attackRange = 0;
    [SerializeField] private float attackTime = 1f;
    [SerializeField] private GameObject InkSpots;
    private bool lasering, cooldown = false;

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

        if (Vector2.Distance(transform.position, target.position) < attackRange && !lasering)
        {
            lasering = true;
            StartCoroutine(Ink());
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

    public void SetBlinderEnemy(float _attackRange, float _attackTime)
    {
        attackRange = _attackRange;
        attackTime = _attackTime;
    }

    public override int GetPointsValue()
    {
        return pointsValue;
    }

    IEnumerator Ink()
    {
        InkSpots.SetActive(true);
        target.GetComponent<Player>().Blind();
        yield return new WaitForSeconds(attackTime);
        InkSpots.SetActive(false);
        cooldown = true;

    }

    IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(attackTime);
        lasering = false;
        cooldown = false;
        
    }
}
