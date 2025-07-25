using System;
using UnityEngine;

public class MiniEnemy : ShooterEnemy
{
    public Action OnDeath;

    void OnDestroy()
    {
        OnDeath?.Invoke();
    }

}
