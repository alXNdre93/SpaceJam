using UnityEngine;

public interface IDamageable
{
    int GetPointsValue();
    void GetDamage(float damage);
    public float GetHealth();
}
