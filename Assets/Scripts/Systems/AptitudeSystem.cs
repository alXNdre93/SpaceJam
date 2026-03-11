using UnityEngine;

public enum AptitudeType
{
    MaxHealth = 0,           // Increase maximum health
    HealthRegen = 1,         // Faster health regeneration 
    MovementSpeed = 2,       // Increase movement speed
    FireRate = 3,            // Faster firing rate
    Damage = 4,              // Increase weapon damage
    WeaponCapacity = 5,      // Increase weapon ammo/time capacity
    ShieldDuration = 6,      // Longer shield duration
    ExpGain = 7,             // Bonus experience from enemies
    LuckPickup = 8,          // Better pickup spawn rates
    CriticalChance = 9       // Chance for critical damage
}

[System.Serializable]
public class AptitudeUpgrade
{
    public AptitudeType type;
    public string displayName;
    public string description;
    public int currentLevel;
    public int maxLevel;
    public float valuePerLevel;
    
    public AptitudeUpgrade(AptitudeType upgradeType, string name, string desc, int max, float value)
    {
        type = upgradeType;
        displayName = name;
        description = desc;
        currentLevel = 0;
        maxLevel = max;
        valuePerLevel = value;
    }
    
    public bool CanUpgrade()
    {
        return currentLevel < maxLevel;
    }
    
    public float GetCurrentBonus()
    {
        return currentLevel * valuePerLevel;
    }
}

public class AptitudeSystem : MonoBehaviour
{
    [SerializeField] private AptitudeUpgrade[] availableUpgrades;
    private Player player;
    
    private void Start()
    {
        Debug.Log("AptitudeSystem.Start() called");
        InitializeUpgrades();
        Debug.Log($"AptitudeSystem: Initialized {availableUpgrades?.Length ?? 0} upgrades");
        
        if (availableUpgrades != null)
        {
            for (int i = 0; i < availableUpgrades.Length; i++)
            {
                Debug.Log($"Upgrade {i}: {availableUpgrades[i].type} - {availableUpgrades[i].displayName}");
            }
        }
        
        // Subscribe to game start event to get player reference when game begins
        GameManager gameManager = GameManager.GetInstance();
        if (gameManager != null)
        {
            gameManager.OnGameStart += OnGameStart;
            Debug.Log("AptitudeSystem: Subscribed to OnGameStart event");
        }
        else
        {
            Debug.LogError("AptitudeSystem: GameManager instance is null!");
        }
    }
    
    private void OnGameStart()
    {
        Debug.Log("AptitudeSystem.OnGameStart() called");
        player = GameManager.GetInstance().Getplayer();
        Debug.Log($"AptitudeSystem: Player found: {player != null}");
        
        if (player == null)
        {
            Debug.LogError("AptitudeSystem: Player is still null after game start!");
        }
    }
    
    private void InitializeUpgrades()
    {
        availableUpgrades = new AptitudeUpgrade[]
        {
            new AptitudeUpgrade(AptitudeType.MaxHealth, "Vitality", "Increase maximum health by 20 per level", 10, 20f),
            new AptitudeUpgrade(AptitudeType.HealthRegen, "Regeneration", "Increase health regen rate by 25% per level", 5, 0.25f),
            new AptitudeUpgrade(AptitudeType.MovementSpeed, "Agility", "Increase movement speed by 15% per level", 5, 0.15f),
            new AptitudeUpgrade(AptitudeType.FireRate, "Rapid Fire", "Increase fire rate by 10% per level", 5, 0.1f),
            new AptitudeUpgrade(AptitudeType.Damage, "Power", "Increase all weapon damage by 20% per level", 10, 0.2f),
            new AptitudeUpgrade(AptitudeType.WeaponCapacity, "Capacity", "Increase weapon capacity by 25% per level", 5, 0.25f),
            new AptitudeUpgrade(AptitudeType.ShieldDuration, "Shielding", "Increase shield duration by 0.5s per level", 5, 0.5f),
            new AptitudeUpgrade(AptitudeType.ExpGain, "Wisdom", "Increase experience gain by 25% per level", 5, 0.25f),
            new AptitudeUpgrade(AptitudeType.LuckPickup, "Fortune", "Better pickup spawn rates", 3, 0.2f),
            new AptitudeUpgrade(AptitudeType.CriticalChance, "Precision", "5% critical chance per level (2x damage)", 4, 0.05f)
        };
    }
    
