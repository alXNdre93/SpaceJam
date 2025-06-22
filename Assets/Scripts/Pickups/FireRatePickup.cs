using UnityEngine;

public class FireRatePickup : Pickup, IDamageable
{
    [SerializeField] float fireRateIncrement = 0.01f;
    private int pointsValue = 0;

    public override void OnPicked()
    {
        base.OnPicked();
        Player player = GameManager.GetInstance().Getplayer();
        player.IncrementFireRate(fireRateIncrement);
    }

    public  void GetDamage(float damage)
    {
        OnPicked();
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")){
            OnPicked();
        }
    }

    public int GetPointsValue()
    {
        return pointsValue;
    }
}
