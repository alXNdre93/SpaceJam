using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

public class Weapon
{
    private string name;
    private float damage = 1f;
    private float bulletSpeed;

    public Weapon( string _name, float _damage, float _bulletSpeed)
    {
        name = _name;
        damage = _damage;
        bulletSpeed = _bulletSpeed;
    }

    public Weapon(){}

    public void Shoot(Bullet _bullet, Transform _firePoint, string[] _targetTag, float multiplyShot, float _timeToLive = 3f)
    {
        Bullet tempBullet = GameObject.Instantiate(_bullet, _firePoint.position, _firePoint.rotation);
        tempBullet.gameObject.transform.localScale += new Vector3(multiplyShot,multiplyShot,0);
        tempBullet.SetBullet(damage, _targetTag, bulletSpeed);
        GameObject.Destroy(tempBullet.gameObject, _timeToLive);
    }

    public float GetDamage()
    {
        return damage;
    }
    
}
