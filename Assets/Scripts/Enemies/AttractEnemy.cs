using System.Collections;
using UnityEngine;

public class AttractEnemy : Enemy
{
    [SerializeField] private float attackRange = 0;
    [SerializeField] private float attackTime = 1f;
    [SerializeField] private GameObject absorption;
    private bool absorbs, cooldown = false;

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
    void OnDestroy()
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

    public void SetAbsorbsEnemy(float _attackRange, float _attackTime)
    {
        attackRange = _attackRange;
        attackTime = _attackTime;
    }

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
