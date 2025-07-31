using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class UpgradeCardManager : MonoBehaviour
{
    private UpgradeCard upgradeCard1;
    private UpgradeCard upgradeCard2;
    private UpgradeCard upgradeCard3;
    [SerializeField] private Sprite legendaryBack;
    [SerializeField] private Sprite normalBack;
    [SerializeField] private Sprite speedFront;
    [SerializeField] private Sprite damageFront;
    [SerializeField] private Sprite pointFront;
    [SerializeField] private Sprite spawnFront;
    [SerializeField] private Sprite healthFront;
    [SerializeField] private Sprite speedIcon;
    [SerializeField] private Sprite damageIcon;
    [SerializeField] private Sprite pointIcon;
    [SerializeField] private Sprite spawnIcon;
    [SerializeField] private Sprite healthIcon;

    [SerializeField] private GameObject Card1Back, Card2Back, Card3Back;
    [SerializeField] private GameObject Card1Front, Card2Front, Card3Front;
    [SerializeField] private Image Card1FrontColor, Card2FrontColor, Card3FrontColor;
    [SerializeField] private Image Card1TypeIcon, Card2TypeIcon, Card3TypeIcon;
    [SerializeField] private TMP_Text Card1TypeText, Card2TypeText, Card3TypeText;
    [SerializeField] private TMP_Text Card1PercentText, Card2PercentText, Card3PercentText;
    [SerializeField] private TMP_Text Card1Description, Card2Description, Card3Description;

    [SerializeField] private float normalMinValue = 0.01f;
    [SerializeField] private float normalMaxValue = 0.2f;
    [SerializeField] private float legendaryMinValue = 0.25f;
    [SerializeField] private float legendaryMaxValue = 0.75f;


    public void ShowCards(bool isPlayer)
    {
        float currentTimeScale = Time.timeScale;
        Time.timeScale = 0;
        CreateCards(isPlayer);
        StartCoroutine(ShowBackCards());

        Time.timeScale = currentTimeScale;
    }

    private void CreateCards(bool isPlayer) {
        UpgradeCardType upgradeTypeCard1 = (UpgradeCardType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(UpgradeCardType)).Length);
        switch (upgradeTypeCard1)
        {
            case UpgradeCardType.Speed:
                Card1FrontColor.sprite = speedFront;
                Card1TypeIcon.sprite = speedIcon;
                Card1TypeText.SetText("Speed Multiplier");
                break;
            case UpgradeCardType.Damage:
                break;
            case UpgradeCardType.Point:
                break;
            case UpgradeCardType.Spawn:
                break;
            case UpgradeCardType.Health:
                break;
            default:
            break;
        }
        UpgradeCardType upgradeTypeCard2 = (UpgradeCardType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(UpgradeCardType)).Length);
        UpgradeCardType upgradeTypeCard3 = (UpgradeCardType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(UpgradeCardType)).Length);
        if (isPlayer)
        {

        }

    }
    IEnumerator ShowBackCards()
    {
        Card1Back.transform.Rotate(new Vector3(0,90,0));
        Card2Back.transform.Rotate(new Vector3(0,90,0));
        Card3Back.transform.Rotate(new Vector3(0,90,0));
        yield return new WaitForSeconds(1);
    }

    IEnumerator ShowFrontCard1()
    {
        Card1Back.transform.Rotate(new Vector3(0, 90, 0));
        yield return new WaitForSeconds(1);
        Card1Front.transform.Rotate(new Vector3(0,90,0));
    }

    IEnumerator ShowFrontCard2()
    {
        Card2Back.transform.Rotate(new Vector3(0, 90, 0));
        yield return new WaitForSeconds(1);
        Card3Front.transform.Rotate(new Vector3(0,90,0));
    }

    IEnumerator ShowFrontCard3()
    {
        Card3Back.transform.Rotate(new Vector3(0, 90, 0));
        yield return new WaitForSeconds(1);
        Card3Front.transform.Rotate(new Vector3(0,90,0));
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