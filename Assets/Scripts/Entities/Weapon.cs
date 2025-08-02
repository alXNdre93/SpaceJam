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

    public void Shoot(Bullet _bullet, Transform _firePoint, string[] _targetTag, float _multiplyShot, float _timeToLive = 3f)
    {
        multiplyShot = _multiplyShot;
        Bullet tempBullet = GameObject.Instantiate(_bullet, _firePoint.position, _firePoint.rotation);
        tempBullet.gameObject.transform.localScale += new Vector3(_multiplyShot > 2 ? 2 : multiplyShot,_multiplyShot > 2 ? 2 : multiplyShot,0);
        tempBullet.SetBullet(damage * (_multiplyShot > 0 ? 1 + (_multiplyShot * 3) : 1), _targetTag, bulletSpeed);
        if (tempBullet.GetComponent<Projectile>() != null)
            tempBullet.GetComponent<Projectile>().firing_ship = _firePoint.parent.gameObject;
        GameObject.Destroy(tempBullet.gameObject, _timeToLive);
    }

    public float GetDamage()
    {
        return damage * (multiplyShot > 0 ? 1 + (multiplyShot * 3) : 1);
    }
    
}
