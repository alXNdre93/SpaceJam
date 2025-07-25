using System;
using UnityEngine;

public class MiniEnemy : ShooterEnemy
{
    public Action OnDeath;

    protected override void Start()
    {
        base.Start();
        health = new Health(1, 0, 1);
        pointsValue = 1;
        InvokeRepeating(nameof(Shoot), 0, attackTime);
    }

    void OnDestroy()
    {
        OnDeath?.Invoke();
    }

}
