using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Slider playerSliderHealth;
    [SerializeField] private UnityEngine.UI.Slider enemysliderHealth;
    [SerializeField] private UnityEngine.UI.Slider fuelSlider;
    [SerializeField] private UnityEngine.UI.Slider magnetCooldownSlider;
    [SerializeField] private UnityEngine.UI.Slider experienceSlider;
    [SerializeField] private TMP_Text txtScore, txtMultiply, txtFireRate, txtHighScore, txtWeaponHUD, txtLevel, txtExperience, txtAptitudePoints, txtPerkPoints;
    [SerializeField] GameObject nuke1, nuke2, nuke3, shield1, shield2, machineGunTimerUI, machineGunTimerPlayer, menuCanvas, normalModeOverText;
    [SerializeField] TMP_Text levelUpNotification;
    [SerializeField] Camera cam;
    private Player player;
    private ScoreManager scoreManager;

    private void Start()
    {
        Debug.Log($"UIManager.Start() - Slider assignments: fuelSlider={fuelSlider != null}, magnetSlider={magnetCooldownSlider != null}");
        scoreManager = GameManager.GetInstance().scoreManager;
        GameManager.GetInstance().OnGameStart += GameStarted;
        GameManager.GetInstance().OnGameOver += GameOver;
    }


    public void GameStarted()
    {
        Debug.Log("UIManager.GameStarted() called");
        player = GameManager.GetInstance().Getplayer();
        player.OnHealthUpdate += UpdateHealth;
        menuCanvas.SetActive(false);
        UpdateWeaponHUD();
        
        // Initialize level display
        UpdateLevelDisplay(player.GetCurrentLevel(), player.GetCurrentExperience(), player.GetExperienceToNextLevel());
        UpdateAptitudePoints(player.GetAptitudePoints());
        
        // Initialize fuel and magnet sliders
        Debug.Log($"About to initialize sliders. Player fuel: {player.GetCurrentFuel()}/{player.GetMaxFuel()}");
        UpdateFuel(player.GetCurrentFuel(), player.GetMaxFuel());
        UpdateMagnetCooldown(player.GetMagnetCooldownRemaining(), player.GetMagnetCooldownDuration());
        
        // Initialize upgrade menu system
        InitializeUpgradeMenu();
    }

    public void GameOver()
    {
        menuCanvas.SetActive(true);
    }

    public void BossSpawned(Enemy boss)
    {
        boss.OnBossHealthUpdate += UpdateEnemyHealth;
    }

    private void OnDisable()
    {
        player.OnHealthUpdate -= UpdateHealth;
    }

    public void UpdateHighScore()
    {
        txtHighScore.SetText("High Score: " + scoreManager.GetHighScore().ToString());
    }

    private void OnGUI()
    {
        if (machineGunTimerUI.activeSelf && player != null)
        {
            machineGunTimerPlayer.transform.position = cam.WorldToScreenPoint(player.transform.position);
            machineGunTimerPlayer.transform.rotation = player.transform.rotation;
        }
    }

    public void UpdateHealth(float currentHealth)
    {
        playerSliderHealth.value = currentHealth;
    }

    public void UpdateEnemyHealth(float currentHealth)
    {
        enemysliderHealth.value = currentHealth;
    }

    public void UpdateNukes(int nukeAvalaibale)
    {
        nuke1.SetActive(false);
        nuke2.SetActive(false);
        nuke3.SetActive(false);
        if (nukeAvalaibale == 1)
        {
            nuke1.SetActive(true);
            nuke2.SetActive(false);
            nuke3.SetActive(false);
        }
        else if (nukeAvalaibale == 2)
        {
            nuke1.SetActive(true);
            nuke2.SetActive(true);
            nuke3.SetActive(false);
        }
        else if (nukeAvalaibale == 3)
        {
            nuke1.SetActive(true);
            nuke2.SetActive(true);
            nuke3.SetActive(true);
        }
    }

    public void UpdateShields(int shieldAvailable)
    {
        shield1.SetActive(false);
        shield2.SetActive(false);
        
        if (shieldAvailable == 1)
        {
            shield1.SetActive(true);
            shield2.SetActive(false);
        }
        else if (shieldAvailable == 2)
        {
            shield1.SetActive(true);
            shield2.SetActive(true);
        }
    }

    public void UpdateScore()
    {
        txtScore.SetText(GameManager.GetInstance().scoreManager.GetScore().ToString());
    }

    public void UpdateAugments()
    {
        txtFireRate.SetText("Fire Rate: " + (Mathf.Floor(1 / player.fireRate * 100f) / 100f).ToString() + "/s");
        txtMultiply.SetText("Damage: " + (1 + player.multiplyShot).ToString() + "x");
    }

    public void MachineGunTimer()
    {
        machineGunTimerUI.SetActive(!machineGunTimerUI.activeSelf);
    }

    public void UpdateBossSprite(Sprite bossSprite)
    {
        enemysliderHealth.gameObject.transform.GetChild(enemysliderHealth.gameObject.transform.childCount - 1).GetComponent<UnityEngine.UI.Image>().sprite = bossSprite;
        enemysliderHealth.gameObject.transform.GetChild(enemysliderHealth.gameObject.transform.childCount - 2).GetComponent<UnityEngine.UI.Image>().sprite = bossSprite;
    }

    public void ToggleBossHealth()
    {
        enemysliderHealth.gameObject.SetActive(!enemysliderHealth.gameObject.activeSelf);
    }
    public bool isBossSliderActive()
    {
        return enemysliderHealth.gameObject.activeSelf;
    }

    public void SetBossMaxHealth(float maxHealth)
    {
        Debug.Log("bOSS mAX hEALTH "+maxHealth.ToString());
        enemysliderHealth.maxValue = maxHealth;
        enemysliderHealth.value = maxHealth;
    }

    public void SetPlayerMaxHealth(float maxHealth)
    {
        playerSliderHealth.maxValue = maxHealth;
    }

    public void ShowNormalModeOver()
    {
        normalModeOverText.SetActive(true);
        float currentTimeScale = Time.timeScale;
        Time.timeScale = 0;
        while (!Input.anyKey) { Debug.Log("Waiting For input"); }

        Time.timeScale = currentTimeScale;
        normalModeOverText.SetActive(false);
    }

    public void UpdateWeaponHUD()
    {
        if (txtWeaponHUD == null || player == null)
            return;

        string weaponName = player.GetActiveWeaponName();
        float resource = player.GetActiveWeaponResource();
        float maxResource = player.GetActiveWeaponMaxResource();
        WeaponResourceType resourceType = player.GetActiveWeaponResourceType();

        if (resourceType == WeaponResourceType.Infinite)
        {
            txtWeaponHUD.SetText($"{weaponName}");
        }
        else if (resourceType == WeaponResourceType.Ammo)
        {
            txtWeaponHUD.SetText($"{weaponName}: {Mathf.CeilToInt(resource)}/{Mathf.CeilToInt(maxResource)}");
        }
        else if (resourceType == WeaponResourceType.Time)
        {
            txtWeaponHUD.SetText($"{weaponName}: {Mathf.Max(0f, resource):F1}s/{maxResource:F1}s");
        }
    }

    public void UpdateLevelDisplay(int level, int currentExp, int expToNext)
    {
        if (txtLevel != null)
            txtLevel.SetText($"{level}");
        if (txtExperience != null)
            txtExperience.SetText($"{currentExp}/{expToNext} XP");
        if (experienceSlider != null)
        {
            experienceSlider.maxValue = expToNext;
            experienceSlider.value = currentExp;
        }
    }
    
    public void UpdateAptitudePoints(int points)
    {
        if (txtAptitudePoints != null)
            txtAptitudePoints.SetText($"{points}");
            
        // Update upgrade menu tab text if it exists
        UpdateUpgradeMenuTabText();
    }
    
    public void UpdatePerkPoints(int points)
    {
        if (txtPerkPoints != null)
            txtPerkPoints.SetText($"{points}");
            
        // Update upgrade menu tab text if it exists
        UpdateUpgradeMenuTabText();
    }
    
    public void ShowLevelUpNotification(int newLevel)
    {
        if (levelUpNotification != null)
        {
            // Make sure the notification is in the right parent
            Transform parent = levelUpNotification.transform.parent;
            while (parent != null)
            {
                if (!parent.gameObject.activeInHierarchy)
                {
                    Debug.LogWarning($"Level up notification parent '{parent.name}' is inactive, activating it.");
                    parent.gameObject.SetActive(true);
                }
                parent = parent.parent;
            }
            
            // Update the text content directly since levelUpNotification is already a TMP_Text
            levelUpNotification.SetText($"LEVEL UP!\nLevel {newLevel}");
            Debug.Log($"Level up text set to: LEVEL UP! Level {newLevel}");
            
            levelUpNotification.gameObject.SetActive(true);
            Debug.Log($"Level up notification activated. Active in hierarchy: {levelUpNotification.gameObject.activeInHierarchy}");
            
            // Auto-hide after 3 seconds
            StartCoroutine(HideLevelUpNotificationAfterDelay(3f));
        }
        else
        {
            Debug.LogError("Level up notification TMP_Text is null! Check Inspector assignment.");
        }
        
        // Optional: Display level up text
        Debug.Log($"LEVEL UP! Reached Level {newLevel}!");
    }
    
    public void ShowBossKillNotification()
    {
        // Use the level up notification system for now, or create a separate boss kill notification
        if (levelUpNotification != null)
        {
            levelUpNotification.SetText("BOSS DEFEATED!\n+1 Perk Point");
            levelUpNotification.gameObject.SetActive(true);
            StartCoroutine(HideLevelUpNotificationAfterDelay(2f));
        }
        
        Debug.Log("BOSS DEFEATED! +1 Perk Point awarded!");
    }
    
    private System.Collections.IEnumerator HideLevelUpNotificationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (levelUpNotification != null)
            levelUpNotification.gameObject.SetActive(false);
    }
    
    public void UpdateFuel(float currentFuel, float maxFuel)
    {
        Debug.Log($"UpdateFuel called: currentFuel={currentFuel}, maxFuel={maxFuel}, slider={fuelSlider != null}");
        if (fuelSlider != null)
        {
            fuelSlider.value = currentFuel / maxFuel;
            Debug.Log($"Fuel slider value set to: {fuelSlider.value}");
        }
    }
    
    public void UpdateMagnetCooldown(float currentCooldown, float maxCooldown)
    {
        Debug.Log($"UpdateMagnetCooldown called: currentCooldown={currentCooldown}, maxCooldown={maxCooldown}, slider={magnetCooldownSlider != null}");
        if (magnetCooldownSlider != null)
        {
            magnetCooldownSlider.value = (maxCooldown - currentCooldown) / maxCooldown;
            Debug.Log($"Magnet slider value set to: {magnetCooldownSlider.value}");
        }
    }
    
    private void Update()
    {
        // Only update UI if the game is actually playing
        if (!GameManager.GetInstance().IsPlaying())
            return;
            
        if (player != null)
        {
            // Update fuel slider
            UpdateFuel(player.GetCurrentFuel(), player.GetMaxFuel());
            
            // Update magnet cooldown slider
            UpdateMagnetCooldown(player.GetMagnetCooldownRemaining(), player.GetMagnetCooldownDuration());
        }
        else if (Time.frameCount % 60 == 0) // Only log once per second
        {
            Debug.LogWarning("UIManager.Update() - Player is null even though game is playing!");
        }
    }
    
    void InitializeUpgradeMenu()
    {
        UpgradeMenu upgradeMenu = FindFirstObjectByType<UpgradeMenu>();
        PerkSystem perkSystem = GameManager.GetInstance().GetPerkSystem();
        AptitudeSystem aptitudeSystem = FindFirstObjectByType<AptitudeSystem>();
        
        if (upgradeMenu != null && perkSystem != null && player != null && aptitudeSystem != null)
        {
            upgradeMenu.Initialize(player, perkSystem, aptitudeSystem);
        }
    }
    
    void UpdateUpgradeMenuTabText()
    {
        UpgradeMenu upgradeMenu = FindFirstObjectByType<UpgradeMenu>();
        if (upgradeMenu != null)
        {
            upgradeMenu.UpdateTabButtonText();
        }
    }
}
