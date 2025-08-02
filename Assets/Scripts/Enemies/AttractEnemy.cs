using System.Collections;
using UnityEngine;

public class AttractEnemy : Enemy
{
    [SerializeField] private GameObject absorption;
    private bool absorbs, cooldown = false;

    protected override void Start()
    {
        base.Start();
        health = new Health(1 * gameManager.multiplierEnemyHealth * (isBoss ? 30 : 1), 0, 1 * gameManager.multiplierEnemyHealth * (isBoss ? 30 : 1));
        pointsValue = 3 * (isBoss ? 30 : 1);
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

        if (Vector2.Distance(transform.position, target.position) < attackRange && !absorbs)
        {
            absorbs = true;
            StartCoroutine(Attract());
            target.gameObject.GetComponent<Player>().attracted = false;
        }
        else if (Vector2.Distance(transform.position, target.position) < attackRange && absorbs)
        {
            target.gameObject.GetComponent<Player>().attracted = true;
            if (Vector2.Distance(transform.position, target.position) < (attackRange / 2))
            {
                Debug.Log("damage");
                target.gameObject.GetComponent<IDamageable>().GetDamage(weapon.GetDamage() * Time.deltaTime);
            }
        }
        else
        {
            target.gameObject.GetComponent<Player>().attracted = false;
        }

        if (cooldown)
            StartCoroutine(Cooldown());
    }
    void OnDisable()
    {
        target.gameObject.GetComponent<Player>().attracted = false;
    }
    protected override void OnDestroy()
    {
        target.gameObject.GetComponent<Player>().attracted = false;
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

    IEnumerator Attract()
    {
        absorption.SetActive(true);
        yield return new WaitForSeconds(attackTime);
        absorption.SetActive(false);
        cooldown = true;

    }

    IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(attackTime);
        absorbs = false;
        cooldown = false;

    }

}
