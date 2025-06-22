using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float damage;

    private string[] targetTag;

    public void SetBullet(float _damage, string[] _targetTag, float _speed = 10f)
    {
        damage = _damage;
        targetTag = _targetTag;
        speed = _speed;
    }

    private void Update()
    {
        Move();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool rightTarget = false;
        if (collision.CompareTag("Border")){
            Destroy(gameObject);
            return;
        }
        foreach(string tag in targetTag){
            if (collision.CompareTag(tag) && rightTarget == false){
                rightTarget = true;
            } 
        }
        if (!rightTarget)
            return;
        
        IDamageable damageable = collision.GetComponent<IDamageable>();
        Damage(damageable);
    }

    void Damage(IDamageable damageable)
    {
        if (damageable != null){
            damageable.GetDamage(damage);

            GameManager.GetInstance().scoreManager.IncrementScore(damageable.GetPointsValue());

            Destroy(gameObject);
        }
    }

    private void Move()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

}
