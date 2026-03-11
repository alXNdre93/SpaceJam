using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour
{
    [Header("Game Entities")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Game Variables")]
    [SerializeField] private float enemySpawnRate;

    public Action OnGameStart;
    public Action OnGameOver;

    public ScoreManager scoreManager;
    public PickupManager pickupManager;
    public UIManager uIManager;
    [SerializeField] private UpgradeCardManager upgradeCardManager;
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private PerkSystem perkSystem;

    private Player player;
    private GameObject tempEnemy;
    private bool isEnemySpawning;
    private bool isPlaying;
    private bool isEndlessMode = false;
    private int currentLimit = 4;

    private Weapon meleeWeapon = new Weapon("Melee", 1, 0);
    private Weapon machineGunWeapon = new Weapon("Machine Gun", 2, 10);
    private Weapon sniperWeapon = new Weapon("Sniper", 5, 15);
    private Weapon explosionWeapon = new Weapon("Explosion", 20, 0);
    private Weapon electricWeapon = new Weapon("Electric", 2, 0);
    private Weapon spikeThrow = new Weapon("Spike", 2, 15);
    private Weapon laserWeapon = new Weapon("Laser", 3, 0);
    private Weapon blinderWeapon = new Weapon("Blinder", 0, 0);
    private Weapon absorbsWeapon = new Weapon("Absorbs", 4, 0);

    private int bossEvent = 250; 
    public float multiplierEnemyHealth = 1;
    public float multiplierPlayerHealth = 1;
    public float mulitplierSpawnRate = 1;
    public float multiplierPoint = 1;
    public float multiplierEnemyDamage = 1;
    public float multiplierPlayerDamage = 1;
    public float multiplierEnemySpeed = 1;
    public float multiplierPlayerSpeed = 1;
    public float multiplierPlayerHealthRegen = 1;


    //Singleton Start
    private static GameManager instance;
    public bool isMoving;

    public static GameManager GetInstance()
    {
        return instance;
    }

    void SetSingleton()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        instance = this;
    }

    void Awake()
    {
        SetSingleton();
    }

    //Singleton End

    public void NotifyDeath(Enemy enemy)
    {
        pickupManager.SpawnPickup(enemy.transform.position);
        
        // Spawn experience pickup instead of directly giving experience
        if (player != null)
        {
            int experienceGain = CalculateExperienceGain(enemy);
            pickupManager.SpawnExperiencePickup(enemy.transform.position, experienceGain);
        }
    }

    private int CalculateExperienceGain(Enemy enemy)
    {
        // Base experience from enemy's point value
        int baseExp = Mathf.Max(1, enemy.GetPointsValue() / 10);
        
        // Boss enemies give bonus experience
        if (enemy.IsBoss())
        {
            baseExp *= 3;
        }
        
        // Scale with multipliers (higher difficulty = more exp)
        baseExp = Mathf.RoundToInt(baseExp * multiplierEnemyHealth);
        
        return baseExp;
    }

    public Player Getplayer()
    {
        return player;
    }

    public bool IsPlaying()
    {
        return isPlaying;
    }

    public void StartGame(bool endlessMode)
    {
        player = Instantiate(playerPrefab, Vector2.zero, Quaternion.identity).GetComponent<Player>();
        player.ResetNukes();
        player.ResetShields();
        player.ResetExperience();
        uIManager.UpdateHealth(player.health.GetHealth());
        player.OnDeath += StopGame;
        scoreManager.onScoreChange += CheckPointsForEvents;
        isPlaying = true;
        isEndlessMode = endlessMode;
        if (isEndlessMode)
            currentLimit = enemyPrefabs.Length;
        OnGameStart?.Invoke();
        ResetValues();
        StartCoroutine(GameStarter());
    }

    void ResetValues()
    {
        bossEvent = 250; 
        multiplierEnemyHealth = 1;
        multiplierPlayerHealth = 1;
        mulitplierSpawnRate = 1;
        multiplierPoint = 1;
        multiplierEnemyDamage = 1;
        multiplierPlayerDamage = 1;
        multiplierEnemySpeed = 1;
        multiplierPlayerSpeed = 1;
        multiplierPlayerHealthRegen = 1;
    }

    IEnumerator GameStarter()
    {
        yield return new WaitForSeconds(0.5f);
        player.HealPlayer(player.GetMaxHealth());
        isEnemySpawning = true;
        StartCoroutine(EnemySpawner());
    }

    IEnumerator SpawnEnemy()
    {
        yield return new WaitForSeconds(0.5f);
        isEnemySpawning = true;
        StartCoroutine(EnemySpawner());
    }

    private bool isBossAlive = false;
    private bool isCardSelectionActive = false;
    private float lastBossDeathTime = 0f;
    private float bossSpawnCooldown = 3f; // Prevent boss spawns for 3 seconds after last boss death

    void SpawnBoss()
    {
        if (isBossAlive) 
        {
            Debug.LogWarning("Attempted to spawn boss but one is already alive!");
            return; // Prevent multiple bosses
        }
        
        // Check cooldown to prevent immediate respawning
        if (Time.time - lastBossDeathTime < bossSpawnCooldown)
        {
            Debug.LogWarning($"Boss spawn on cooldown. Time remaining: {bossSpawnCooldown - (Time.time - lastBossDeathTime):F1}s");
            return;
        }
        
        // Don't spawn boss during card selection
        if (isCardSelectionActive)
        {
            Debug.LogWarning("Cannot spawn boss during card selection");
            return;
        }
        
        isBossAlive = true;
        Debug.Log($"SpawnBoss called - bossEvent threshold: {bossEvent}");
        
        if (!isEndlessMode)
        {
            // Use pool manager if available, otherwise fallback to instantiate
            if (PoolManager.Instance != null)
            {
                tempEnemy = PoolManager.Instance.GetEnemy(enemyPrefabs[currentLimit]).gameObject;
            }
            else
            {
                tempEnemy = Instantiate(enemyPrefabs[currentLimit]);
                Debug.LogWarning("PoolManager not found, falling back to Instantiate for boss");
            }
            
            tempEnemy.transform.position = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)].position;
            if (!isEndlessMode)
                tempEnemy.GetComponent<Enemy>().OnBossDeath += UpCurrentLimit;
            // Also clear boss flag when boss dies
            tempEnemy.GetComponent<Enemy>().OnBossDeath += BossDied;
            tempEnemy.GetComponent<Enemy>().OnBossDeath += GiveBossReward;
            SetEnemy(true);
            BossSpawnPrep();
        }
        else
        {
            CreateEnemy(true);
            // Subscribe to boss events for endless mode
            tempEnemy.GetComponent<Enemy>().OnBossDeath += BossDied;
            tempEnemy.GetComponent<Enemy>().OnBossDeath += GiveBossReward;
        }
    }

    private void BossDied()
    {
        Debug.Log("BossDied() called - resetting isBossAlive flag");
        isBossAlive = false;
        lastBossDeathTime = Time.time;
    }
    
    private void GiveBossReward()
    {
        Player player = Getplayer();
        if (player != null)
        {
            // Award 1 perk point for killing a boss
            player.AddPerkPoints(1);
            Debug.Log("Boss defeated! Awarded 1 perk point!");
            
            // Optional: Show special notification for boss kill
            uIManager.ShowBossKillNotification();
        }
    }

    void WeaponUpdate(){
        meleeWeapon = new Weapon("Melee", 1*multiplierEnemyDamage, 0);
        machineGunWeapon = new Weapon("Machine Gun", 2*multiplierEnemyDamage, 10);
        sniperWeapon = new Weapon("Sniper", 5*multiplierEnemyDamage, 15);
        explosionWeapon = new Weapon("Explosion", 20*multiplierEnemyDamage, 0);
        electricWeapon = new Weapon("Electric", 2*multiplierEnemyDamage, 0);
        spikeThrow = new Weapon("Spike", 10*multiplierEnemyDamage, 15);
        laserWeapon = new Weapon("Laser", 3*multiplierEnemyDamage, 0);
        absorbsWeapon = new Weapon("Absorbs", 4*multiplierEnemyDamage, 0);
    }

    void UpCurrentLimit(){
        uIManager.ToggleBossHealth();
        if (!isEndlessMode)
            if (currentLimit < enemyPrefabs.Length - 1)
                currentLimit++;
            else
                NormalModeOver();
        CardSelection();
        UpgradePlayer();
        UpgradeEnemy();
        WeaponUpdate();
    }

    void BossSpawnPrep() {
        uIManager.ToggleBossHealth();
        uIManager.UpdateBossSprite(tempEnemy.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite);
        uIManager.SetBossMaxHealth(tempEnemy.GetComponent<Enemy>().GetMaxHealth());
        uIManager.BossSpawned(tempEnemy.GetComponent<Enemy>());
    }

    void NormalModeOver(){
        uIManager.ShowNormalModeOver();
    }

    void CardSelection(){
        if (upgradeCardManager != null)
        {
            // Ensure GameManager stays active so coroutines can run during card selection
            gameObject.SetActive(true);
            enabled = true;
            isCardSelectionActive = true;
            
            Debug.Log("GameManager.CardSelection(): triggering card selection - showing player cards");
            // show player choices first, then enemy choices
            upgradeCardManager.ShowCards(true, () => {
                Debug.Log("GameManager.CardSelection(): player made selection - showing enemy cards");
                // after player selection, show enemy upgrade choices
                upgradeCardManager.ShowCards(false, () => {
                    // Card selection complete
                    isCardSelectionActive = false;
                    Debug.Log("GameManager.CardSelection(): card selection complete");
                });
            });
        }
    }


    public void ApplyPlayerUpgrade(UpgradeCardType type, float value)
    {
        switch (type)
        {
            case UpgradeCardType.Speed:
                if (value > 0){
                    multiplierPlayerSpeed += value;
                }else{
                    multiplierEnemySpeed += value;
                }
                break;
            case UpgradeCardType.Damage:
                if (value > 0){
                    multiplierPlayerDamage += value;
                }else{
                    multiplierEnemyDamage += value;
                }
                
                break;
            case UpgradeCardType.Point:
                multiplierPoint += value;
                break;
            case UpgradeCardType.Spawn:
                mulitplierSpawnRate -= value;
                break;
            case UpgradeCardType.Health:
            if (value > 0){
                    multiplierPlayerHealth += value;
                }else{
                    multiplierEnemyHealth += value;
                }
                break;
            case UpgradeCardType.MaxAmmo:
                if (player != null)
                {
                    // Upgrade all ammo-based weapons by the value (convert percentage to actual amount)
                    float increment = Mathf.Max(1f, value * 5f); // Scale percentage to meaningful amount
                    player.UpgradeAllWeaponsMaxAmmo(increment);
                }
                break;
            case UpgradeCardType.MaxTime:
                if (player != null)
                {
                    // Upgrade all time-based weapons by the value (convert percentage to seconds)
                    float increment = Mathf.Max(0.5f, value * 2f); // Scale percentage to meaningful amount
                    player.UpgradeAllWeaponsMaxTime(increment);
                }
                break;
            case UpgradeCardType.LaserMaxTime:
                if (player != null)
                {
                    float increment = Mathf.Max(0.5f, value * 1.5f);
                    player.UpgradeSpecificWeaponMaxTime(PlayerWeaponType.Laser, increment);
                }
                break;
            case UpgradeCardType.RocketMaxAmmo:
                if (player != null)
                {
                    float increment = Mathf.Max(1f, value * 3f);
                    player.UpgradeSpecificWeaponMaxAmmo(PlayerWeaponType.Rocket, increment);
                }
                break;
            case UpgradeCardType.PlasmaMaxTime:
                if (player != null)
                {
                    float increment = Mathf.Max(0.5f, value * 1.5f);
                    player.UpgradeSpecificWeaponMaxTime(PlayerWeaponType.Plasma, increment);
                }
                break;
            case UpgradeCardType.RailgunMaxAmmo:
                if (player != null)
                {
                    float increment = Mathf.Max(1f, value * 3f);
                    player.UpgradeSpecificWeaponMaxAmmo(PlayerWeaponType.Railgun, increment);
                }
                break;
            case UpgradeCardType.ShieldDuration:
                if (player != null)
                {
                    float increment = Mathf.Max(0.2f, value * 0.8f); // Scale to reasonable duration increase
                    player.UpgradeShieldDuration(increment);
                }
                break;
            default:
                break;
        }
    }

    public void ApplyEnemyUpgrade(UpgradeCardType type, float value)
    {
        switch (type)
        {
            case UpgradeCardType.Speed:
                if (value > 0){
                    multiplierEnemySpeed += value;
                }else{
                    multiplierPlayerSpeed += value;
                }
                break;
            case UpgradeCardType.Damage:
                if (value > 0){
                    multiplierEnemyDamage += value;
                }else{
                    multiplierPlayerDamage += value;
                }
                break;
            case UpgradeCardType.Point:
                multiplierPoint -= value;
                break;
            case UpgradeCardType.Spawn:
                mulitplierSpawnRate += value;
                break;
            case UpgradeCardType.Health:
            if (value > 0){
                    multiplierEnemyHealth += value;
                }else{
                    multiplierPlayerHealth += value;
                }
                break;
            default:
                break;
        }
    }
    void UpgradePlayer(){
        player.Upgrade();
    }

    void UpgradeEnemy() {
        mulitplierSpawnRate += 0.05f;
        multiplierEnemyDamage += 0.1f;
        multiplierEnemyHealth += 0.1f;
        multiplierEnemySpeed += 0.1f;
    }

    void CheckPointsForEvents(float score)
    {
        // Don't trigger boss events during card selection or while boss is alive or on cooldown
        if (isCardSelectionActive || isBossAlive || (Time.time - lastBossDeathTime < bossSpawnCooldown))
        {
            if (score >= bossEvent)
            {
                Debug.Log($"Boss event deferred - score {score} >= threshold {bossEvent} but conditions not met (cardSelection: {isCardSelectionActive}, bossAlive: {isBossAlive}, cooldown: {Time.time - lastBossDeathTime < bossSpawnCooldown})");
            }
            return;
        }
        
        if (score >= bossEvent){
            Debug.Log($"Boss event triggered at score {score} (threshold: {bossEvent})");
            SpawnBoss();
            bossEvent = bossEvent*2;
        }else{
            // Use event system for random events
            if (eventSystem != null && UnityEngine.Random.Range(0,100) > 95){
                eventSystem.TriggerRandomEvent();
                Debug.Log($"Random Event Triggered at score {score}!");
            }
        }
    }
    
    public void UpgradeSpawnRateReduction(float percentage)
    {
        mulitplierSpawnRate *= (1f - percentage);
        Debug.Log($"Spawn rate reduced by {percentage * 100}%. New multiplier: {mulitplierSpawnRate}");
    }
    
    public EventSystem GetEventSystem()
    {
        return eventSystem;
    }
    
    public PerkSystem GetPerkSystem()
    {
        return perkSystem;
    }

    public void StopGame()
    {
        isEnemySpawning = false;
        isPlaying = false; // Stop game immediately to prevent further updates
        scoreManager.SetHighScore();
        
        // Immediately clean up enemies and pickups to prevent null reference issues
        var allenemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        var allpickups = FindObjectsByType<Pickup>(FindObjectsSortMode.None);
        
        foreach (Enemy enemy in allenemies)
        {
            if (enemy != null)
            {
                // Use pooling system if available
                if (PoolManager.Instance != null)
                {
                    PoolManager.Instance.ReturnEnemy(enemy);
                }
                else
                {
                    Destroy(enemy.gameObject);
                }
            }
        }
        
        foreach (Pickup pickup in allpickups)
        {
            if (pickup != null)
            {
                // Use pooling system if available
                if (PoolManager.Instance != null)
                {
                    PoolManager.Instance.ReturnPickup(pickup);
                }
                else
                {
                    Destroy(pickup.gameObject);
                }
            }
        }
        
        StartCoroutine(GameStopper());
    }

    IEnumerator GameStopper()
    {
        // Game is already stopped and enemies/pickups cleaned up in StopGame()
        // Just need a small delay before invoking game over event
        yield return new WaitForSeconds(0.5f);
        OnGameOver?.Invoke();
    }

    void CreateEnemy(bool isBoss = false)
    {
        // Use pool manager if available, otherwise fallback to instantiate
        if (PoolManager.Instance != null)
        {
            Enemy enemyComponent = PoolManager.Instance.GetEnemy(enemyPrefabs[UnityEngine.Random.Range(0, currentLimit)]);
            if (enemyComponent != null)
            {
                tempEnemy = enemyComponent.gameObject;
            }
            else
            {
                tempEnemy = Instantiate(enemyPrefabs[UnityEngine.Random.Range(0, currentLimit)]);
                Debug.LogWarning("Enemy pool returned null, falling back to Instantiate");
            }
        }
        else
        {
            tempEnemy = Instantiate(enemyPrefabs[UnityEngine.Random.Range(0, currentLimit)]);
            Debug.LogWarning("PoolManager not found, falling back to Instantiate for enemy");
        }
        
        tempEnemy.transform.position = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)].position;
        if (isBoss)
        {
            BossSpawnPrep();
            // Only subscribe to boss death events here for bosses created via CreateEnemy
            // Don't duplicate the subscriptions that are already in SpawnBoss()
            Debug.Log("Boss created via CreateEnemy - events already subscribed in SpawnBoss");
        }
        SetEnemy(isBoss);
    }

    void SetEnemy(bool isBoss = false){
        if (tempEnemy.GetComponent<MeleeEnemy>() != null)
        {
            tempEnemy.GetComponent<MeleeEnemy>().weapon = meleeWeapon;
            tempEnemy.GetComponent<MeleeEnemy>().SetEnemy(1, 0.25f, isBoss);
        }
        else if (tempEnemy.GetComponent<ExploderEnemy>() != null)
        {
            tempEnemy.GetComponent<ExploderEnemy>().weapon = explosionWeapon;
            tempEnemy.GetComponent<ExploderEnemy>().SetEnemy(1, 4f, isBoss);
        }
        else if (tempEnemy.GetComponent<MachineGunEnemy>() != null)
        {
            tempEnemy.GetComponent<MachineGunEnemy>().weapon = machineGunWeapon;
            tempEnemy.GetComponent<MachineGunEnemy>().SetEnemy(5, 0.3f, isBoss);
        }
        else if (tempEnemy.GetComponent<ShooterEnemy>() != null)
        {
            tempEnemy.GetComponent<ShooterEnemy>().weapon = sniperWeapon;
            tempEnemy.GetComponent<ShooterEnemy>().SetEnemy(8, 2f, isBoss);
        }
        else if (tempEnemy.GetComponent<ElectricEnemy>() != null)
        {
            tempEnemy.GetComponent<ElectricEnemy>().weapon = electricWeapon;
            tempEnemy.GetComponent<ElectricEnemy>().SetEnemy(2, 2f, isBoss);
        }
        else if (tempEnemy.GetComponent<SpikeEnemy>() != null)
        {
            tempEnemy.GetComponent<SpikeEnemy>().weapon = spikeThrow;
            tempEnemy.GetComponent<SpikeEnemy>().SetEnemy(10, 2f, isBoss);
        }
        else if (tempEnemy.GetComponent<LaserEnemy>() != null)
        {
            tempEnemy.GetComponent<LaserEnemy>().weapon = laserWeapon;
            tempEnemy.GetComponent<LaserEnemy>().SetEnemy(4, 1f, isBoss);
        }
        else if (tempEnemy.GetComponent<BlinderEnemy>() != null)
        {
            tempEnemy.GetComponent<BlinderEnemy>().weapon = blinderWeapon;
            tempEnemy.GetComponent<BlinderEnemy>().SetEnemy(1, 2f, isBoss);
        }
        else if (tempEnemy.GetComponent<AttractEnemy>() != null)
        {
            tempEnemy.GetComponent<AttractEnemy>().weapon = absorbsWeapon;
            tempEnemy.GetComponent<AttractEnemy>().SetEnemy(2, 3f, isBoss);
        }
        else if (tempEnemy.GetComponent<SpawnEnemy>() != null)
        {
            tempEnemy.GetComponent<SpawnEnemy>().SetEnemy(10, 3f, isBoss);
        }
        else { return; }
    }

    IEnumerator EnemySpawner()
    {
        while (isEnemySpawning)
        {
            yield return new WaitForSeconds(2.5f / ((enemySpawnRate * mulitplierSpawnRate > 0.25f)?enemySpawnRate * mulitplierSpawnRate:0.25f));
            CreateEnemy();
        }
    }
}
