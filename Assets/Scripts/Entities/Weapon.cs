using UnityEngine;
using UnityEngine.UIElements;

public class Weapon
{
    private string name;
    private float damage = 1f;
    private float bulletSpeed;
    private float multiplyShot = 0;

    public Weapon(string _name, float _damage, float _bulletSpeed)
    {
        name = _name;
        damage = _damage;
        bulletSpeed = _bulletSpeed;
    }

    public Weapon(){}

    public void Shoot(Bullet _bullet, Transform _firePoint, string[] _targetTag, float _multiplyShot, float _timeToLive = 3f, float projectileSizeMultiplier = 1f)
    {
        multiplyShot = _multiplyShot;
        
        // Use pool manager if available, otherwise fallback to instantiate
        Bullet tempBullet;
        if (PoolManager.Instance != null)
        {
            tempBullet = PoolManager.Instance.GetBullet(_bullet);
            if (tempBullet != null)
            {
                tempBullet.transform.position = _firePoint.position;
                tempBullet.transform.rotation = _firePoint.rotation;
            }
            else
            {
                // Fallback to instantiate if pool is empty or unavailable
                tempBullet = GameObject.Instantiate(_bullet, _firePoint.position, _firePoint.rotation);
                Debug.LogWarning($"Bullet pool for '{_bullet.name}' returned null, falling back to Instantiate");
            }
        }
        else
        {
            // Fallback if PoolManager doesn't exist
            tempBullet = GameObject.Instantiate(_bullet, _firePoint.position, _firePoint.rotation);
            Debug.LogWarning("PoolManager not found, falling back to Instantiate");
        }
        
        tempBullet.gameObject.transform.localScale += new Vector3(_multiplyShot > 2 ? 2 : multiplyShot,_multiplyShot > 2 ? 2 : multiplyShot,0);
        if (projectileSizeMultiplier != 1f)
        {
            tempBullet.gameObject.transform.localScale *= projectileSizeMultiplier;
        }
        tempBullet.SetBullet(damage * (_multiplyShot > 0 ? 1 + (_multiplyShot * 3) : 1), _targetTag, bulletSpeed);
        if (tempBullet.GetComponent<Projectile>() != null)
            tempBullet.GetComponent<Projectile>().firing_ship = _firePoint.parent.gameObject;
            
        // Note: No need for GameObject.Destroy with timer as bullets now handle their own lifetime through IPoolable
    }

    public float GetDamage()
    {
        return damage * (multiplyShot > 0 ? 1 + (multiplyShot * 3) : 1);
    }
    
}
