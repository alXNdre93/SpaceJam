using UnityEngine;

public class UpgradeCard : MonoBehaviour
{
    [SerializeField] private bool isLegendary;
    [SerializeField] private string description;
    [SerializeField] private float multiplierDamage;
    [SerializeField] private float multiplierSpeed;
    [SerializeField] private float multiplierPoint;
    [SerializeField] private float mulitplierSpawnRate;
    [SerializeField] private float multiplierHealth;
    [SerializeField] private UpgradeCardType cardType;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void OnSelect()
    {
        
    }
}
