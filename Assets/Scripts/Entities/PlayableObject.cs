using UnityEngine;

public abstract class PlayableObject : MonoBehaviour, IDamageable
{
    public Health health = new Health();
    public Weapon weapon;
    public bool canShoot = false;
    public int pointsValue = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public abstract void Move(Vector2 direction, Vector2 target);
    public virtual void Move(Vector2 direction){}
    public virtual void Move(float speed){}
    public abstract void Shoot();
    public abstract void Die();
    public abstract void Attack(float interval);
    
    public virtual float GetHealth(){
        return health.GetHealth();
    }

    public virtual void GetDamage(float damage)
    {
        health.DeductHealth(damage);
        if (health.GetHealth() <= 0)
        {
            Die();
        }
    }
    public virtual int GetPointsValue()
    {
        return pointsValue;
    }
}
