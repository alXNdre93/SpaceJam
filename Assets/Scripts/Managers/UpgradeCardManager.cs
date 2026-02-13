using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeCardManager : MonoBehaviour
{
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

    [SerializeField] private GameObject Card1Back;
    [SerializeField] private GameObject Card2Back;
    [SerializeField] private GameObject Card3Back;
    [SerializeField] private GameObject Card1Front;
    [SerializeField] private GameObject Card2Front;
    [SerializeField] private GameObject Card3Front;
    [SerializeField] private Image Card1FrontColor;
    [SerializeField] private Image Card2FrontColor;
    [SerializeField] private Image Card3FrontColor;
    [SerializeField] private Image Card1TypeIcon,Card1TypeIcon2;
    [SerializeField] private Image Card2TypeIcon,Card2TypeIcon2;
    [SerializeField] private Image Card3TypeIcon,Card3TypeIcon2;
    [SerializeField] private TMP_Text Card1TypeText;
    [SerializeField] private TMP_Text Card2TypeText;
    [SerializeField] private TMP_Text Card3TypeText;
    [SerializeField] private TMP_Text Card1PercentText;
    [SerializeField] private TMP_Text Card2PercentText;
    [SerializeField] private TMP_Text Card3PercentText;
    [SerializeField] private TMP_Text Card1Description;
    [SerializeField] private TMP_Text Card2Description;
    [SerializeField] private TMP_Text Card3Description;

    [SerializeField] private float normalMinValue = 0.01f;
    [SerializeField] private float normalMaxValue = 0.2f;
    [SerializeField] private float legendaryMinValue = 0.25f;
    [SerializeField] private float legendaryMaxValue = 0.75f;

    private float previousTimeScale = 1f;

    private CardData card1Data, card2Data, card3Data;
    private bool currentIsPlayer;
    private Action onComplete;
    private float originalTimeScale = 1f; // Save the original timescale before any pause
    private int playerSelectionsRemaining = 0; // Track how many more selections the player needs to make
    private bool card1Selected = false, card2Selected = false, card3Selected = false; // Track which cards are selected

    // Public API: show cards for player (isPlayer=true) or enemies (isPlayer=false).
    public void ShowCards(bool isPlayer, Action onComplete = null)
    {
        Debug.Log($"UpgradeCardManager.ShowCards() called: isPlayer={isPlayer}, onComplete={onComplete}");
        this.onComplete = onComplete;
        currentIsPlayer = isPlayer;
        
        // Reset selected card flags when starting new phase
        card1Selected = false;
        card2Selected = false;
        card3Selected = false;
        
        // Re-enable all cards in case some were hidden during player selection
        if (Card1Back != null) Card1Back.SetActive(true);
        if (Card1Front != null) Card1Front.SetActive(true);
        if (Card2Back != null) Card2Back.SetActive(true);
        if (Card2Front != null) Card2Front.SetActive(true);
        if (Card3Back != null) Card3Back.SetActive(true);
        if (Card3Front != null) Card3Front.SetActive(true);
        
        // Set up selection counter: player needs 2 selections, enemies need 1
        playerSelectionsRemaining = isPlayer ? 2 : 1;
        Debug.Log($"UpgradeCardManager.ShowCards(): playerSelectionsRemaining set to {playerSelectionsRemaining}");
        
        // ensure UI and this manager are active so cards are visible and coroutines can run
        gameObject.SetActive(true);
        enabled = true;
        Debug.Log($"UpgradeCardManager.ShowCards(): Manager activated. gameObject.activeInHierarchy={gameObject.activeInHierarchy}, enabled={enabled}");
        
        // Only save the original timescale on the first call (player selection)
        if (isPlayer)
        {
            originalTimeScale = Time.timeScale;
            Debug.Log($"UpgradeCardManager.ShowCards(): Saved original timescale: {originalTimeScale}");
        }
        
        Time.timeScale = 0f; // pause game
        CreateCards(isPlayer);
        Debug.Log($"UpgradeCardManager.ShowCards(): isPlayer={isPlayer} card1={card1Data.upgrades.Count} upgrades, card2={card2Data.upgrades.Count} upgrades, card3={card3Data.upgrades.Count} upgrades");
        StartCoroutine(ShowBackCards());
    }

    private void CreateCards(bool isPlayer)
    {
        card1Data = GenerateCard(isPlayer);
        ApplyCardUI(card1Data, Card1Back, Card1FrontColor, Card1TypeIcon, Card1TypeIcon2, Card1TypeText, Card1PercentText, Card1Description, isPlayer);

        card2Data = GenerateCard(isPlayer);
        ApplyCardUI(card2Data, Card2Back, Card2FrontColor, Card2TypeIcon, Card2TypeIcon2, Card2TypeText, Card2PercentText, Card2Description, isPlayer);

        card3Data = GenerateCard(isPlayer);
        ApplyCardUI(card3Data, Card3Back, Card3FrontColor, Card3TypeIcon, Card3TypeIcon2, Card3TypeText, Card3PercentText, Card3Description, isPlayer);
    }

    private CardData GenerateCard(bool isPlayer)
    {
        CardData cardData = new CardData();
        bool isLegendary = UnityEngine.Random.Range(0, 100) > 95;
        
        if (isLegendary)
        {
            // Legendary: always 2 buffs
            cardData.upgrades.Add(GenerateUpgrade(true, 0.25f, 0.75f));
            cardData.upgrades.Add(GenerateUpgradeDifferentType(cardData.upgrades[0].type, true, 0.25f, 0.75f));
            cardData.isLegendary = true;
        }
        else
        {
            // Normal card: either 2 small buffs OR 1 medium buff + 1 debuff
            if (UnityEngine.Random.Range(0, 100) > 50)
            {
                // 2 small buffs
                cardData.upgrades.Add(GenerateUpgrade(true, 0.01f, 0.15f));
                cardData.upgrades.Add(GenerateUpgradeDifferentType(cardData.upgrades[0].type, true, 0.01f, 0.15f));
            }
            else
            {
                // 1 medium buff + 1 debuff
                cardData.upgrades.Add(GenerateUpgrade(true, 0.1f, 0.2f));
                cardData.upgrades.Add(GenerateUpgradeDifferentType(cardData.upgrades[0].type, false, 0.1f, 0.2f));
            }
            cardData.isLegendary = false;
        }
        
        return cardData;
    }

    private CardUpgrade GenerateUpgrade(bool isBuff, float minValue, float maxValue)
    {
        UpgradeCardType type = RandomType();
        float baseValue = UnityEngine.Random.Range(minValue, maxValue);
        float value = isBuff ? baseValue : -baseValue;
        
        return new CardUpgrade { type = type, value = value };
    }

    private CardUpgrade GenerateUpgradeDifferentType(UpgradeCardType excludeType, bool isBuff, float minValue, float maxValue)
    {
        UpgradeCardType type;
        do
        {
            type = RandomType();
        } while (type == excludeType);
        
        float baseValue = UnityEngine.Random.Range(minValue, maxValue);
        float value = isBuff ? baseValue : -baseValue;
        
        return new CardUpgrade { type = type, value = value };
    }

    private UpgradeCardType RandomType()
    {
        int count = Enum.GetValues(typeof(UpgradeCardType)).Length;
        return (UpgradeCardType)UnityEngine.Random.Range(0, count);
    }

    private void ApplyCardUI(CardData cardData, GameObject backObj, Image frontColor, Image typeIcon, Image typeIcon2, TMP_Text typeText, TMP_Text percentText, TMP_Text description, bool isPlayer)
    {
        if (cardData.upgrades.Count == 0)
            return;

        // Build title and description
        string titleText = "";
        string descText = "";
        Sprite firstIcon = GetIconForType(cardData.upgrades[0].type);
        frontColor.sprite = GetFrontSpriteForType(cardData.upgrades[0].type);

        if (cardData.upgrades.Count == 1)
        {
            // Single upgrade - original behavior
            CardUpgrade upgrade = cardData.upgrades[0];
            titleText = GetTypeString(upgrade.type);
            descText = BuildDescription(upgrade.type, upgrade.value, isPlayer);
            
            if (typeIcon != null) typeIcon.sprite = GetIconForType(upgrade.type);
            if (typeIcon2 != null) typeIcon2.sprite = GetIconForType(upgrade.type);
            
            if (percentText != null)
            {
                string sign = GetSign(upgrade.type, upgrade.value, isPlayer);
                percentText.SetText(sign + Mathf.Ceil(Mathf.Abs(upgrade.value) * 100).ToString() + "%");
            }
        }
        else if (cardData.upgrades.Count == 2)
        {
            // Multiple upgrades
            CardUpgrade upgrade1 = cardData.upgrades[0];
            CardUpgrade upgrade2 = cardData.upgrades[1];
            
            // Title: type1 / type2
            titleText = GetTypeString(upgrade1.type) + " / " + GetTypeString(upgrade2.type);
            
            // Description explains both upgrades
            string desc1 = BuildDescriptionPart(upgrade1.type, upgrade1.value, isPlayer);
            string desc2 = BuildDescriptionPart(upgrade2.type, upgrade2.value, isPlayer);
            descText = desc1 + " and " + desc2;
            
            // Icons - set both icons with the appropriate types
            if (typeIcon != null) typeIcon.sprite = GetIconForType(upgrade1.type);
            if (typeIcon2 != null)
            {
                // If both upgrades are the same type, show the same icon in both slots
                if (upgrade1.type == upgrade2.type)
                {
                    typeIcon2.sprite = GetIconForType(upgrade1.type);
                }
                else
                {
                    typeIcon2.sprite = GetIconForType(upgrade2.type);
                }
                typeIcon2.gameObject.SetActive(true);
            }
            
            // Percent text: show both percentages
            if (percentText != null)
            {
                string sign1 = GetSign(upgrade1.type, upgrade1.value, isPlayer);
                string sign2 = GetSign(upgrade2.type, upgrade2.value, isPlayer);
                string percent1 = sign1 + Mathf.Ceil(Mathf.Abs(upgrade1.value) * 100).ToString() + "%";
                string percent2 = sign2 + Mathf.Ceil(Mathf.Abs(upgrade2.value) * 100).ToString() + "%";
                
                // If same percentage, show once, otherwise show both
                if (percent1 == percent2)
                {
                    percentText.SetText(percent1);
                }
                else
                {
                    percentText.SetText(percent1 + " / " + percent2);
                }
            }
        }

        if (typeText != null) typeText.SetText(titleText);
        if (description != null) description.SetText(descText);

        // Set card background based on if legendary
        if (backObj != null)
        {
            var img = backObj.GetComponent<Image>();
            if (img != null) img.sprite = cardData.isLegendary ? legendaryBack : normalBack;
        }
    }

    private string GetTypeString(UpgradeCardType type)
    {
        return type switch
        {
            UpgradeCardType.Speed => "Speed",
            UpgradeCardType.Damage => "Damage",
            UpgradeCardType.Point => "Points",
            UpgradeCardType.Spawn => "Spawn Rate",
            UpgradeCardType.Health => "Health",
            _ => "Unknown"
        };
    }

    private Sprite GetIconForType(UpgradeCardType type)
    {
        return type switch
        {
            UpgradeCardType.Speed => speedIcon,
            UpgradeCardType.Damage => damageIcon,
            UpgradeCardType.Point => pointIcon,
            UpgradeCardType.Spawn => spawnIcon,
            UpgradeCardType.Health => healthIcon,
            _ => pointIcon
        };
    }

    private Sprite GetFrontSpriteForType(UpgradeCardType type)
    {
        return type switch
        {
            UpgradeCardType.Speed => speedFront,
            UpgradeCardType.Damage => damageFront,
            UpgradeCardType.Point => pointFront,
            UpgradeCardType.Spawn => spawnFront,
            UpgradeCardType.Health => healthFront,
            _ => pointFront
        };
    }

    private string BuildDescription(UpgradeCardType type, float value, bool isPlayer)
    {
        if (type == UpgradeCardType.Point || type == UpgradeCardType.Spawn)
            return "Change " + GetTypeString(type).ToLower() + " value by";

        bool isDebuff = value < 0;
        string subject = "";
        if (isPlayer && !isDebuff)
            subject = "Player by";
        else if (isPlayer && isDebuff)
            subject = "Enemy by";
        else if (!isPlayer && !isDebuff)
            subject = "Enemy by";
        else // !isPlayer && isDebuff
            subject = "Player by";
        
        string action = isPlayer && !isDebuff || !isPlayer && !isDebuff ? "Upgrade" : "Reduce";
        return action + " the " + GetTypeString(type).ToLower() + " of " + subject;
    }

    private string BuildDescriptionPart(UpgradeCardType type, float value, bool isPlayer)
    {
        if (type == UpgradeCardType.Point)
            return "change points value";
        if (type == UpgradeCardType.Spawn)
            return "change spawn rate";

        bool isBuff = value > 0;
        string target = "";
        if (isPlayer && isBuff)
            target = "upgrade Player";
        else if (isPlayer && !isBuff)
            target = "reduce Enemy";
        else if (!isPlayer && isBuff)
            target = "upgrade Enemy";
        else // !isPlayer && !isBuff
            target = "reduce Player";
        
        return target + " " + GetTypeString(type).ToLower();
    }

    private string GetSign(UpgradeCardType type, float value, bool isPlayer)
    {
        // Determine the sign based on type, value, and who's choosing
        if (type == UpgradeCardType.Point)
        {
            return isPlayer ? "+" : "-";
        }
        if (type == UpgradeCardType.Spawn)
        {
            return isPlayer ? "-" : "+";
        }
        
        return value >= 0 ? "+" : "-";
    }

    private IEnumerator ShowBackCards()
    {
        Debug.Log($"UpgradeCardManager.ShowBackCards(): starting coroutine for isPlayer={currentIsPlayer}");
        
        // Initialize: front cards hidden (0,90,0), back cards visible (0,0,0)
        if (Card1Front != null) Card1Front.transform.localRotation = Quaternion.Euler(0, 90, 0);
        if (Card2Front != null) Card2Front.transform.localRotation = Quaternion.Euler(0, 90, 0);
        if (Card3Front != null) Card3Front.transform.localRotation = Quaternion.Euler(0, 90, 0);
        
        if (Card1Back != null)
        {
            Card1Back.transform.localRotation = Quaternion.Euler(0, 0, 0); // Show back
            // Back card button reveals the front
            Button btn1 = Card1Back.GetComponent<Button>();
            if (btn1 == null) btn1 = Card1Back.AddComponent<Button>();
            btn1.onClick.RemoveAllListeners();
            btn1.onClick.AddListener(() => RevealCard(1, Card1Back, Card1Front));
        }
        if (Card2Back != null)
        {
            Card2Back.transform.localRotation = Quaternion.Euler(0, 0, 0); // Show back
            Button btn2 = Card2Back.GetComponent<Button>();
            if (btn2 == null) btn2 = Card2Back.AddComponent<Button>();
            btn2.onClick.RemoveAllListeners();
            btn2.onClick.AddListener(() => RevealCard(2, Card2Back, Card2Front));
        }
        if (Card3Back != null)
        {
            Card3Back.transform.localRotation = Quaternion.Euler(0, 0, 0); // Show back
            Button btn3 = Card3Back.GetComponent<Button>();
            if (btn3 == null) btn3 = Card3Back.AddComponent<Button>();
            btn3.onClick.RemoveAllListeners();
            btn3.onClick.AddListener(() => RevealCard(3, Card3Back, Card3Front));
        }

        Debug.Log($"UpgradeCardManager.ShowBackCards(): cards initialized and buttons assigned");
        yield return new WaitForSecondsRealtime(1f);
        Debug.Log($"UpgradeCardManager.ShowBackCards(): waiting for selection...");
        // remain paused until player selects a card
    }

    private void RevealCard(int cardIndex, GameObject back, GameObject front)
    {
        StartCoroutine(RevealCardCoroutine(cardIndex, back, front));
    }

    private IEnumerator RevealCardCoroutine(int cardIndex, GameObject back, GameObject front)
    {
        // Smoothly animate the flip using unscaled time so it's responsive while game is paused.
        float duration = 0.25f; // shorter, snappier reveal
        float t = 0f;

        Quaternion backStart = back != null ? back.transform.localRotation : Quaternion.Euler(0, 0, 0);
        Quaternion backEnd = Quaternion.Euler(0, 90, 0);
        Quaternion frontStart = Quaternion.Euler(0, 90, 0);
        Quaternion frontEnd = Quaternion.Euler(0, 0, 0);

        // If front exists, ensure it starts hidden
        if (front != null) front.transform.localRotation = frontStart;

        while (t < duration)
        {
            float ratio = t / duration;
            if (back != null) back.transform.localRotation = Quaternion.Slerp(backStart, backEnd, ratio);
            if (front != null) front.transform.localRotation = Quaternion.Slerp(frontStart, frontEnd, ratio);
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        if (back != null) back.transform.localRotation = backEnd;
        if (front != null) front.transform.localRotation = frontEnd;

        // Add button to front card to select the upgrade
        if (front != null)
        {
            Button btnFront = front.GetComponent<Button>();
            if (btnFront == null) btnFront = front.AddComponent<Button>();
            btnFront.onClick.RemoveAllListeners();
            btnFront.onClick.AddListener(() => SelectCard(cardIndex));
        }
    }

    private void ResetCardRotations()
    {
        // Reset all card rotations back to 0,90,0 for next selection
        if (Card1Back != null) Card1Back.transform.localRotation = Quaternion.Euler(0, 90, 0);
        if (Card2Back != null) Card2Back.transform.localRotation = Quaternion.Euler(0, 90, 0);
        if (Card3Back != null) Card3Back.transform.localRotation = Quaternion.Euler(0, 90, 0);
        if (Card1Front != null) Card1Front.transform.localRotation = Quaternion.Euler(0, 90, 0);
        if (Card2Front != null) Card2Front.transform.localRotation = Quaternion.Euler(0, 90, 0);
        if (Card3Front != null) Card3Front.transform.localRotation = Quaternion.Euler(0, 90, 0);
    }

    // UI button hooks - now only called from front cards
    public void SelectCard1() => SelectCard(1);
    public void SelectCard2() => SelectCard(2);
    public void SelectCard3() => SelectCard(3);

    private void SelectCard(int cardIndex)
    {
        StartCoroutine(HandleSelection(cardIndex));
    }

    private IEnumerator HandleSelection(int cardIndex)
    {
        CardData selectedCardData = null;

        switch (cardIndex)
        {
            case 1:
                selectedCardData = card1Data;
                card1Selected = true;
                Debug.Log($"UpgradeCardManager.HandleSelection(): selected card1 with {selectedCardData.upgrades.Count} upgrade(s), isPlayer={currentIsPlayer}");
                break;
            case 2:
                selectedCardData = card2Data;
                card2Selected = true;
                Debug.Log($"UpgradeCardManager.HandleSelection(): selected card2 with {selectedCardData.upgrades.Count} upgrade(s), isPlayer={currentIsPlayer}");
                break;
            case 3:
                selectedCardData = card3Data;
                card3Selected = true;
                Debug.Log($"UpgradeCardManager.HandleSelection(): selected card3 with {selectedCardData.upgrades.Count} upgrade(s), isPlayer={currentIsPlayer}");
                break;
        }

        ApplyCard(selectedCardData);
        yield return new WaitForSecondsRealtime(0.1f);

        // Decrement selections remaining counter
        playerSelectionsRemaining--;
        Debug.Log($"UpgradeCardManager.HandleSelection(): playerSelectionsRemaining now = {playerSelectionsRemaining}");

        // Hide the selected card by moving it off screen or disabling it
        HideCard(cardIndex);
        yield return new WaitForSecondsRealtime(0.2f);
        
        // Check if we need more selections or move to next phase
        if (playerSelectionsRemaining > 0)
        {
            // Still need more selections, make sure remaining cards stay visible and clickable
            Debug.Log($"UpgradeCardManager.HandleSelection(): waiting for next selection (remaining: {playerSelectionsRemaining})");
            // Re-enable buttons on remaining cards to ensure they're still clickable
            EnsureRemainingCardsClickable(cardIndex);
        }
        else
        {
            // All selections for this phase complete
            Action callback = onComplete;
            bool isFinalSelection = (callback == null);
            
            Time.timeScale = originalTimeScale;
            Debug.Log($"UpgradeCardManager.HandleSelection(): all selections complete. isFinalSelection={isFinalSelection}. Restored timescale to {originalTimeScale}. Calling callback...");
            
            yield return new WaitForSecondsRealtime(0.1f);
            
            // Hide all remaining cards before ending
            HideAllCards();
            
            yield return new WaitForSecondsRealtime(0.1f);
            
            // Invoke callback BEFORE checking for deactivation
            // This ensures ShowCards(false, null) is called while manager is still active
            if (callback != null)
            {
                Debug.Log("UpgradeCardManager.HandleSelection(): invoking onComplete callback for next round");
                callback.Invoke();
            }
            
            yield return new WaitForSecondsRealtime(0.1f);
            
            // Only deactivate if this is the final selection (enemy cards, callback was null)
            if (isFinalSelection && enabled)
            {
                enabled = false;
                Debug.Log("UpgradeCardManager.HandleSelection(): final selection complete, deactivating UI");
            }
        }
        yield break;
    }

    private void HideCard(int cardIndex)
    {
        // Disable the card container so it's not visible or clickable
        switch (cardIndex)
        {
            case 1:
                if (Card1Back != null) Card1Back.SetActive(false);
                if (Card1Front != null) Card1Front.SetActive(false);
                break;
            case 2:
                if (Card2Back != null) Card2Back.SetActive(false);
                if (Card2Front != null) Card2Front.SetActive(false);
                break;
            case 3:
                if (Card3Back != null) Card3Back.SetActive(false);
                if (Card3Front != null) Card3Front.SetActive(false);
                break;
        }
    }

    private void HideAllCards()
    {
        // Hide all cards - used when a phase is complete
        if (Card1Back != null) Card1Back.SetActive(false);
        if (Card1Front != null) Card1Front.SetActive(false);
        if (Card2Back != null) Card2Back.SetActive(false);
        if (Card2Front != null) Card2Front.SetActive(false);
        if (Card3Back != null) Card3Back.SetActive(false);
        if (Card3Front != null) Card3Front.SetActive(false);
    }

    private void EnsureRemainingCardsClickable(int selectedCardIndex)
    {
        // Make sure the remaining cards' fronts are revealed and clickable
        // Automatically reveal any unrevealed cards so player can click them directly
        if (selectedCardIndex != 1)
        {
            RevealCardFront(1, Card1Back, Card1Front);
        }
        if (selectedCardIndex != 2)
        {
            RevealCardFront(2, Card2Back, Card2Front);
        }
        if (selectedCardIndex != 3)
        {
            RevealCardFront(3, Card3Back, Card3Front);
        }
    }

    private void RevealCardFront(int cardIndex, GameObject back, GameObject front)
    {
        // Check if the front is already revealed (rotation is not hidden)
        if (front != null)
        {
            Quaternion frontRotation = front.transform.localRotation;
            Quaternion hiddenRotation = Quaternion.Euler(0, 90, 0);
            
            // If front is hidden, reveal it
            if (Quaternion.Angle(frontRotation, hiddenRotation) < 1f)
            {
                // Front is hidden, so reveal it
                if (back != null) back.transform.localRotation = Quaternion.Euler(0, 90, 0);
                front.transform.localRotation = Quaternion.Euler(0, 0, 0);
                
                // Add button to front card to select the upgrade
                Button btnFront = front.GetComponent<Button>();
                if (btnFront == null) btnFront = front.AddComponent<Button>();
                btnFront.onClick.RemoveAllListeners();
                btnFront.onClick.AddListener(() => SelectCard(cardIndex));
            }
            else
            {
                // Front is already revealed, just ensure button is set up
                Button btnFront = front.GetComponent<Button>();
                if (btnFront != null) btnFront.interactable = true;
            }
        }
    }

    private void ApplyCard(CardData cardData)
    {
        foreach (CardUpgrade upgrade in cardData.upgrades)
        {
            if (currentIsPlayer)
                GameManager.GetInstance().ApplyPlayerUpgrade(upgrade.type, upgrade.value);
            else
                GameManager.GetInstance().ApplyEnemyUpgrade(upgrade.type, upgrade.value);
        }
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
[System.Serializable]
public class CardUpgrade
{
    public UpgradeCardType type;
    public float value; // positive = buff, negative = debuff
}

[System.Serializable]
public class CardData
{
    public System.Collections.Generic.List<CardUpgrade> upgrades = new System.Collections.Generic.List<CardUpgrade>();
    public bool isLegendary;
}