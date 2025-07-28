using System;
using UnityEngine;

public class MiniEnemy : ShooterEnemy
{
    

    protected override void Start()
    {
        base.Start();
        health = new Health(1*(isBoss?30:1), 0, 1*(isBoss?30:1));
        pointsValue = 1*(isBoss?30:1);
        InvokeRepeating(nameof(Shoot), 0, attackTime);
        gameObject.transform.localScale = gameObject.transform.localScale * (isBoss?5:1);
    }

    protected void OnDestroy()
    {
        base.OnDestroy();
        OnDeath?.Invoke();
    }

}
