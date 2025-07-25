using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class ExploderEnemy : Enemy
{
    [SerializeField] private float explosionRange = 5f;
    [SerializeField] private float attackRange = 1;
    [SerializeField] private float attackTime = 3f;
    [SerializeField] private GameObject antenna1, antenna2, antenna3, eyes, hitEffect;
    private bool exploding = false;

    private float timer;

    protected override void Start()
    {
        base.Start();
        health = new Health(4, 0, 4);
        pointsValue = 4;
    }

    protected override void Update()
    {
        base.Update();
        if (target == null){
            return;
        }

        if (Vector2.Distance(transform.position, target.position) < attackRange && !exploding){
            speed=0;
            exploding=true;
            StartCoroutine(Explode());
        }
    }

    public override void Attack(float interval){}

    public override void GetDamage(float damage)
    {
        base.GetDamage(damage);
    }

    public void SetExploderEnemy(float _attackRange, float _attackTime){
        attackRange = _attackRange;
        attackTime = _attackTime;
    }

    IEnumerator Explode(){
        while (timer <= 4 ){
            if (timer < attackTime)
            {
                if (timer == 1)
                {
                    antenna1.SetActive(true);
                }
                if (timer == 2)
                {
                    antenna2.SetActive(true);
                }
                if (timer == 3)
                {
                    antenna3.SetActive(true);
                    eyes.SetActive(true);
                }
                    
            }
            else
            {
                timer = 0f;
                speed = 0f;
                Instantiate(hitEffect, transform.position, Quaternion.identity);
                if (Vector2.Distance(transform.position, target.position) < explosionRange)
                {
                    target.gameObject.GetComponent<IDamageable>().GetDamage(weapon.GetDamage());
                }
                Destroy(gameObject);
            }
            timer+=1;
            yield return new WaitForSeconds(1);
        }
    }
}
