using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MachineGunEnemy : ShooterEnemy  
{
    [SerializeField] private float shootingTime = 2f;
    [SerializeField] private Transform point1, point2;
    private float timer;
    private bool isShooting = false;

    protected override void Start()
    {
        base.Start();
        health = new Health(1 * gameManager.multiplierEnemyHealth * (isBoss ? 30 : 1), 0, 1 * gameManager.multiplierEnemyHealth * (isBoss ? 30 : 1));
        pointsValue = 4 * (int)gameManager.multiplierPoint * (isBoss ? 30 : 1);
        speed *= gameManager.multiplierEnemySpeed;
        canShoot = false;
        InvokeRepeating(nameof(Shoot), 0, attackTime);
        isShooting = true;
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
            weapon.Shoot(bulletPrefab, point1, new string[] {"Player"}, (isBoss?5:0));
            weapon.Shoot(bulletPrefab, point2, new string[] {"Player"}, (isBoss?5:0));
        }
    }

    public override int GetPointsValue()
    {
        return pointsValue;
    }

    IEnumerator CooldownShoot(){
        yield return new WaitForSeconds(shootingTime);
        
    }

}
