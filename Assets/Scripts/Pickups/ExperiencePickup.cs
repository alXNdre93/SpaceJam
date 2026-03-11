using UnityEngine;

public class ExperiencePickup : Pickup, IDamageable
{
    [SerializeField] private int experienceValue;
    private bool isBeingMagneted = false;
    private Transform playerTransform;
    private float magnetPullSpeed = 8f;

    public void Initialize(int expValue)
    {
        experienceValue = expValue;
    }

    public override void OnPicked()
    {
        base.OnPicked();
        Player player = GameManager.GetInstance().Getplayer();
        if (player != null)
        {
            player.AddExperience(experienceValue);
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
        return 0; // XP pickups don't give points
    }

    private void Update()
    {
        if (isBeingMagneted && playerTransform != null)
        {
            // Move towards player
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, magnetPullSpeed * Time.deltaTime);
        }
    }

    public void StartMagnetPull(Transform player)
    {
        isBeingMagneted = true;
        playerTransform = player;
    }

    public void StopMagnetPull()
    {
        isBeingMagneted = false;
        playerTransform = null;
    }
}