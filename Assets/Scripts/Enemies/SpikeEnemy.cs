using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using Unity.VisualScripting;

public class SpikeEnemy : ShooterEnemy
{
    [SerializeField] private Transform points;

    protected override void Start()
    {
        base.Start();
        health = new Health(3, 0, 3);
        pointsValue = 3;
        InvokeRepeating(nameof(Shoot), 0, attackTime);
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

    public void SetSpikeEnemy(float _attackRange, float _attackTime){
        attackRange = _attackRange;
        attackTime = _attackTime;
    }

    public override int GetPointsValue()
    {
        return pointsValue;
    }
}
