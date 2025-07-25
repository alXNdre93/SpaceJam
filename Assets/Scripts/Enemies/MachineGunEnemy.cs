using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;

public class MachineGunEnemy : ShooterEnemy  
{
    [SerializeField] private float shootingTime = 2f;
    [SerializeField] private Transform point1, point2;
    private float timer;
    private bool isShooting = false;

    protected override void Start()
    {
        base.Start();
        health = new Health(1, 0, 1);
        pointsValue = 4;
        canShoot = false;
        InvokeRepeating(nameof(Shoot), 0, attackTime);
        isShooting = true;
    }

    protected override void Update()
    {
        base.Update();
        if (target == null){
            return;
        }
        if (Vector2.Distance(transform.position, target.position) < attackRange){
            canShoot = true;
            Attack(shootingTime);
        }else{
            canShoot = false;
            Attack(shootingTime);
        }
    }

    public override void Attack(float interval)
    {
        if (timer <= interval){
            timer += Time.deltaTime;
        }else{
            timer = 0f;
            if (isShooting){
                CancelInvoke(nameof(Shoot));
                isShooting = false;
            }else{
                InvokeRepeating(nameof(Shoot), 0, attackTime);
                isShooting = true;
            }
        }
    }

    public override void GetDamage(float damage)
    {
        base.GetDamage(damage);
    }

    public override void Shoot()
    {
        if (canShoot){
            weapon.Shoot(bulletPrefab, point1, new string[] {"Player"}, 0);
            weapon.Shoot(bulletPrefab, point2, new string[] {"Player"}, 0);
        }
    }

    public void SetMachineGunEnemy(float _attackRange, float _attackTime){
        attackRange = _attackRange;
        attackTime = _attackTime;
    }

    public override int GetPointsValue()
    {
        return pointsValue;
    }

    IEnumerator CooldownShoot(){
        yield return new WaitForSeconds(shootingTime);
        
    }
}
