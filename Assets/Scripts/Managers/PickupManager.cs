using System.Collections.Generic;
using UnityEngine;

public class PickupManager : MonoBehaviour
{
    [SerializeField] private PickupSpawn[] pickups;
    [SerializeField] private ExperiencePickup experiencePickupPrefab;

    [Range(0,1)]
    [SerializeField] private float pickupProbability;

    List<Pickup> pickupPool = new List<Pickup>();
    Pickup chosenPickup; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (PickupSpawn spawn in pickups){
            for (int i = 0; i < spawn.spawnWeight; i++){
                pickupPool.Add(spawn.pickup);
            }
        }
    }

    public void SpawnPickup(Vector2 position)
    {
        if (pickupPool.Count <= 0)
            return;
        if (Random.Range(0.0f, 0.1f) < pickupProbability){
            chosenPickup = pickupPool[Random.Range(0, pickupPool.Count)];
            
            // Use pool manager if available, otherwise fallback to instantiate
            if (PoolManager.Instance != null)
            {
                Pickup pooledPickup = PoolManager.Instance.GetPickup(chosenPickup);
                if (pooledPickup != null)
                {
                    pooledPickup.transform.position = position;
                    pooledPickup.transform.rotation = Quaternion.identity;
                }
                else
                {
                    Instantiate(chosenPickup, position, Quaternion.identity);
                    Debug.LogWarning("Pickup pool returned null, falling back to Instantiate");
                }
            }
            else
            {
                Instantiate(chosenPickup, position, Quaternion.identity);
                Debug.LogWarning("PoolManager not found, falling back to Instantiate for pickup");
            }
        }
        pickupProbability = Random.Range(0.0f,0.1f);
    }
    
    public void SpawnExperiencePickup(Vector2 position, int xpValue)
    {
        if (experiencePickupPrefab == null)
        {
            Debug.LogWarning("Experience pickup prefab not assigned!");
            return;
        }
        
        ExperiencePickup xpPickup = Instantiate(experiencePickupPrefab, position, Quaternion.identity);
        xpPickup.Initialize(xpValue);
    }
}

[System.Serializable]
public struct PickupSpawn
{
    public Pickup pickup;
    public int spawnWeight;

}
