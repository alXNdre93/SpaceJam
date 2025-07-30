using System.Collections;
using UnityEngine;

public class BlinderEnemy : Enemy
{
    [SerializeField] private GameObject InkSpots;
    private bool lasering, cooldown = false;

    protected override void Start()
    {
        base.Start();
        health = new Health(2*gameManager.multiplierEnemyHealth*(isBoss?30:1), 0, 2*(isBoss?30:1));
        pointsValue = 1*(int)gameManager.multiplierPoint*(isBoss?30:1);
        speed *= gameManager.multiplierEnemySpeed;
        gameObject.transform.localScale = gameObject.transform.localScale * (isBoss?5:1);
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
