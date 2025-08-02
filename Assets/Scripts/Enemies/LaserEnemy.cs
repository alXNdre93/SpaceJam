using System.Collections;
using UnityEngine;

public class LaserEnemy : Enemy
{
    [SerializeField] private GameObject lasers;
    private bool lasering, cooldown = false;

    protected override void Start()
    {
        base.Start();
        health = new Health(1 * gameManager.multiplierEnemyHealth * (isBoss ? 30 : 1), 0, 1 * gameManager.multiplierEnemyHealth * (isBoss ? 30 : 1));
        pointsValue = 3 * (int)gameManager.multiplierPoint * (isBoss ? 30 : 1);
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
        if (target == null)
        {
            return;
        }

        if (Vector2.Distance(transform.position, target.position) < attackRange && !lasering)
        {
            lasering = true;
            StartCoroutine(Laser());
        }else if (Vector2.Distance(transform.position, target.position) < attackRange ){
            target.gameObject.GetComponent<IDamageable>().GetDamage(weapon.GetDamage() * Time.deltaTime);
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
