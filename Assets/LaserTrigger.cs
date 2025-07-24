using Unity.VisualScripting;
using UnityEngine;

public class LaserTrigger : MonoBehaviour
{
    [SerializeField] PlayableObject itself;
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.GetComponent<IDamageable>().GetDamage(itself.weapon.GetDamage()*Time.deltaTime);
        }
    }
}