    public bool TryUpgradeAptitude(AptitudeType type)
    {
        Debug.Log($"TryUpgradeAptitude called for: {type}");
        
        if (player == null)
        {
            Debug.LogError("TryUpgradeAptitude: Player is null!");
            return false;
        }
        
        int pointsBefore = player.GetAptitudePoints();
        Debug.Log($"Player has {pointsBefore} aptitude points");
        
        if (!player.SpendAptitudePoint())
        {
            Debug.LogWarning($"TryUpgradeAptitude: Failed to spend aptitude point. Points available: {pointsBefore}");
            return false;
        }
        
        Debug.Log($"Successfully spent aptitude point. Looking for upgrade type: {type}");
        
        foreach (var upgrade in availableUpgrades)
        {
            Debug.Log($"Checking upgrade: {upgrade.type} (looking for {type})");
            if (upgrade.type == type)
            {
                Debug.Log($"Found matching upgrade. Level: {upgrade.currentLevel}/{upgrade.maxLevel}, CanUpgrade: {upgrade.CanUpgrade()}");
                if (upgrade.CanUpgrade())
                {
                    upgrade.currentLevel++;
                    Debug.Log($"Upgrading {upgrade.displayName} to level {upgrade.currentLevel}");
                    ApplyUpgradeEffect(upgrade);
                    return true;
                }
                else
                {
                    Debug.LogWarning($"Cannot upgrade {upgrade.displayName} - already at max level {upgrade.maxLevel}");
                    // Refund the aptitude point since we couldn't upgrade
                    player.AddAptitudePoints(1);
                    return false;
                }
            }
        }
        
        Debug.LogError($"Could not find upgrade for type: {type}");
        // Refund the aptitude point since we couldn't find the upgrade
        player.AddAptitudePoints(1);
        return false;
    }
    
    private void ApplyUpgradeEffect(AptitudeUpgrade upgrade)
    {
        switch (upgrade.type)
        {
            case AptitudeType.MaxHealth:
                // Increase max health and heal player
                player.IncreaseMaxHealth(upgrade.valuePerLevel);
                break;
                
            case AptitudeType.HealthRegen:
                // Increase regen multiplier
                GameManager.GetInstance().multiplierPlayerHealthRegen += upgrade.valuePerLevel;
                break;
                
            case AptitudeType.MovementSpeed:
                // Increase speed multiplier
                GameManager.GetInstance().multiplierPlayerSpeed += upgrade.valuePerLevel;
                break;
                
            case AptitudeType.FireRate:
                // Increase fire rate (faster shooting) - use player method since GameManager doesn't have this multiplier
                player.UpgradeFireRate(upgrade.valuePerLevel);
                break;
                
            case AptitudeType.Damage:
                // Increase damage multiplier
                GameManager.GetInstance().multiplierPlayerDamage += upgrade.valuePerLevel;
                break;
                
            case AptitudeType.WeaponCapacity:
                // Increase weapon capacity - use player method since GameManager doesn't have this multiplier
                player.UpgradeWeaponCapacity(upgrade.valuePerLevel);
                break;
                
            case AptitudeType.ShieldDuration:
                // Increase shield duration
                player.UpgradeShieldDuration(upgrade.valuePerLevel);
                break;
                
            case AptitudeType.ExpGain:
                // Increase experience gain - use player method since GameManager doesn't have this multiplier
                player.UpgradeExperienceGain(upgrade.valuePerLevel);
                break;
                
            case AptitudeType.LuckPickup:
                // Increase pickup luck - use player method since GameManager doesn't have this multiplier
                player.UpgradePickupRates(upgrade.valuePerLevel);
                break;
                
            case AptitudeType.CriticalChance:
                // Increase critical chance - use player method since GameManager doesn't have this multiplier
                player.UpgradeCriticalChance(upgrade.valuePerLevel);
                break;
        }
        
        Debug.Log($"Upgraded {upgrade.displayName} to level {upgrade.currentLevel}!");
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        GameManager gameManager = GameManager.GetInstance();
        if (gameManager != null)
        {
            gameManager.OnGameStart -= OnGameStart;
        }
    }
    
    public AptitudeUpgrade[] GetAvailableUpgrades()
    {
        return availableUpgrades;
    }
}