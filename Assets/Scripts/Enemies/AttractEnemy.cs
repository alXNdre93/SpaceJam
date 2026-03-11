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
        if (target == null || target.gameObject == null)
        {
            return;
        }

        Player playerComponent = target.gameObject.GetComponent<Player>();
        if (playerComponent == null)
        {
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        
        if (distanceToTarget < attackRange && !absorbs)
        {
            absorbs = true;
            StartCoroutine(Attract());
            playerComponent.attracted = false;
        }
        else if (distanceToTarget < attackRange && absorbs)
        {
            playerComponent.attracted = true;
            if (distanceToTarget < (attackRange / 2))
            {
                Debug.Log("damage");
                IDamageable damageable = target.gameObject.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.GetDamage(weapon.GetDamage() * Time.deltaTime);
                }
            }
        }
        else
        {
            playerComponent.attracted = false;
        }

        if (cooldown)
            StartCoroutine(Cooldown());
    }
    void OnDisable()
    {
        if (target != null && target.gameObject != null)
        {
            Player playerComponent = target.gameObject.GetComponent<Player>();
            if (playerComponent != null)
            {
                playerComponent.attracted = false;
            }
        }
    }
    protected override void OnDestroy()
    {
        if (target != null && target.gameObject != null)
        {
            Player playerComponent = target.gameObject.GetComponent<Player>();
            if (playerComponent != null)
            {
                playerComponent.attracted = false;
            }
        }
        base.OnDestroy();
    }

    // Override OnPoolDespawnInternal to ensure player attraction state is cleaned up
    protected override void OnPoolDespawnInternal()
    {
        if (target != null && target.gameObject != null)
        {
            Player playerComponent = target.gameObject.GetComponent<Player>();
            if (playerComponent != null)
            {
                playerComponent.attracted = false;
            }
        }
        
        // Reset attraction state
        absorbs = false;
        cooldown = false;
        
        // Ensure absorption effect is disabled
        if (absorption != null)
        {
            absorption.SetActive(false);
        }
        
        base.OnPoolDespawnInternal();
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
