using UnityEngine;

public class FuelPickup : Pickup, IDamageable
{
    [SerializeField] private float fuelAmount = 25f;

    public override void OnPicked()
    {
        base.OnPicked();
        Player player = GameManager.GetInstance().Getplayer();
        if (player != null)
        {
            player.AddFuel(fuelAmount);
        }
    }

    public void GetDamage(float damage)
    {
        OnPicked();
    }
    
    public float GetHealth() { return 0; }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnPicked();
        }
    }

    public int GetPointsValue()
    {
        return 0;
    }
}