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
    private Weapon spikeThrow = new Weapon("Spike", 10, 15);
    private Weapon laserWeapon = new Weapon("Laser", 3, 0);
    private Weapon blinderWeapon = new Weapon("Blinder", 0, 0);
    private Weapon absorbsWeapon = new Weapon("Absorbs", 4, 0);

    private int bossEvent = 10; 
    public float multiplierEnemyHealth, multiplierPlayerHealth, mulitplierSpawnRate, multiplierPoint, multiplierEnemyDamage, multiplierPlayerDamage, multiplierEnemySpeed, multiplierPlayerSpeed, multiplierPlayerHealthRegen = 1;


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
        uIManager.UpdateHealth(player.health.GetHealth());
        player.OnDeath += StopGame;
        scoreManager.onScoreChange += CheckPointsForEvents;
        isPlaying = true;
        isEndlessMode = endlessMode;
        if (isEndlessMode)
            currentLimit = enemyPrefabs.Length;
        OnGameStart?.Invoke();
        StartCoroutine(GameStarter());
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

    void SpawnBoss()
    {
        if (!isEndlessMode){
            tempEnemy = Instantiate(enemyPrefabs[currentLimit]);
            tempEnemy.transform.position = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)].position;
            BossSpawnPrep();
            if (!isEndlessMode)
                tempEnemy.GetComponent<Enemy>().OnBossDeath += UpCurrentLimit;
            SetEnemy(true);
        }else{
            CreateEnemy(true);
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
        if (currentLimit < enemyPrefabs.Length-1)
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
        uIManager.SetBossMaxHealth(tempEnemy.GetComponent<Enemy>().GetHealth());
        uIManager.BossSpawned(tempEnemy.GetComponent<Enemy>());
    }

    void NormalModeOver(){
        uIManager.ShowNormalModeOver();
    }

    void CardSelection(){

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
        if (score >= bossEvent){
            SpawnBoss();
            bossEvent = bossEvent*2;
        }else{
            if (UnityEngine.Random.Range(0,100) > 90){
                Debug.Log("Make Event");
            }
        }
    }

    public void StopGame()
    {
        isEnemySpawning = false;
        scoreManager.SetHighScore();
        StartCoroutine(GameStopper());
    }

    IEnumerator GameStopper()
    {
        isEnemySpawning = false;
        yield return new WaitForSeconds(0.5f);
        isPlaying = false;

        var allenemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        var allpickups = FindObjectsByType<Pickup>(FindObjectsSortMode.None);
        foreach (Enemy enemy in allenemies)
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }
        foreach (Pickup pickup in allpickups)
        {
            if (pickup != null)
                Destroy(pickup.gameObject);
        }
        OnGameOver?.Invoke();
    }

    void CreateEnemy(bool isBoss = false)
    {
        tempEnemy = Instantiate(enemyPrefabs[UnityEngine.Random.Range(0, currentLimit)]);
        tempEnemy.transform.position = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)].position;
        if (isBoss)
            BossSpawnPrep();
        if (isBoss && !isEndlessMode)
            tempEnemy.GetComponent<Enemy>().OnBossDeath += UpCurrentLimit;
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
            tempEnemy.GetComponent<SpikeEnemy>().SetEnemy(20, 2f, isBoss);
        }
        else if (tempEnemy.GetComponent<LaserEnemy>() != null)
        {
            tempEnemy.GetComponent<LaserEnemy>().weapon = laserWeapon;
            tempEnemy.GetComponent<LaserEnemy>().SetEnemy(3, 2f, isBoss);
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
