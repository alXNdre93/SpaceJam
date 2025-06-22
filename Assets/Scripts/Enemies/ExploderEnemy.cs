using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class ExploderEnemy : Enemy
{
    [SerializeField] private float explosionRange = 5f;
    [SerializeField] private float attackRange = 1;
    [SerializeField] private float attackTime = 3f;
    [SerializeField] private GameObject detonator1, detonator2, detonator3;
    private bool exploding = false;

    private float timer;

    protected override void Start()
    {
        base.Start();
        health = new Health(4, 0, 4);
        pointsValue = 1;
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
            if (timer < attackTime){
                if (timer == 1){
                    detonator1.SetActive(false);
                }
                if (timer == 2){
                    detonator2.SetActive(false);
                }
            }else{
                timer = 0f;
                speed = 0f;
                detonator3.SetActive(false);
                if (Vector2.Distance(transform.position, target.position) < explosionRange){
                    target.gameObject.GetComponent<IDamageable>().GetDamage(weapon.GetDamage());
                }
                Destroy(gameObject);
            }
            timer+=1;
            yield return new WaitForSeconds(1);
        }
    }
}
