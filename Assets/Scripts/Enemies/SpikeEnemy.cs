using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class SpikeEnemy : ShooterEnemy
{
    [SerializeField] private Transform points;

    protected override void Start()
    {
        base.Start();
        health = new Health(3 * gameManager.multiplierEnemyHealth * (isBoss ? 30 : 1), 0, 3 * gameManager.multiplierEnemyHealth * (isBoss ? 30 : 1));
        pointsValue = 3 * (isBoss ? 30 : 1);
        InvokeRepeating(nameof(Shoot), 0, attackTime);
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
    }

    public override void Attack(float interval)
    {
        
    }

    public override void GetDamage(float damage)
    {
        base.GetDamage(damage);
    }

    public override void Shoot()
    {
        for (int i = 0; i < points.childCount; i++)
        {
            weapon.Shoot(bulletPrefab, points.GetChild(i), new string[] { "Player" }, 0);
        }
    }

    public override int GetPointsValue()
    {
        return pointsValue;
    }

}
