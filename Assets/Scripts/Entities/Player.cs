using UnityEngine;
using UnityEngine.Rendering;
using System;
using Unity.VisualScripting;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Player : PlayableObject
{
    [Serializable]
    private class SpecialWeaponConfig
    {
        public PlayerWeaponType weaponType = PlayerWeaponType.Laser;
        public string displayName = "Special Weapon";
        public Bullet bulletPrefab;
        public float damage = 2f;
        public float bulletSpeed = 12f;
        public float fireRate = 0.2f;
        public WeaponResourceType resourceType = WeaponResourceType.Ammo;
        public float defaultPickupAmount = 10f;
        public float maxAmmo = 5f;
        public float maxTime = 2f;
    }

    private class SpecialWeaponState
    {
        public PlayerWeaponType weaponType;
        public string displayName;
        public Bullet bulletPrefab;
        public float baseDamage;
        public float bulletSpeed;
        public float fireRate;
        public WeaponResourceType resourceType;
        public float resource;
        public float defaultPickupAmount;
        public float maxAmmo;
        public float maxTime;
        public Weapon weapon;
    }

    [SerializeField] private string nickName;
    [SerializeField] private float speed;
    [SerializeField] private Camera cam;
    [SerializeField] private float weaponDamage = 1;
    [SerializeField] private float bulletSpeed = 10;
    [SerializeField] private Bullet bulletPrefab;
    [Header("Special Weapons")]
    [SerializeField] private SpecialWeaponConfig[] specialWeaponConfigs;
    [SerializeField] private bool enableAllWeaponsAtStartForTesting = false;
    [SerializeField] private bool enableDebugRefillHotkey = false;
    [SerializeField] private float testStartAmmo = 200f;
    [SerializeField] private float testStartTime = 60f;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float nukeRange = 100;
    [SerializeField] private int nukeAvailable = 0;
    [SerializeField] private float machineGunRate = 0.01f;
    [SerializeField] private float slowness = 0.5f;
    [SerializeField] private float nukeDamage = 10f;
    [Header("Shield System")]
    [SerializeField] private int shieldAvailable = 0;
    [SerializeField] private int maxShields = 2; // Maximum shields player can have
    [SerializeField] private float shieldDuration = 2f;
    [SerializeField] private GameObject shieldEffect;
    private bool isShieldActive = false;
    private float shieldTimer = 0f;
    [Header("Experience System")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentExperience = 0;
    [SerializeField] private int experienceToNextLevel = 100;
    [SerializeField] private int aptitudePoints = 0;
    [SerializeField] private int perkPoints = 0;
    [SerializeField] private float experienceMultiplier = 1f;
    [SerializeField] private float criticalChance = 0f;
    [SerializeField] private float baseExperienceRequirement = 100f;
    [SerializeField] private float experienceScaling = 1.2f;
    [Header("Magnet System")]
    [SerializeField] private float magnetRadius = 5f;
    [SerializeField] private float magnetDuration = 3f;
    [SerializeField] private float magnetCooldown = 10f;
    private bool isMagnetActive = false;
    private float magnetTimer = 0f;
    private float magnetCooldownTimer = 0f;
    [Header("Fuel System")]
    [SerializeField] private float maxFuel = 100f;
    [SerializeField] private float currentFuel = 100f;
    [SerializeField] private float boostFuelConsumption = 30f; // fuel per second while boosting
    [SerializeField] private float boostSpeedMultiplier = 2f;
    private bool isBoosting = false;
    [Header("Laser")]
    [SerializeField] private GameObject laserSparkEffect;
    [SerializeField] private LineRenderer laserBeamRenderer;
    [SerializeField] private float laserRange = 10f;
    [SerializeField] private float laserDamageTickInterval = 0.1f;
    [SerializeField] private float laserBeamWidth = 0.06f;
    [SerializeField] private float laserBeamWidthScale = 0.5f;
    [SerializeField] private GameObject booster1Ps, booster2Ps, shockWave, inkSpots;

    public Action<float> OnHealthUpdate;
    public Action OnDeath;
    private Rigidbody2D playerRB;
    private GameManager gameManager;
    public float fireRate = 0.5f;
    public float multiplyShot = 0;
    private bool machineGunMode, inkSpotActivated = false;
    public bool triggerNuke, electrocuted, attracted = false;
    private PlayerWeaponType activeWeaponType = PlayerWeaponType.Default;
    private readonly Dictionary<PlayerWeaponType, SpecialWeaponState> specialWeapons = new Dictionary<PlayerWeaponType, SpecialWeaponState>();
    private readonly List<PlayerWeaponType> weaponOrder = new List<PlayerWeaponType> { PlayerWeaponType.Default };
    private float laserTickTimer = 0f;


    private void Awake()
    {
        health = new Health(100, 0.5f, 100);
        playerRB = GetComponent<Rigidbody2D>();

        weapon = new Weapon("PPC", weaponDamage, bulletSpeed);
        activeWeaponType = PlayerWeaponType.Default;
        
        // Validate serialized references to prevent SerializedObjectNotCreatableException
        ValidateSerializedReferences();
        
        InitializeSpecialWeapons();
        cam = Camera.main;
        OnHealthUpdate?.Invoke(health.GetHealth());
    }
    
    private void ValidateSerializedReferences()
    {
        if (bulletPrefab == null)
            Debug.LogError("Player.bulletPrefab is null! Assign it in Inspector.");
            
        if (firePoint == null)
            Debug.LogError("Player.firePoint is null! Assign it in Inspector.");
            
        if (shieldEffect == null)
            Debug.LogWarning("Player.shieldEffect is null. Shield visuals will not work.");
            
        if (laserSparkEffect == null)
            Debug.LogWarning("Player.laserSparkEffect is null. Laser visuals may not work.");
            
        if (laserBeamRenderer == null)
            Debug.LogWarning("Player.laserBeamRenderer is null. Laser beam will not render.");
            
        // Check for null entries in specialWeaponConfigs array
        if (specialWeaponConfigs != null)
        {
            for (int i = 0; i < specialWeaponConfigs.Length; i++)
            {
                if (specialWeaponConfigs[i] == null)
                {
                    Debug.LogError($"SpecialWeaponConfigs[{i}] is null! Remove or assign properly in Inspector.");
                }
                else if (specialWeaponConfigs[i].bulletPrefab == null)
                {
                    Debug.LogError($"SpecialWeaponConfigs[{i}].bulletPrefab is null for {specialWeaponConfigs[i].weaponType}! Assign in Inspector.");
                }
            }
        }
    }

    private void Start()
    {
        gameManager = GameManager.GetInstance();
        GrantAllWeaponsForTestingIfEnabled();
        InvokeRepeating(nameof(Shoot), 0, fireRate);
        GameManager.GetInstance().uIManager.UpdateAugments();
        booster1Ps.SetActive(false);
        booster2Ps.SetActive(false);
        if (laserSparkEffect != null)
            laserSparkEffect.SetActive(false);
        if (laserBeamRenderer != null)
        {
            laserBeamRenderer.enabled = false;
            laserBeamRenderer.positionCount = 2;
            laserBeamRenderer.useWorldSpace = true;
            ApplyLaserBeamWidth();
        }
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
        TickTimedWeaponResource();
        HandleLaserBeam();
        HandleShieldTimer();
        HandleMagnetTimer();
        HandleBoost();

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
        float currentSpeedMultiplier = isBoosting ? boostSpeedMultiplier : 1f;
        playerRB.linearVelocity = direction * speed * gameManager.multiplierPlayerSpeed * currentSpeedMultiplier * (electrocuted ? slowness : 1) * (attracted ? -1 : 1);

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
        if (!canShoot)
            return;

        if (activeWeaponType == PlayerWeaponType.Default)
        {
            weapon.Shoot(bulletPrefab, firePoint, new string[] { "Enemy", "Pickup", "Asteroid" }, multiplyShot);
            return;
        }

        if (!specialWeapons.TryGetValue(activeWeaponType, out SpecialWeaponState state))
        {
            SelectDefaultWeapon();
            weapon.Shoot(bulletPrefab, firePoint, new string[] { "Enemy", "Pickup", "Asteroid" }, multiplyShot);
            return;
        }

        if (state.resourceType == WeaponResourceType.Ammo && state.resource <= 0f)
        {
            SelectDefaultWeapon();
            weapon.Shoot(bulletPrefab, firePoint, new string[] { "Enemy", "Pickup", "Asteroid" }, multiplyShot);
            return;
        }

        if (activeWeaponType == PlayerWeaponType.Laser)
            return;

        if (state.weapon == null || state.bulletPrefab == null)
            return;

        float projectileSizeMultiplier = activeWeaponType == PlayerWeaponType.Rocket ? 0.5f : 1f;
        state.weapon.Shoot(state.bulletPrefab, firePoint, new string[] { "Enemy", "Pickup", "Asteroid" }, multiplyShot, 3f, projectileSizeMultiplier);

        if (state.resourceType == WeaponResourceType.Ammo)
        {
            state.resource -= 1f;
            GameManager.GetInstance().uIManager.UpdateWeaponHUD();
            if (state.resource <= 0f)
            {
                state.resource = 0f;
                SelectDefaultWeapon();
            }
        }
    }

    public override void Attack(float interval)
    {
        throw new System.NotImplementedException();
    }

    public override void GetDamage(float damage)
    {
        // Shield blocks all damage
        if (isShieldActive)
        {
            return;
        }
        
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
            RestartShootLoop(GetCurrentFireInterval());
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
                    if (!enemy.IsBoss())
                        Destroy(enemy.gameObject);
                    else
                        enemy.GetDamage(nukeDamage);
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

    public void PickupShield()
    {
        if (shieldAvailable < 2)
        {
            shieldAvailable++;
            GameManager.GetInstance().uIManager.UpdateShields(shieldAvailable);
        }
    }

    public void ActivateShield()
    {
        if (shieldAvailable > 0 && !isShieldActive)
        {
            isShieldActive = true;
            shieldTimer = shieldDuration;
            shieldAvailable--;
            
            // Activate shield visual effect
            if (shieldEffect != null)
            {
                shieldEffect.SetActive(true);
            }
            
            GameManager.GetInstance().uIManager.UpdateShields(shieldAvailable);
        }
    }

    private void HandleShieldTimer()
    {
        if (isShieldActive)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0f)
            {
                DeactivateShield();
            }
        }
    }

    private void DeactivateShield()
    {
        isShieldActive = false;
        shieldTimer = 0f;
        
        // Deactivate shield visual effect
        if (shieldEffect != null)
        {
            shieldEffect.SetActive(false);
        }
        
        // Optional: Add visual/audio feedback when shield deactivates
        // Could add a particle burst or sound effect here
    }

    private void HandleMagnetTimer()
    {
        if (isMagnetActive)
        {
            magnetTimer -= Time.deltaTime;
            if (magnetTimer <= 0f)
            {
                DeactivateMagnet();
            }
        }
        
        if (magnetCooldownTimer > 0f)
        {
            magnetCooldownTimer -= Time.deltaTime;
        }
    }

    public void ActivateMagnet()
    {
        if (magnetCooldownTimer <= 0f && !isMagnetActive)
        {
            isMagnetActive = true;
            magnetTimer = magnetDuration;
            magnetCooldownTimer = magnetCooldown;
            
            // Pull all XP pickups in radius
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, magnetRadius);
            foreach (Collider2D collider in colliders)
            {
                ExperiencePickup xpPickup = collider.GetComponent<ExperiencePickup>();
                if (xpPickup != null)
                {
                    xpPickup.StartMagnetPull(transform);
                }
            }
        }
    }

    private void DeactivateMagnet()
    {
        isMagnetActive = false;
        magnetTimer = 0f;
        
        // Stop pulling all XP pickups
        ExperiencePickup[] allXpPickups = FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None);
        foreach (ExperiencePickup xpPickup in allXpPickups)
        {
            xpPickup.StopMagnetPull();
        }
    }

    private void HandleBoost()
    {
        if (isBoosting && currentFuel > 0f)
        {
            currentFuel -= boostFuelConsumption * Time.deltaTime;
            if (currentFuel <= 0f)
            {
                currentFuel = 0f;
                StopBoost();
            }
        }
    }

    public void StartBoost()
    {
        if (currentFuel > 0f)
        {
            isBoosting = true;
        }
    }

    public void StopBoost()
    {
        isBoosting = false;
    }

    public void AddFuel(float amount)
    {
        currentFuel = Mathf.Min(currentFuel + amount, maxFuel);
    }

    public void MachineGunShoot()
    {
        if (!machineGunMode)
        {
            // Machine gun buff is for default weapon only.
            SelectDefaultWeapon();
            machineGunMode = true;
            CancelInvoke(nameof(Shoot));
            InvokeRepeating(nameof(Shoot), 0, machineGunRate);
            StartCoroutine(StopMachineGun());
            GameManager.GetInstance().uIManager.MachineGunTimer();
        }

    }

    public void Upgrade()
    {
        float newMaxHealth = health.GetMaxHealth() * gameManager.multiplierPlayerHealth;
        float newHealthRegen = health.GetRegenHealth() * gameManager.multiplierPlayerHealthRegen;
        float newHealth = health.GetMaxHealth() * gameManager.multiplierPlayerHealth - (health.GetMaxHealth() - health.GetHealth());
        health = new Health(newMaxHealth, newHealthRegen, newHealth);

        RebuildWeaponsByMultiplier();
    }

    IEnumerator StopMachineGun()
    {
        if (machineGunMode)
        {
            yield return new WaitForSeconds(5f);
            machineGunMode = false;
            GameManager.GetInstance().uIManager.MachineGunTimer();
            RestartShootLoop(GetCurrentFireInterval());
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

    private void InitializeSpecialWeapons()
    {
        specialWeapons.Clear();
        weaponOrder.Clear();
        weaponOrder.Add(PlayerWeaponType.Default);
        float playerDamageMultiplier = GameManager.GetInstance() != null ? GameManager.GetInstance().multiplierPlayerDamage : 1f;

        if (specialWeaponConfigs == null)
        {
            Debug.LogWarning("SpecialWeaponConfigs array is null!");
            return;
        }

        foreach (SpecialWeaponConfig config in specialWeaponConfigs)
        {
            if (config == null)
            {
                Debug.LogError("Found null SpecialWeaponConfig in array! This may cause SerializedObjectNotCreatableException.");
                continue;
            }
            
            if (config.weaponType == PlayerWeaponType.Default)
                continue;
                
            if (config.bulletPrefab == null)
            {
                Debug.LogError($"BulletPrefab is null for weapon {config.weaponType}! This may cause SerializedObjectNotCreatableException.");
                continue;
            }
                
            if (specialWeapons.ContainsKey(config.weaponType))
                continue;

            SpecialWeaponState state = new SpecialWeaponState
            {
                weaponType = config.weaponType,
                displayName = string.IsNullOrEmpty(config.displayName) ? config.weaponType.ToString() : config.displayName,
                bulletPrefab = config.bulletPrefab,
                baseDamage = config.damage,
                bulletSpeed = config.bulletSpeed,
                fireRate = Mathf.Max(0.03f, config.fireRate),
                resourceType = config.resourceType,
                resource = 0f,
                defaultPickupAmount = config.defaultPickupAmount <= 0f ? 1f : config.defaultPickupAmount,
                maxAmmo = config.maxAmmo <= 0f ? 5f : config.maxAmmo,
                maxTime = config.maxTime <= 0f ? 2f : config.maxTime,
                weapon = new Weapon(config.weaponType.ToString(), config.damage * playerDamageMultiplier, config.bulletSpeed)
            };

            specialWeapons.Add(config.weaponType, state);
            weaponOrder.Add(config.weaponType);
        }
    }

    private void GrantAllWeaponsForTestingIfEnabled()
    {
        if (!enableAllWeaponsAtStartForTesting)
            return;

        foreach (SpecialWeaponState state in specialWeapons.Values)
        {
            if (state.resourceType == WeaponResourceType.Ammo)
                state.resource = Mathf.Max(1f, Mathf.Min(testStartAmmo, state.maxAmmo));
            else if (state.resourceType == WeaponResourceType.Time)
                state.resource = Mathf.Max(1f, Mathf.Min(testStartTime, state.maxTime));
        }

        GameManager.GetInstance().uIManager.UpdateWeaponHUD();
    }

    public void RefillAllWeaponsForTesting()
    {
        foreach (SpecialWeaponState state in specialWeapons.Values)
        {
            if (state.resourceType == WeaponResourceType.Ammo)
                state.resource = Mathf.Max(1f, Mathf.Min(testStartAmmo, state.maxAmmo));
            else if (state.resourceType == WeaponResourceType.Time)
                state.resource = Mathf.Max(1f, Mathf.Min(testStartTime, state.maxTime));
        }

        GameManager.GetInstance().uIManager.UpdateWeaponHUD();
    }

    public bool IsDebugRefillHotkeyEnabled()
    {
        return enableDebugRefillHotkey;
    }

    private void RebuildWeaponsByMultiplier()
    {
        float playerDamageMultiplier = gameManager != null ? gameManager.multiplierPlayerDamage : 1f;
        weapon = new Weapon("Light Particule Gun", weaponDamage * playerDamageMultiplier, bulletSpeed);
        foreach (SpecialWeaponState state in specialWeapons.Values)
        {
            state.weapon = new Weapon(state.displayName, state.baseDamage * playerDamageMultiplier, state.bulletSpeed);
        }
    }

    private void TickTimedWeaponResource()
    {
        if (!canShoot)
            return;
        if (activeWeaponType == PlayerWeaponType.Default)
            return;
        if (!specialWeapons.TryGetValue(activeWeaponType, out SpecialWeaponState state))
            return;
        if (state.resourceType != WeaponResourceType.Time)
            return;

        state.resource -= Time.deltaTime;
        GameManager.GetInstance().uIManager.UpdateWeaponHUD();
        if (state.resource <= 0f)
        {
            state.resource = 0f;
            SelectDefaultWeapon();
        }
    }

    private void HandleLaserBeam()
    {
        bool laserShouldBeActive = false;
        Vector2 beamOrigin = firePoint != null ? firePoint.position : transform.position;
        Vector2 beamDirection = firePoint != null ? firePoint.up : transform.up;
        Vector2 beamEnd = beamOrigin + beamDirection * laserRange;

        if (activeWeaponType == PlayerWeaponType.Laser && canShoot &&
            specialWeapons.TryGetValue(PlayerWeaponType.Laser, out SpecialWeaponState laserState) &&
            laserState.resource > 0f)
        {
            laserShouldBeActive = true;

            if (TryGetBeamEndPoint(beamOrigin, beamDirection, out Vector2 hitPoint))
                beamEnd = hitPoint;

            laserTickTimer += Time.deltaTime;

            if (laserTickTimer >= Mathf.Max(0.02f, laserDamageTickInterval))
            {
                laserTickTimer = 0f;
                DamageWithLaser(laserState);
            }
        }
        else
        {
            laserTickTimer = 0f;
        }

        if (laserSparkEffect != null)
        {
            laserSparkEffect.transform.position = beamOrigin;
            laserSparkEffect.transform.up = beamDirection;
            laserSparkEffect.SetActive(laserShouldBeActive);
        }

        if (laserBeamRenderer != null)
        {
            ApplyLaserBeamWidth();
            laserBeamRenderer.enabled = laserShouldBeActive;
            if (laserShouldBeActive)
            {
                laserBeamRenderer.SetPosition(0, beamOrigin);
                laserBeamRenderer.SetPosition(1, beamEnd);
            }
        }
    }

    private void DamageWithLaser(SpecialWeaponState laserState)
    {
        Vector2 origin = firePoint != null ? firePoint.position : transform.position;
        Vector2 direction = firePoint != null ? firePoint.up : transform.up;
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, laserRange);
        if (hits == null || hits.Length == 0)
            return;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;
            if (hitCollider == null)
                continue;

            if (!hitCollider.CompareTag("Enemy") && !hitCollider.CompareTag("Pickup"))
                continue;

            IDamageable damageable = hitCollider.GetComponent<IDamageable>();
            if (damageable == null)
                continue;

            float scaledDamage = laserState.baseDamage * (gameManager != null ? gameManager.multiplierPlayerDamage : 1f);
            float shotMultiplier = multiplyShot > 0 ? 1 + (multiplyShot * 3) : 1;
            damageable.GetDamage(scaledDamage * shotMultiplier);

            if (damageable.GetHealth() == 0)
            {
                GameManager.GetInstance().scoreManager.IncrementScore(damageable.GetPointsValue());
            }
            return;
        }
    }

    private bool TryGetBeamEndPoint(Vector2 origin, Vector2 direction, out Vector2 endPoint)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, laserRange);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;
            if (hitCollider == null)
                continue;
            if (IsSelfCollider(hitCollider))
                continue;

            endPoint = hits[i].point;
            return true;
        }

        endPoint = origin + direction * laserRange;
        return false;
    }

    private bool IsSelfCollider(Collider2D collider)
    {
        if (collider == null)
            return false;

        Transform hitTransform = collider.transform;
        return hitTransform == transform || hitTransform.IsChildOf(transform);
    }

    private void ApplyLaserBeamWidth()
    {
        if (laserBeamRenderer == null)
            return;

        float appliedWidth = Mathf.Max(0.005f, laserBeamWidth * Mathf.Max(0.01f, laserBeamWidthScale));
        laserBeamRenderer.startWidth = appliedWidth;
        laserBeamRenderer.endWidth = appliedWidth;
        laserBeamRenderer.widthMultiplier = appliedWidth;
        laserBeamRenderer.widthCurve = AnimationCurve.Constant(0f, 1f, 1f);
    }

    public void AddWeaponResource(PlayerWeaponType weaponType, float amount)
    {
        if (weaponType == PlayerWeaponType.Default)
            return;
        if (!specialWeapons.TryGetValue(weaponType, out SpecialWeaponState state))
            return;

        float amountToAdd = amount > 0f ? amount : state.defaultPickupAmount;
        float maxCapacity = state.resourceType == WeaponResourceType.Ammo ? state.maxAmmo : state.maxTime;
        state.resource = Mathf.Min(state.resource + amountToAdd, maxCapacity);

        // Always update UI, but don't auto-switch weapons
        GameManager.GetInstance().uIManager.UpdateWeaponHUD();
    }

    public void SelectWeaponBySlot(int slotNumber)
    {
        int index = slotNumber - 1;
        if (index < 0 || index >= weaponOrder.Count)
            return;

        PlayerWeaponType wantedWeapon = weaponOrder[index];
        if (!TrySelectWeapon(wantedWeapon) && wantedWeapon != PlayerWeaponType.Default)
        {
            SelectDefaultWeapon();
        }
    }

    public void SelectNextWeapon(int direction)
    {
        if (weaponOrder.Count <= 1)
            return;

        int currentIndex = weaponOrder.IndexOf(activeWeaponType);
        if (currentIndex < 0)
            currentIndex = 0;

        int step = direction >= 0 ? 1 : -1;
        for (int i = 0; i < weaponOrder.Count; i++)
        {
            currentIndex = (currentIndex + step + weaponOrder.Count) % weaponOrder.Count;
            PlayerWeaponType candidate = weaponOrder[currentIndex];
            if (IsWeaponSelectable(candidate))
            {
                TrySelectWeapon(candidate);
                return;
            }
        }
    }

    private bool TrySelectWeapon(PlayerWeaponType weaponType)
    {
        if (!IsWeaponSelectable(weaponType))
            return false;

        activeWeaponType = weaponType;
        RestartShootLoop(GetCurrentFireInterval());
        GameManager.GetInstance().uIManager.UpdateWeaponHUD();
        return true;
    }

    private bool IsWeaponSelectable(PlayerWeaponType weaponType)
    {
        if (weaponType == PlayerWeaponType.Default)
            return true;
        if (!specialWeapons.TryGetValue(weaponType, out SpecialWeaponState state))
            return false;
        return state.resource > 0f;
    }

    private void SelectDefaultWeapon()
    {
        activeWeaponType = PlayerWeaponType.Default;
        RestartShootLoop(GetCurrentFireInterval());
        GameManager.GetInstance().uIManager.UpdateWeaponHUD();
    }

    private float GetCurrentFireInterval()
    {
        if (machineGunMode && activeWeaponType == PlayerWeaponType.Default)
            return machineGunRate;
        if (activeWeaponType == PlayerWeaponType.Default)
            return fireRate;
        if (specialWeapons.TryGetValue(activeWeaponType, out SpecialWeaponState state))
            return state.fireRate;
        return fireRate;
    }

    private void RestartShootLoop(float interval)
    {
        CancelInvoke(nameof(Shoot));
        InvokeRepeating(nameof(Shoot), 0, Mathf.Max(0.03f, interval));
    }

    public void ResetNukes()
    {
        nukeAvailable = 0;
        GameManager.GetInstance().uIManager.UpdateNukes(nukeAvailable);
    }

    public void ResetShields()
    {
        shieldAvailable = 0;
        isShieldActive = false;
        shieldTimer = 0f;
        if (shieldEffect != null)
        {
            shieldEffect.SetActive(false);
        }
        GameManager.GetInstance().uIManager.UpdateShields(shieldAvailable);
    }

    public void ResetExperience()
    {
        currentLevel = 1;
        currentExperience = 0;
        experienceToNextLevel = Mathf.RoundToInt(baseExperienceRequirement);
        aptitudePoints = 0;
        perkPoints = 0;
        GameManager.GetInstance().uIManager.UpdateLevelDisplay(currentLevel, currentExperience, experienceToNextLevel);
        GameManager.GetInstance().uIManager.UpdateAptitudePoints(aptitudePoints);
        GameManager.GetInstance().uIManager.UpdatePerkPoints(perkPoints);
    }

    public void AddExperience(int amount)
    {
        // Apply experience multiplier from Wisdom perks
        int adjustedAmount = Mathf.RoundToInt(amount * experienceMultiplier);
        currentExperience += adjustedAmount;
        
        // Check for level up
        while (currentExperience >= experienceToNextLevel)
        {
            LevelUp();
        }
        
        GameManager.GetInstance().uIManager.UpdateLevelDisplay(currentLevel, currentExperience, experienceToNextLevel);
    }

    private void LevelUp()
    {
        currentExperience -= experienceToNextLevel;
        currentLevel++;
        aptitudePoints += 1; // Gain 1 aptitude point per level
        
        // Grant 1 perk point every 5 levels
        if (currentLevel % 5 == 0)
        {
            perkPoints += 1;
            GameManager.GetInstance().uIManager.UpdatePerkPoints(perkPoints);
            Debug.Log($"Perk point gained! Total perk points: {perkPoints}");
        }
        
        // Calculate next level requirement
        experienceToNextLevel = Mathf.RoundToInt(baseExperienceRequirement * Mathf.Pow(experienceScaling, currentLevel - 1));
        
        GameManager.GetInstance().uIManager.UpdateAptitudePoints(aptitudePoints);
        GameManager.GetInstance().uIManager.ShowLevelUpNotification(currentLevel);
        
        // Optional: Add level up effects (heal player, temporary invincibility, etc.)
        HealPlayer(GetMaxHealth() * 0.5f); // Heal 50% on level up
    }

    // Experience Getters
    public int GetCurrentLevel()
    {
        return currentLevel;
    }
    
    public int GetCurrentExperience()
    {
        return currentExperience;
    }
    
    public int GetExperienceToNextLevel()
    {
        return experienceToNextLevel;
    }
    
    public int GetAptitudePoints()
    {
        return aptitudePoints;
    }
    
    public int GetPerkPoints()
    {
        return perkPoints;
    }
    
    public bool SpendAptitudePoint()
    {
        if (aptitudePoints > 0)
        {
            aptitudePoints--;
            GameManager.GetInstance().uIManager.UpdateAptitudePoints(aptitudePoints);
            return true;
        }
        return false;
    }
    
    public bool SpendPerkPoints(int amount)
    {
        if (perkPoints >= amount)
        {
            perkPoints -= amount;
            GameManager.GetInstance().uIManager.UpdatePerkPoints(perkPoints);
            return true;
        }
        return false;
    }
    
    public void IncreaseMaxHealth(float amount)
    {
        health.IncreaseMaxHealth(amount);
        OnHealthUpdate?.Invoke(health.GetHealth());
    }

    public string GetActiveWeaponName()
    {
        if (activeWeaponType == PlayerWeaponType.Default)
            return "Light Particule Gun";
        if (specialWeapons.TryGetValue(activeWeaponType, out SpecialWeaponState state))
            return state.displayName;
        return "Unknown";
    }

    public float GetActiveWeaponResource()
    {
        if (activeWeaponType == PlayerWeaponType.Default)
            return float.PositiveInfinity;
        if (specialWeapons.TryGetValue(activeWeaponType, out SpecialWeaponState state))
            return state.resource;
        return 0f;
    }

    public WeaponResourceType GetActiveWeaponResourceType()
    {
        if (activeWeaponType == PlayerWeaponType.Default)
            return WeaponResourceType.Infinite;
        if (specialWeapons.TryGetValue(activeWeaponType, out SpecialWeaponState state))
            return state.resourceType;
        return WeaponResourceType.Infinite;
    }
    
    // Fuel system getters
    public float GetCurrentFuel() { return currentFuel; }
    public float GetMaxFuel() { return maxFuel; }
    
    // Magnet system getters
    public float GetMagnetCooldownRemaining() { return magnetCooldownTimer; }
    public float GetMagnetCooldownDuration() { return magnetCooldown; }
    
    // Upgrade methods for perk system
    public void UpgradeFuelCapacity(float percentage)
    {
        float increase = maxFuel * (percentage / 100f);
        maxFuel += increase;
        currentFuel += increase; // Also increase current fuel
        Debug.Log($"Fuel capacity upgraded by {percentage}%. New max: {maxFuel}");
    }
    
    public void UpgradeMagnetRadius(float amount)
    {
        magnetRadius += amount;
        Debug.Log($"Magnet radius upgraded by {amount}. New radius: {magnetRadius}");
    }
    
    public void UpgradeMagnetDuration(float amount)
    {
        magnetDuration += amount;
        Debug.Log($"Magnet duration upgraded by {amount}s. New duration: {magnetDuration}s");
    }
    
    public void UpgradeBoostEfficiency(float percentage)
    {
        boostFuelConsumption *= (1f - percentage);
        Debug.Log($"Boost efficiency upgraded by {percentage * 100}%. New consumption rate: {boostFuelConsumption}");
    }
    
    public void UnlockWeapon(PlayerWeaponType weaponType)
    {
        if (!specialWeapons.ContainsKey(weaponType))
        {
            // Create default weapon state for unlocked weapons
            SpecialWeaponState newWeapon = new SpecialWeaponState
            {
                weaponType = weaponType,
                resourceType = GetDefaultResourceType(weaponType),
                resource = GetDefaultMaxResource(weaponType),
                maxAmmo = GetDefaultResourceType(weaponType) == WeaponResourceType.Ammo ? GetDefaultMaxResource(weaponType) : 0f,
                maxTime = GetDefaultResourceType(weaponType) == WeaponResourceType.Time ? GetDefaultMaxResource(weaponType) : 0f
            };
            
            specialWeapons[weaponType] = newWeapon;
            Debug.Log($"Weapon unlocked: {weaponType}");
        }
    }
    
    private WeaponResourceType GetDefaultResourceType(PlayerWeaponType weaponType)
    {
        switch (weaponType)
        {
            case PlayerWeaponType.Rocket:
            case PlayerWeaponType.Railgun:
                return WeaponResourceType.Ammo;
            case PlayerWeaponType.Laser:
            case PlayerWeaponType.Plasma:
                return WeaponResourceType.Time;
            default:
                return WeaponResourceType.Infinite;
        }
    }
    
    private float GetDefaultMaxResource(PlayerWeaponType weaponType)
    {
        switch (weaponType)
        {
            case PlayerWeaponType.Rocket:
                return 5f; // 5 rockets
            case PlayerWeaponType.Railgun:
                return 3f; // 3 shots
            case PlayerWeaponType.Laser:
                return 8f; // 8 seconds
            case PlayerWeaponType.Plasma:
                return 6f; // 6 seconds
            default:
                return 100f;
        }
    }
    
    public void UpgradeShieldCapacity(int amount)
    {
        maxShields = Mathf.Min(maxShields + amount, 5); // Cap at 5 shields
        Debug.Log($"Shield capacity upgraded. New max: {maxShields}");
    }
    
    public void UpgradeHealthRegen(float amount)
    {
        // Add getter/setter for health regen in Health class
        float currentRegen = health.GetRegenHealth();
        Health newHealth = new Health(health.GetMaxHealth(), currentRegen + amount, health.GetHealth());
        health = newHealth;
        Debug.Log($"Health regen upgraded by {amount}/sec. New rate: {health.GetRegenHealth()}/sec");
    }
    
    public void ToggleUpgradeMenu()
    {
        UpgradeMenu upgradeMenu = FindFirstObjectByType<UpgradeMenu>();
        if (upgradeMenu != null)
        {
            upgradeMenu.ToggleMenu();
        }
        else
        {
            Debug.LogWarning("UpgradeMenu not found in scene!");
        }
    }
    
    // Aptitude points methods
    public void AddAptitudePoints(int amount)
    {
        aptitudePoints += amount;
        GameManager.GetInstance().uIManager.UpdateAptitudePoints(aptitudePoints);
    }
    
    public void AddPerkPoints(int amount)
    {
        perkPoints += amount;
        GameManager.GetInstance().uIManager.UpdatePerkPoints(perkPoints);
    }
    
    public void SpendAptitudePoints(int amount)
    {
        if (aptitudePoints >= amount)
        {
            aptitudePoints -= amount;
            GameManager.GetInstance().uIManager.UpdateAptitudePoints(aptitudePoints);
        }
    }
    
    // Damage method for events
    public void TakeDamage(float damage)
    {
        if (!isShieldActive)
        {
            health.DeductHealth(damage);
            OnHealthUpdate?.Invoke(health.GetHealth());
            
            if (health.GetHealth() <= 0)
            {
                OnDeath?.Invoke();
            }
        }
    }
    
    // Traditional RPG-style upgrade methods
    public void UpgradeMaxHealth(float amount)
    {
        health.IncreaseMaxHealth(amount);
        OnHealthUpdate?.Invoke(health.GetHealth());
        Debug.Log($"Max health increased by {amount}. New max: {health.GetMaxHealth()}");
    }
    
    public void UpgradeHealthRegenRate(float percentage)
    {
        float currentRegen = health.GetRegenHealth();
        float newRegen = currentRegen * (1f + percentage);
        Health newHealth = new Health(health.GetMaxHealth(), newRegen, health.GetHealth());
        health = newHealth;
        Debug.Log($"Health regen increased by {percentage * 100}%. New rate: {health.GetRegenHealth()}/sec");
    }
    
    public void UpgradeMovementSpeed(float percentage)
    {
        speed *= (1f + percentage);
        Debug.Log($"Movement speed increased by {percentage * 100}%. New speed: {speed}");
    }
    
    public void UpgradeFireRate(float percentage)
    {
        fireRate *= (1f - percentage); // Lower fire rate = faster shooting
        Debug.Log($"Fire rate improved by {percentage * 100}%. New fire rate: {fireRate}");
    }
    
    public void UpgradeWeaponDamage(float percentage)
    {
        multiplyShot += percentage;
        Debug.Log($"Weapon damage increased by {percentage * 100}%. New multiplier: {1 + multiplyShot}x");
    }
    
    public void UpgradeWeaponCapacity(float percentage)
    {
        // Upgrade all weapons' ammo/time capacity
        foreach (var weaponPair in specialWeapons)
        {
            if (weaponPair.Value.resourceType == WeaponResourceType.Ammo)
            {
                weaponPair.Value.maxAmmo *= (1f + percentage);
                weaponPair.Value.resource = weaponPair.Value.maxAmmo;
            }
            else if (weaponPair.Value.resourceType == WeaponResourceType.Time)
            {
                weaponPair.Value.maxTime *= (1f + percentage);
                weaponPair.Value.resource = weaponPair.Value.maxTime;
            }
        }
        Debug.Log($"All weapons capacity increased by {percentage * 100}%");
    }
    
    public void UpgradeExperienceGain(float percentage)
    {
        experienceMultiplier = experienceMultiplier * (1f + percentage);
        Debug.Log($"Experience gain increased by {percentage * 100}%. New multiplier: {experienceMultiplier}x");
    }
    
    public void UpgradePickupRates(float percentage)
    {
        // This would affect GameManager's pickup probability
        Debug.Log($"Pickup spawn rates improved by {percentage * 100}%");
    }
    
    public void UpgradeCriticalChance(float amount)
    {
        criticalChance += amount;
        Debug.Log($"Critical chance increased by {amount * 100}%. New chance: {criticalChance * 100}%");
    }
    
    public float GetCriticalChance()
    {
        return criticalChance;
    }

    // Upgrade Methods for Max Ammo/Time
    public void UpgradeWeaponMaxAmmo(PlayerWeaponType weaponType, float increment)
    {
        if (weaponType == PlayerWeaponType.Default)
            return;
        if (!specialWeapons.TryGetValue(weaponType, out SpecialWeaponState state))
            return;
        if (state.resourceType != WeaponResourceType.Ammo)
            return;

        state.maxAmmo += increment;
        GameManager.GetInstance().uIManager.UpdateWeaponHUD();
    }

    public void UpgradeWeaponMaxTime(PlayerWeaponType weaponType, float increment)
    {
        if (weaponType == PlayerWeaponType.Default)
            return;
        if (!specialWeapons.TryGetValue(weaponType, out SpecialWeaponState state))
            return;
        if (state.resourceType != WeaponResourceType.Time)
            return;

        state.maxTime += increment;
        GameManager.GetInstance().uIManager.UpdateWeaponHUD();
    }

    // Individual Weapon Upgrade Methods
    public void UpgradeSpecificWeaponMaxAmmo(PlayerWeaponType weaponType, float increment)
    {
        if (weaponType == PlayerWeaponType.Default)
            return;
        if (!specialWeapons.TryGetValue(weaponType, out SpecialWeaponState state))
            return;
        if (state.resourceType != WeaponResourceType.Ammo)
            return;

        state.maxAmmo += increment;
        GameManager.GetInstance().uIManager.UpdateWeaponHUD();
    }

    public void UpgradeSpecificWeaponMaxTime(PlayerWeaponType weaponType, float increment)
    {
        if (weaponType == PlayerWeaponType.Default)
            return;
        if (!specialWeapons.TryGetValue(weaponType, out SpecialWeaponState state))
            return;
        if (state.resourceType != WeaponResourceType.Time)
            return;

        state.maxTime += increment;
        GameManager.GetInstance().uIManager.UpdateWeaponHUD();
    }

    // All Weapons Upgrade Methods  
    public void UpgradeAllWeaponsMaxAmmo(float increment)
    {
        foreach (SpecialWeaponState state in specialWeapons.Values)
        {
            if (state.resourceType == WeaponResourceType.Ammo)
            {
                state.maxAmmo += increment;
            }
        }
        GameManager.GetInstance().uIManager.UpdateWeaponHUD();
    }

    public void UpgradeAllWeaponsMaxTime(float increment)
    {
        foreach (SpecialWeaponState state in specialWeapons.Values)
        {
            if (state.resourceType == WeaponResourceType.Time)
            {
                state.maxTime += increment;
            }
        }
        GameManager.GetInstance().uIManager.UpdateWeaponHUD();
    }

    // Shield Upgrade Methods
    public void UpgradeShieldDuration(float increment)
    {
        shieldDuration += increment;
    }
    
    // Shield Getter Methods
    public float GetShieldDuration()
    {
        return shieldDuration;
    }
    
    public int GetShieldCount()
    {
        return shieldAvailable;
    }
    
    public bool IsShieldActive()
    {
        return isShieldActive;
    }
    
    public float GetShieldTimeRemaining()
    {
        return isShieldActive ? shieldTimer : 0f;
    }

    // Getter methods for max values
    public float GetWeaponMaxAmmo(PlayerWeaponType weaponType)
    {
        if (!specialWeapons.TryGetValue(weaponType, out SpecialWeaponState state))
            return 0f;
        return state.maxAmmo;
    }

    public float GetWeaponMaxTime(PlayerWeaponType weaponType)
    {
        if (!specialWeapons.TryGetValue(weaponType, out SpecialWeaponState state))
            return 0f;
        return state.maxTime;
    }

    public float GetActiveWeaponMaxResource()
    {
        if (activeWeaponType == PlayerWeaponType.Default)
            return float.PositiveInfinity;
        if (specialWeapons.TryGetValue(activeWeaponType, out SpecialWeaponState state))
        {
            return state.resourceType == WeaponResourceType.Ammo ? state.maxAmmo : state.maxTime;
        }
        return 0f;
    }
}
