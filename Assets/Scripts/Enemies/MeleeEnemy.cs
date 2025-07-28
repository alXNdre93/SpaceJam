using NUnit.Framework.Interfaces;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MeleeEnemy : Enemy
{
    [SerializeField] private GameObject eyes;

    private float timer;
    private float setSpeed = 1f;
    private bool isSpinning = false;
    private float speedBoost = 5f;
    private float speedUpRange = 5f;

    protected override void Start()
    {
        base.Start();
        health = new Health(2*(isBoss?30:1), 0, 2*(isBoss?30:1));
        pointsValue = 3*(isBoss?30:1);
        gameObject.transform.localScale = gameObject.transform.localScale * (isBoss?5:1);
    }

    protected override void Update()
    {
        base.Update();
        if (target == null){
            return;
        }

        if (Vector2.Distance(transform.position, target.position) < attackRange){
            speed = 0;
            Attack(attackTime);
        }else{
            speed = setSpeed;
        }

        if (Vector2.Distance(transform.position, target.position) < speedUpRange)
        {
            if (!isSpinning)
            {
                isSpinning = true;
                GetComponent<Animator>().SetBool("Attack", true);
                eyes.SetActive(true);
                speed += speedBoost;
            }
        }
        else
        {
            if (isSpinning)
            {
                speed = setSpeed;
            }
            isSpinning = false;
            GetComponent<Animator>().SetBool("Attack", false);
            eyes.SetActive(false);
        }

    }

    public override void Attack(float interval)
    {
        if (timer <= interval){
            timer += Time.deltaTime;
        }else{
            timer = 0f;
            target.gameObject.GetComponent<IDamageable>().GetDamage(weapon.GetDamage());
        }
    }

    public override void GetDamage(float damage)
    {
        base.GetDamage(damage);
    }

    public override int GetPointsValue()
    {
        return pointsValue;
    }

}
