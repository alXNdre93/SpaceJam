using System.Collections.Generic;
using System.Security;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class UpgradeCardManager : MonoBehaviour
{
    [SerializeField] private List<UpgradeCard> playerUpgradeCards;
    [SerializeField] private List<UpgradeCard> ennemyUpgradeCards;
    [SerializeField] private Image legendaryBack;
    [SerializeField] private Image normalBack;
    [SerializeField] private Image speedFront;
    [SerializeField] private Image damageFront;
    [SerializeField] private Image pointFront;
    [SerializeField] private Image spawnFront;
    [SerializeField] private Image healthFront;

    [SerializeField] private GameObject Card1Back, Card2Back, Card3Back;
    [SerializeField] private GameObject Card1Front, Card2Front, Card3Front;
    [SerializeField] private GameObject Card1FrontColor, Card2FrontColor, Card3FrontColor;
    [SerializeField] private GameObject Card1TypeIcon, Card2TypeIcon, Card3TypeIcon;
    [SerializeField] private GameObject Card1TypeText, Card2TypeText, Card3TypeText;
    [SerializeField] private GameObject Card1PercentText, Card2PercentText, Card3PercentText;
    [SerializeField] private GameObject Card1Description, Card2Description, Card3Description;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}

public enum UpgradeCardType
{
    Speed,
    Damage,
    Point,
    Spawn,
    Health
}