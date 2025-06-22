using System.Collections.Generic;
using UnityEngine;

public class PickupManager : MonoBehaviour
{
    [SerializeField] private PickupSpawn[] pickups;

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
            Instantiate(chosenPickup, position, Quaternion.identity);
        }
        pickupProbability = Random.Range(0.0f,0.1f);
    }
}

[System.Serializable]
public struct PickupSpawn
{
    public Pickup pickup;
    public int spawnWeight;

}
