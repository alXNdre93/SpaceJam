
using System.Collections;
using UnityEngine;

public class ElectricEnemy : Enemy
{
    [SerializeField] private GameObject lighting;
    private bool electrocuting = false;

    private float timer, timerAttack;

    protected override void Start()
    {
        base.Start();
        health = new Health(2 * gameManager.multiplierEnemyHealth * (isBoss ? 30 : 1), 0, 2 * gameManager.multiplierEnemyHealth * (isBoss ? 30 : 1));
        pointsValue = 2 * (int)gameManager.multiplierPoint * (isBoss ? 30 : 1);
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
        timer += Time.deltaTime;
        if (target == null)
        {
            return;
        }

        if (Vector2.Distance(transform.position, target.position) < attackRange * (isBoss?5:1) && !electrocuting)
        {
            electrocuting = true;
            StartCoroutine(Electrify());
            timer = 0;
        }

        if (electrocuting && timer >= attackTime)
        {
            electrocuting = false;
        }
        Attack(attackTime);
    }

    public override void Attack(float interval)
    {
        if (timerAttack <= interval)
        {
            timerAttack += Time.deltaTime;
        }
        else
        {
            timerAttack = 0;
            if (target != null && target.gameObject != null && 
                Vector2.Distance(transform.position, target.position) < attackRange * (isBoss?5:1) && electrocuting)
            {
                IDamageable damageable = target.gameObject.GetComponent<IDamageable>();
                Player playerComponent = target.gameObject.GetComponent<Player>();
                
                if (damageable != null)
                {
                    damageable.GetDamage(weapon.GetDamage());
                }
                
                if (playerComponent != null)
                {
                    playerComponent.electrocuted = true;
                }
            }
        }
    }

    public override void GetDamage(float damage)
    {
        base.GetDamage(damage);
    }

    IEnumerator Electrify()
    {
        lighting.SetActive(true);
        yield return new WaitForSeconds(2);
        lighting.SetActive(false);
    }

    // Override OnPoolDespawnInternal to ensure player electrocuted state is cleaned up
    protected override void OnPoolDespawnInternal()
    {
        if (target != null && target.gameObject != null)
        {
            Player playerComponent = target.gameObject.GetComponent<Player>();
            if (playerComponent != null)
            {
                playerComponent.electrocuted = false;
            }
        }
        
        // Reset electrocution state
        electrocuting = false;
        timer = 0;
        timerAttack = 0;
        
        // Ensure lighting effect is disabled
        if (lighting != null)
        {
            lighting.SetActive(false);
        }
        
        base.OnPoolDespawnInternal();
    }
}
