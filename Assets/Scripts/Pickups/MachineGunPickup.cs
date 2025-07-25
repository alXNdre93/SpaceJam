using UnityEngine;

public class MachineGunpickup : Pickup, IDamageable
{
    private int pointsValue = 0;
    public override void OnPicked()
    {
        base.OnPicked();
        Player player = GameManager.GetInstance().Getplayer();
        player.MachineGunShoot();
    }

    public  void GetDamage(float damage)
    {
        OnPicked();
    }
    public float GetHealth(){return 0;}

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
