using System;
using UnityEngine;

public class MiniEnemy : ShooterEnemy
{
    

    protected override void Start()
    {
        base.Start();
        health = new Health(1*gameManager.multiplierEnemyHealth*(isBoss?30:1), 0, 1*gameManager.multiplierEnemyHealth*(isBoss?30:1));
        pointsValue = 1*(int)gameManager.multiplierPoint*(isBoss?30:1);
        speed *= gameManager.multiplierEnemySpeed;
        InvokeRepeating(nameof(Shoot), 0, attackTime);
        gameObject.transform.localScale = gameObject.transform.localScale * (isBoss?5:1);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        OnDeath?.Invoke();
    }

}
