
using System.Collections;
using UnityEngine;

public class ElectricEnemy : Enemy
{
    [SerializeField] private float attackRange = 1;
    [SerializeField] private float attackTime = 3f;
    [SerializeField] private GameObject lighting;
    private bool electrocuting = false;

    private float timer, timerAttack;

    protected override void Start()
    {
        base.Start();
        health = new Health(2, 0, 2);
        pointsValue = 1;
    }

    protected override void Update()
    {
        base.Update();
        timer += Time.deltaTime;
        if (target == null)
        {
            return;
        }

        if (Vector2.Distance(transform.position, target.position) < attackRange && !electrocuting)
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
            if (Vector2.Distance(transform.position, target.position) < attackRange && electrocuting)
            {
                target.gameObject.GetComponent<IDamageable>().GetDamage(weapon.GetDamage());
                target.gameObject.GetComponent<Player>().electrocuted = true;
            }
        }
    }

    public override void GetDamage(float damage)
    {
        base.GetDamage(damage);
    }

    public void SetElectricEnemy(float _attackRange, float _attackTime){
        attackRange = _attackRange;
        attackTime = _attackTime;
    }

    IEnumerator Electrify()
    {
        lighting.SetActive(true);
        yield return new WaitForSeconds(2);
        lighting.SetActive(false);
    }
}
