using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
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

    private Weapon meleeWeapon = new Weapon("Melee", 1, 0);
    private Weapon machineGunWeapon = new Weapon("Machine Gun", 2, 10);
    private Weapon sniperWeapon = new Weapon("Sniper", 5, 15);
    private Weapon explosionWeapon = new Weapon("Explosion", 20, 0);
    private Weapon electricWeapon = new Weapon("Electric", 2, 0);
    private Weapon spikeThrow = new Weapon("Spike", 10, 15);
    private Weapon laserWeapon = new Weapon("Laser", 3, 0);
    private Weapon blinderWeapon = new Weapon("Blinder", 0, 0);
    private Weapon absorbsWeapon = new Weapon("Absorbs", 4, 0);

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

    public void StartGame()
    {
        player = Instantiate(playerPrefab, Vector2.zero, Quaternion.identity).GetComponent<Player>();
        uIManager.UpdateHealth(player.health.GetHealth());
        player.OnDeath += StopGame;
        scoreManager.onScoreChange += CheckPointsForEvents;
        isPlaying = true;
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
        
    }

    void CheckPointsForEvents(float score)
    {
        
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

    void CreateEnemy()
    {
        tempEnemy = Instantiate(enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Length)]);
        tempEnemy.transform.position = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)].position;
        if (tempEnemy.GetComponent<MeleeEnemy>() != null)
        {
            tempEnemy.GetComponent<MeleeEnemy>().weapon = meleeWeapon;
            tempEnemy.GetComponent<MeleeEnemy>().SetMeleeEnemy(1, 0.25f);
        }
        else if (tempEnemy.GetComponent<ExploderEnemy>() != null)
        {
            tempEnemy.GetComponent<ExploderEnemy>().weapon = explosionWeapon;
            tempEnemy.GetComponent<ExploderEnemy>().SetExploderEnemy(1, 4f);
        }
        else if (tempEnemy.GetComponent<MachineGunEnemy>() != null)
        {
            tempEnemy.GetComponent<MachineGunEnemy>().weapon = machineGunWeapon;
            tempEnemy.GetComponent<MachineGunEnemy>().SetMachineGunEnemy(5, 0.3f);
        }
        else if (tempEnemy.GetComponent<ShooterEnemy>() != null)
        {
            tempEnemy.GetComponent<ShooterEnemy>().weapon = sniperWeapon;
            tempEnemy.GetComponent<ShooterEnemy>().SetShooterEnemy(8, 2f);
        }
        else if (tempEnemy.GetComponent<ElectricEnemy>() != null)
        {
            tempEnemy.GetComponent<ElectricEnemy>().weapon = electricWeapon;
            tempEnemy.GetComponent<ElectricEnemy>().SetElectricEnemy(2, 2f);
        }
        else if (tempEnemy.GetComponent<SpikeEnemy>() != null)
        {
            tempEnemy.GetComponent<SpikeEnemy>().weapon = spikeThrow;
            tempEnemy.GetComponent<SpikeEnemy>().SetSpikeEnemy(20, 2f);
        }
        else if (tempEnemy.GetComponent<LaserEnemy>() != null)
        {
            tempEnemy.GetComponent<LaserEnemy>().weapon = laserWeapon;
            tempEnemy.GetComponent<LaserEnemy>().SetLaserEnemy(3, 2f);
        }
        else if (tempEnemy.GetComponent<BlinderEnemy>() != null)
        {
            tempEnemy.GetComponent<BlinderEnemy>().weapon = blinderWeapon;
            tempEnemy.GetComponent<BlinderEnemy>().SetBlinderEnemy(1, 2f);
        }
        else if (tempEnemy.GetComponent<AttractEnemy>() != null)
        {
            tempEnemy.GetComponent<AttractEnemy>().weapon = absorbsWeapon;
            tempEnemy.GetComponent<AttractEnemy>().SetAbsorbsEnemy(2, 3f);
        }
        else if (tempEnemy.GetComponent<SpawnEnemy>() != null)
        {
            tempEnemy.GetComponent<SpawnEnemy>().SetSpawnerEnemy(10, 3);
        }
        else { return; }
    }

    IEnumerator EnemySpawner()
    {
        while (isEnemySpawning)
        {
            yield return new WaitForSeconds(2.5f / enemySpawnRate);
            CreateEnemy();
        }
    }
}
