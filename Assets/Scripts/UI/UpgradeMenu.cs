using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UpgradeMenu : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject upgradeMenuPanel;
    
    [Header("Tab System")]
    [SerializeField] private Button aptitudeTabButton;
    [SerializeField] private Button perkTabButton;
    [SerializeField] private GameObject aptitudePanel;
    [SerializeField] private GameObject perkPanel;
    
    [Header("Aptitude UI")]
    [SerializeField] private Button[] aptitudeButtons; // 10 buttons for aptitudes
    [SerializeField] private TMP_Text[] aptitudeTitles;
    [SerializeField] private TMP_Text[] aptitudeLevels;
    [SerializeField] private TMP_Text aptitudePointsText;
    
    [Header("Perk UI")]
    [SerializeField] private Button[] perkButtons;
    [SerializeField] private TMP_Text[] perkTitles;
    [SerializeField] private TMP_Text[] perkDescriptions;
    [SerializeField] private TMP_Text[] perkCosts;
    [SerializeField] private TMP_Text perkPointsText;
    
    [Header("General UI")]
    [SerializeField] private Button closeButton;

    [Header("Perk Categories")]
    [SerializeField] private PerkTreeCategory[] perkCategories;

    private Player player;
    private PerkSystem perkSystem;
    private AptitudeSystem aptitudeSystem;
    private bool isMenuOpen = false;
    private bool isAptitudeTabActive = true; // Start with aptitude tab

    void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseMenu);
            
        // Tab button listeners
        if (aptitudeTabButton != null)
            aptitudeTabButton.onClick.AddListener(() => SwitchTab(true));
        if (perkTabButton != null)
            perkTabButton.onClick.AddListener(() => SwitchTab(false));
        
        // Initialize aptitude buttons
        for (int i = 0; i < aptitudeButtons.Length && i < 10; i++)
        {
            int index = i; // Capture for closure
            if (aptitudeButtons[i] != null)
                aptitudeButtons[i].onClick.AddListener(() => UpgradeAptitude(index));
        }
        
        // Initialize perk buttons
        for (int i = 0; i < perkButtons.Length; i++)
        {
            int index = i; // Capture for closure
            if (perkButtons[i] != null)
                perkButtons[i].onClick.AddListener(() => PurchasePerk(index));
        }
        
        CloseMenu(); // Start with menu closed
    }

    public void Initialize(Player playerRef, PerkSystem perkSystemRef, AptitudeSystem aptitudeSystemRef)
    {
        player = playerRef;
        perkSystem = perkSystemRef;
        aptitudeSystem = aptitudeSystemRef;
    }

    public void ToggleMenu()
    {
        if (isMenuOpen)
            CloseMenu();
        else
            OpenMenu();
    }

    public void OpenMenu()
    {
        if (upgradeMenuPanel == null || player == null) return;
        
        isMenuOpen = true;
        upgradeMenuPanel.SetActive(true);
        Time.timeScale = 0f; // Pause game
        
        // Update tab button text first
        UpdateTabButtonText();
        
        // Default to aptitude tab (more frequent upgrades)
        SwitchTab(true);
        RefreshMenu();
    }

    public void CloseMenu()
    {
        if (upgradeMenuPanel == null) return;
        
        isMenuOpen = false;
        upgradeMenuPanel.SetActive(false);
        Time.timeScale = 1f; // Resume game
    }

    void SwitchTab(bool showAptitudes)
    {
        isAptitudeTabActive = showAptitudes;
        
        // Toggle panels
        if (aptitudePanel != null)
            aptitudePanel.SetActive(showAptitudes);
        if (perkPanel != null)
            perkPanel.SetActive(!showAptitudes);
            
        // Update tab button states
        if (aptitudeTabButton != null)
            aptitudeTabButton.interactable = !showAptitudes;
        if (perkTabButton != null)
            perkTabButton.interactable = showAptitudes;
            
        RefreshMenu();
    }

    void RefreshMenu()
    {
        if (player == null) return;
        
        // Update tab button text with available points
        UpdateTabButtonText();
        
        if (isAptitudeTabActive)
        {
            RefreshAptitudeMenu();
        }
        else
        {
            RefreshPerkMenu();
        }
    }
    
    public void UpdateTabButtonText()
    {
        if (player == null) return;
        
        // Update aptitude tab button text
        if (aptitudeTabButton != null)
        {
            TMP_Text aptitudeButtonText = aptitudeTabButton.GetComponentInChildren<TMP_Text>();
            if (aptitudeButtonText != null)
            {
                int aptitudePoints = player.GetAptitudePoints();
                aptitudeButtonText.SetText($"Aptitude ({aptitudePoints})");
            }
        }
        
        // Update perk tab button text
        if (perkTabButton != null)
        {
            TMP_Text perkButtonText = perkTabButton.GetComponentInChildren<TMP_Text>();
            if (perkButtonText != null)
            {
                int perkPoints = player.GetPerkPoints();
                perkButtonText.SetText($"Perks ({perkPoints})");
            }
        }
    }
    
    void RefreshAptitudeMenu()
    {
        Debug.Log("RefreshAptitudeMenu() called");
        
        if (aptitudeSystem == null) 
        {
            Debug.LogError("RefreshAptitudeMenu: AptitudeSystem is null!");
            return;
        }
        
        if (player == null)
        {
            Debug.LogError("RefreshAptitudeMenu: Player is null!");
            return;
        }
        
        // Update aptitude points display
        if (aptitudePointsText != null)
            aptitudePointsText.SetText($"Aptitude Points: {player.GetAptitudePoints()}");
        
        AptitudeUpgrade[] upgrades = aptitudeSystem.GetAvailableUpgrades();
        Debug.Log($"RefreshAptitudeMenu: Retrieved {upgrades?.Length ?? 0} upgrades");
        
        if (upgrades == null)
        {
            Debug.LogError("RefreshAptitudeMenu: Available upgrades is null!");
            return;
        }
        
        // Update aptitude buttons
        for (int i = 0; i < aptitudeButtons.Length && i < upgrades.Length; i++)
        {
            AptitudeUpgrade upgrade = upgrades[i];
            bool canAfford = player.GetAptitudePoints() > 0;
            bool canUpgrade = upgrade.CanUpgrade();
            
            if (aptitudeTitles[i] != null)
                aptitudeTitles[i].SetText(upgrade.displayName);
            
            if (aptitudeLevels[i] != null)
            {
                string levelText = $"Level {upgrade.currentLevel}/{upgrade.maxLevel}\n+{upgrade.GetCurrentBonus():F1} Total";
                
                // Add "Next upgrade" description if can upgrade
                if (canUpgrade)
                {
                    string nextUpgrade = GetNextUpgradeDescription(upgrade);
                    levelText += $"\n{nextUpgrade}";
                }
                
                aptitudeLevels[i].SetText(levelText);
            }
            
            if (aptitudeButtons[i] != null)
            {
                aptitudeButtons[i].interactable = canAfford && canUpgrade;
                aptitudeButtons[i].gameObject.SetActive(true);
            }
        }
        
        // Hide unused buttons
        for (int i = upgrades.Length; i < aptitudeButtons.Length; i++)
        {
            if (aptitudeButtons[i] != null)
                aptitudeButtons[i].gameObject.SetActive(false);
        }
    }
    
    void RefreshPerkMenu()
    {
        if (perkSystem == null) return;
        
        // Update perk points display
        if (perkPointsText != null)
            perkPointsText.SetText($"Perk Points: {player.GetPerkPoints()}");
        
        // Get available perks for current level
        List<Perk> availablePerks = perkSystem.GetAvailablePerks(player.GetCurrentLevel());
        
        // Update perk buttons
        for (int i = 0; i < perkButtons.Length && i < availablePerks.Count; i++)
        {
            Perk perk = availablePerks[i];
            bool canAfford = player.GetPerkPoints() >= perk.cost;
            bool isUnlocked = perkSystem.IsPerkUnlocked(perk.id);
            
            if (perkTitles[i] != null)
                perkTitles[i].SetText(perk.name);
            
            if (perkDescriptions[i] != null)
                perkDescriptions[i].SetText(perk.description);
            
            if (perkCosts[i] != null)
                perkCosts[i].SetText($"Cost: {perk.cost}");
            
            if (perkButtons[i] != null)
            {
                perkButtons[i].interactable = canAfford && !isUnlocked;
                perkButtons[i].gameObject.SetActive(true);
            }
        }
        
        // Hide unused buttons
        for (int i = availablePerks.Count; i < perkButtons.Length; i++)
        {
            if (perkButtons[i] != null)
                perkButtons[i].gameObject.SetActive(false);
        }
    }

    void UpgradeAptitude(int aptitudeIndex)
    {
        Debug.Log($"UpgradeAptitude called with index: {aptitudeIndex}");
        
        if (aptitudeSystem == null) 
        {
            Debug.LogError("AptitudeSystem is null!");
            return;
        }
        
        if (player == null) 
        {
            Debug.LogError("Player is null!");
            return;
        }
        
        if (aptitudeIndex < 0 || aptitudeIndex >= 10) 
        {
            Debug.LogError($"Invalid aptitude index: {aptitudeIndex}");
            return;
        }
        
        AptitudeType type = (AptitudeType)aptitudeIndex;
        Debug.Log($"Attempting to upgrade aptitude: {type}");
        
        int pointsBefore = player.GetAptitudePoints();
        bool success = aptitudeSystem.TryUpgradeAptitude(type);
        int pointsAfter = player.GetAptitudePoints();
        
        Debug.Log($"Upgrade attempt: success={success}, points before={pointsBefore}, points after={pointsAfter}");
        
        if (success)
        {
            RefreshAptitudeMenu();
            UpdateTabButtonText(); // Update tab text to reflect spent points
            Debug.Log($"Successfully upgraded aptitude: {type}");
        }
        else
        {
            Debug.LogWarning($"Failed to upgrade aptitude: {type}");
        }
    }
    
    string GetNextUpgradeDescription(AptitudeUpgrade upgrade)
    {
        switch (upgrade.type)
        {
            case AptitudeType.MaxHealth:
                float currentMaxHealth = player.health.GetMaxHealth();
                float nextMaxHealth = currentMaxHealth + upgrade.valuePerLevel;
                return $"Next: {currentMaxHealth:F0} → {nextMaxHealth:F0} HP";
                
            case AptitudeType.HealthRegen:
                float currentRegen = player.health.GetRegenHealth();
                float regenIncrease = currentRegen * upgrade.valuePerLevel;
                return $"Next: +{regenIncrease:F1} HP/sec";
                
            case AptitudeType.MovementSpeed:
                return $"Next: +{upgrade.valuePerLevel * 100:F0}% speed";
                
            case AptitudeType.FireRate:
                return $"Next: +{upgrade.valuePerLevel * 100:F0}% fire rate";
                
            case AptitudeType.Damage:
                return $"Next: +{upgrade.valuePerLevel * 100:F0}% damage";
                
            case AptitudeType.WeaponCapacity:
                return $"Next: +{upgrade.valuePerLevel * 100:F0}% ammo/time";
                
            case AptitudeType.ShieldDuration:
                float currentDuration = player.GetShieldDuration();
                float nextDuration = currentDuration + upgrade.valuePerLevel;
                return $"Next: {currentDuration:F1}s → {nextDuration:F1}s";
                
            case AptitudeType.ExpGain:
                return $"Next: +{upgrade.valuePerLevel * 100:F0}% XP gain";
                
            case AptitudeType.LuckPickup:
                return $"Next: +{upgrade.valuePerLevel * 100:F0}% pickup rate";
                
            case AptitudeType.CriticalChance:
                float currentCrit = player.GetCriticalChance() * 100;
                float nextCrit = currentCrit + (upgrade.valuePerLevel * 100);
                return $"Next: {currentCrit:F0}% → {nextCrit:F0}% crit";
                
            default:
                return "Next: +" + upgrade.valuePerLevel.ToString("F1");
        }
    }

    void PurchasePerk(int buttonIndex)
    {
        if (player == null || perkSystem == null) return;
        
        List<Perk> availablePerks = perkSystem.GetAvailablePerks(player.GetCurrentLevel());
        
        if (buttonIndex >= 0 && buttonIndex < availablePerks.Count)
        {
            Perk perk = availablePerks[buttonIndex];
            
            if (player.GetPerkPoints() >= perk.cost && !perkSystem.IsPerkUnlocked(perk.id))
            {
                // Spend perk points
                player.SpendPerkPoints(perk.cost);
                
                // Unlock perk
                perkSystem.UnlockPerk(perk.id);
                
                // Apply perk effects
                ApplyPerkEffect(perk);
                
                // Refresh menu
                RefreshPerkMenu();
                UpdateTabButtonText(); // Update tab text to reflect spent points
                
                Debug.Log($"Purchased perk: {perk.name}");
            }
        }
    }

    void ApplyPerkEffect(Perk perk)
    {
        switch (perk.type)
        {
            // Resource management perks
            case PerkType.FuelCapacity:
                player.UpgradeFuelCapacity(perk.value);
                break;
            case PerkType.MagnetRadius:
                player.UpgradeMagnetRadius(perk.value);
                break;
            case PerkType.MagnetDuration:
                player.UpgradeMagnetDuration(perk.value);
                break;
            case PerkType.BoostEfficiency:
                player.UpgradeBoostEfficiency(perk.value);
                break;
            case PerkType.WeaponUnlock:
                player.UnlockWeapon((PlayerWeaponType)perk.weaponType);
                break;
            case PerkType.ShieldCapacity:
                player.UpgradeShieldCapacity((int)perk.value);
                break;
            case PerkType.SpawnRateReduction:
                GameManager.GetInstance().UpgradeSpawnRateReduction(perk.value);
                break;
        }
    }

    public bool IsMenuOpen()
    {
        return isMenuOpen;
    }
}

[System.Serializable]
public class PerkTreeCategory
{
    public string categoryName;
    public Perk[] perks;
}