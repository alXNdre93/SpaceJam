using System.Collections.Generic;
using UnityEngine;

public class PerkSystem : MonoBehaviour
{
    [Header("Perk Database")]
    [SerializeField] private PerkDatabase perkDatabase;
    
    private HashSet<string> unlockedPerks = new HashSet<string>();
    private Dictionary<string, Perk> allPerks = new Dictionary<string, Perk>();

    void Awake()
    {
        InitializePerks();
    }

    void InitializePerks()
    {
        if (perkDatabase != null)
        {
            foreach (Perk perk in perkDatabase.perks)
            {
                allPerks[perk.id] = perk;
            }
        }
        else
        {
            // Create default perks if no database is assigned
            CreateDefaultPerks();
        }
    }

    void CreateDefaultPerks()
    {
        List<Perk> defaultPerks = new List<Perk>();
        
        // Resource Management Perks (Single level)
        defaultPerks.Add(new Perk("fuel_capacity_1", "Fuel Tank I", "Increase fuel capacity by 25%", PerkType.FuelCapacity, 1, 5, 25f));
        defaultPerks.Add(new Perk("fuel_capacity_2", "Fuel Tank II", "Increase fuel capacity by 50%", PerkType.FuelCapacity, 1, 10, 50f));
        defaultPerks.Add(new Perk("fuel_efficiency_1", "Efficient Boosting I", "Reduce fuel consumption by 25%", PerkType.BoostEfficiency, 1, 8, 0.25f));
        defaultPerks.Add(new Perk("fuel_efficiency_2", "Efficient Boosting II", "Reduce fuel consumption by 50%", PerkType.BoostEfficiency, 1, 15, 0.5f));
        
        defaultPerks.Add(new Perk("magnet_range_1", "Magnetic Field I", "Increase magnet radius by 2 units", PerkType.MagnetRadius, 5, 6, 2f));
        defaultPerks.Add(new Perk("magnet_range_2", "Magnetic Field II", "Increase magnet radius by 4 units", PerkType.MagnetRadius, 10, 12, 4f));
        defaultPerks.Add(new Perk("magnet_duration_1", "Extended Magnetism I", "Increase magnet duration by 2 seconds", PerkType.MagnetDuration, 5, 7, 2f));
        defaultPerks.Add(new Perk("magnet_duration_2", "Extended Magnetism II", "Increase magnet duration by 4 seconds", PerkType.MagnetDuration, 10, 14, 4f));
        
        // Weapon Unlocks
        defaultPerks.Add(new Perk("unlock_rocket", "Rocket Launcher", "Unlock the Rocket Launcher weapon", PerkType.WeaponUnlock, 5, 15, 0f, PlayerWeaponType.Rocket));
        defaultPerks.Add(new Perk("unlock_laser", "Laser Cannon", "Unlock the Laser Cannon weapon", PerkType.WeaponUnlock, 10, 20, 0f, PlayerWeaponType.Laser));
        defaultPerks.Add(new Perk("unlock_plasma", "Plasma Rifle", "Unlock the Plasma Rifle weapon", PerkType.WeaponUnlock, 15, 25, 0f, PlayerWeaponType.Plasma));
        defaultPerks.Add(new Perk("unlock_railgun", "Railgun", "Unlock the Railgun weapon", PerkType.WeaponUnlock, 20, 30, 0f, PlayerWeaponType.Railgun));
        
        // Game Modifier Perks
        defaultPerks.Add(new Perk("spawn_reduction_1", "Crowd Control I", "Reduce enemy spawn rate by 15%", PerkType.SpawnRateReduction, 10, 16, 0.15f));
        defaultPerks.Add(new Perk("spawn_reduction_2", "Crowd Control II", "Reduce enemy spawn rate by 25%", PerkType.SpawnRateReduction, 20, 25, 0.25f));

        foreach (Perk perk in defaultPerks)
        {
            allPerks[perk.id] = perk;
        }
    }

    public List<Perk> GetAvailablePerks(int playerLevel)
    {
        List<Perk> available = new List<Perk>();
        
        foreach (Perk perk in allPerks.Values)
        {
            if (playerLevel >= perk.requiredLevel && !IsUnlocked(perk.id))
            {
                available.Add(perk);
            }
        }
        
        return available;
    }

    public bool IsPerkUnlocked(string perkId)
    {
        return unlockedPerks.Contains(perkId);
    }

    public bool IsUnlocked(string perkId)
    {
        return unlockedPerks.Contains(perkId);
    }

    public void UnlockPerk(string perkId)
    {
        if (!unlockedPerks.Contains(perkId))
        {
            unlockedPerks.Add(perkId);
            Debug.Log($"Perk unlocked: {perkId}");
        }
    }

    public Perk GetPerk(string perkId)
    {
        return allPerks.TryGetValue(perkId, out Perk perk) ? perk : null;
    }

    public List<Perk> GetUnlockedPerks()
    {
        List<Perk> unlocked = new List<Perk>();
        foreach (string perkId in unlockedPerks)
        {
            if (allPerks.TryGetValue(perkId, out Perk perk))
            {
                unlocked.Add(perk);
            }
        }
        return unlocked;
    }

    // Save/Load system for perks
    public void SavePerks()
    {
        string perksData = string.Join(",", unlockedPerks);
        PlayerPrefs.SetString("UnlockedPerks", perksData);
        PlayerPrefs.Save();
    }

    public void LoadPerks()
    {
        string perksData = PlayerPrefs.GetString("UnlockedPerks", "");
        if (!string.IsNullOrEmpty(perksData))
        {
            string[] perkIds = perksData.Split(',');
            unlockedPerks.Clear();
            foreach (string perkId in perkIds)
            {
                if (!string.IsNullOrEmpty(perkId))
                {
                    unlockedPerks.Add(perkId);
                }
            }
        }
    }

    public void ResetPerks()
    {
        unlockedPerks.Clear();
        PlayerPrefs.DeleteKey("UnlockedPerks");
    }
}

[System.Serializable]
public class Perk
{
    public string id;
    public string name;
    public string description;
    public PerkType type;
    public int requiredLevel;
    public int cost;
    public float value;
    public PlayerWeaponType weaponType; // Used for weapon unlock perks

    public Perk(string id, string name, string description, PerkType type, int requiredLevel, int cost, float value, PlayerWeaponType weaponType = PlayerWeaponType.Default)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.type = type;
        this.requiredLevel = requiredLevel;
        this.cost = cost;
        this.value = value;
        this.weaponType = weaponType;
    }
}

[System.Serializable]
public enum PerkType
{
    // Resource management perks
    FuelCapacity,
    MagnetRadius,
    MagnetDuration,
    BoostEfficiency,
    WeaponUnlock,
    ShieldCapacity,
    SpawnRateReduction
}

[CreateAssetMenu(fileName = "New Perk Database", menuName = "Game/Perk Database")]
public class PerkDatabase : ScriptableObject
{
    public Perk[] perks;
}