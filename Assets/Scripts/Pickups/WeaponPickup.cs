using UnityEngine;

public class WeaponPickup : Pickup, IDamageable
{
    [SerializeField] private PlayerWeaponType weaponType = PlayerWeaponType.Laser;
    [SerializeField] private float resourceAmount = 0f;
    private int pointsValue = 0;

    public override void OnPicked()
    {
        base.OnPicked();
        Player player = GameManager.GetInstance().Getplayer();
        if (player != null)
        {
            player.AddWeaponResource(weaponType, resourceAmount);
        }
    }

    public void GetDamage(float damage)
    {
        OnPicked();
    }

    public float GetHealth(){return 0;}

    public int GetPointsValue()
    {
        return pointsValue;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnPicked();
        }
    }
}
