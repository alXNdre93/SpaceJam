using UnityEngine;
using UnityEngine.Rendering;
using System;
using Unity.VisualScripting;
using UnityEngine.UI;
using System.Collections;

public class Player : PlayableObject
{
    [SerializeField] private string nickName;
    [SerializeField] private float speed;
    [SerializeField] private Camera cam;
    [SerializeField] private float weaponDamage = 1;
    [SerializeField] private float bulletSpeed = 10;
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float nukeRange = 100;
    [SerializeField] private int nukeAvailable = 0;
    [SerializeField] private float machineGunRate = 0.01f;
    [SerializeField] private float slowness = 0.5f;
    [SerializeField] private GameObject booster1Ps, booster2Ps, shockWave, inkSpots;

    public Action<float> OnHealthUpdate;
    public Action OnDeath;
    private Rigidbody2D playerRB;
    public float fireRate = 0.5f;
    public float multiplyShot = 0;
    private bool machineGunMode, inkSpotActivated = false;
    public bool triggerNuke, electrocuted = false;


    private void Awake()
    {
        health = new Health(100, 0.5f, 100);
        playerRB = GetComponent<Rigidbody2D>();

        weapon = new Weapon("Player Weapon", weaponDamage, bulletSpeed);
        cam = Camera.main;
        OnHealthUpdate?.Invoke(health.GetHealth());
    }

    private void Start()
    {
        InvokeRepeating(nameof(Shoot), 0, fireRate);
        GameManager.GetInstance().uIManager.UpdateAugments();
        booster1Ps.SetActive(false);
        booster2Ps.SetActive(false);
        RectTransform[] allRect = FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (RectTransform rect in allRect)
        {
            if (rect.gameObject.CompareTag("InkSpots"))
            {
                inkSpots = rect.gameObject;
                return;
            }
        }
    }

    private void Update()
    {
        health.RegenHealth();
        OnHealthUpdate?.Invoke(health.GetHealth());
        BoosterControl();

        if (electrocuted)
            StartCoroutine(ResetElectrocuted());
    }

    private void BoosterControl()
    {
        if (GameManager.GetInstance().isMoving)
        {
            booster1Ps.SetActive(true);
            booster2Ps.SetActive(true);
        }
        else
        {
            booster1Ps.SetActive(false);
            booster2Ps.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Border"))
        {
            Vector3 newPos = transform.position;
            newPos = collision.ClosestPoint(newPos);

            Vector3 collisionDir = transform.position - newPos;
            collisionDir.Normalize();
            collisionDir *= 1.1f;
            collisionDir.x *= GetComponent<Collider2D>().bounds.extents.x;
            collisionDir.y *= GetComponent<Collider2D>().bounds.extents.y;
            collisionDir.z *= GetComponent<Collider2D>().bounds.extents.z;

            transform.position = newPos + collisionDir;
        }
    }

    public override void Move(Vector2 direction, Vector2 target)
    {
        playerRB.linearVelocity = direction * speed * (electrocuted ? slowness : 1);

        Vector3 playerScreenPos = cam.WorldToScreenPoint(transform.position);
        target.x -= playerScreenPos.x;
        target.y -= playerScreenPos.y;

        float angle = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, (angle) - 90);
    }

    public override void Die()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }

    public override void Shoot()
    {
        if (canShoot)
            weapon.Shoot(bulletPrefab, firePoint, new string[] { "Enemy", "Pickup" }, multiplyShot);
    }

    public override void Attack(float interval)
    {
        throw new System.NotImplementedException();
    }

    public override void GetDamage(float damage)
    {
        health.DeductHealth(damage);

        OnHealthUpdate?.Invoke(health.GetHealth());

        if (health.GetHealth() <= 0)
            Die();
    }

    public void HealPlayer(float healthToAdd)
    {
        health.AddHealth(healthToAdd);
        OnHealthUpdate?.Invoke(health.GetHealth());
    }

    public void IncrementFireRate(float fireRateIncrement)
    {
        if (!machineGunMode)
            CancelInvoke(nameof(Shoot));
        fireRate -= fireRateIncrement;
        if (fireRate < 0.05f)
            fireRate = 0.05f;
        if (!machineGunMode)
            InvokeRepeating(nameof(Shoot), 0, fireRate);
        GameManager.GetInstance().uIManager.UpdateAugments();
    }

    public void MultiplyShot(float _multiplyShot)
    {
        multiplyShot += _multiplyShot;
        GameManager.GetInstance().uIManager.UpdateAugments();
    }
    public override int GetPointsValue()
    {
        return base.GetPointsValue();
    }

    public void PickupNuke()
    {
        if (nukeAvailable < 3)
        {
            nukeAvailable++;
            GameManager.GetInstance().uIManager.UpdateNukes(nukeAvailable);
        }
    }

    public float GetMaxHealth()
    {
        return health.GetMaxHealth();
    }

    public void Blind()
    {
        if (!inkSpotActivated)
            StartCoroutine(InkSpotController());
    }

    public void ActivateNuke()
    {
        if (nukeAvailable > 0)
        {
            Instantiate(shockWave, transform.position, Quaternion.identity);
            var allenemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            var allpickups = FindObjectsByType<Pickup>(FindObjectsSortMode.None);
            foreach (Enemy enemy in allenemies)
            {
                if (Vector2.Distance(transform.position, enemy.transform.position) < nukeRange)
                {
                    Destroy(enemy.gameObject);
                }
            }
            foreach (Pickup pickup in allpickups)
            {
                if (Vector2.Distance(transform.position, pickup.transform.position) < nukeRange)
                {
                    Destroy(pickup.gameObject);
                }
            }
            nukeAvailable--;
            GameManager.GetInstance().uIManager.UpdateNukes(nukeAvailable);
        }
    }

    public void MachineGunShoot()
    {
        if (!machineGunMode)
        {
            machineGunMode = true;
            CancelInvoke(nameof(Shoot));
            InvokeRepeating(nameof(Shoot), 0, machineGunRate);
            StartCoroutine(StopMachineGun());
            GameManager.GetInstance().uIManager.MachineGunTimer();
        }

    }

    IEnumerator StopMachineGun()
    {
        if (machineGunMode)
        {
            yield return new WaitForSeconds(5f);
            GameManager.GetInstance().uIManager.MachineGunTimer();
            CancelInvoke(nameof(Shoot));
            InvokeRepeating(nameof(Shoot), 0, fireRate);
            machineGunMode = false;
        }
    }

    IEnumerator ResetElectrocuted()
    {
        yield return new WaitForSeconds(2);
        electrocuted = false;
    }

    IEnumerator InkSpotController()
    {
        inkSpots.SetActive(true);
        inkSpotActivated = true;
        yield return new WaitForSeconds(5);
        inkSpots.SetActive(false);
        inkSpotActivated = false;
    }
}
