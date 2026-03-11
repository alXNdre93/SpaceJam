using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventSystem : MonoBehaviour
{
    [Header("Event UI")]
    [SerializeField] private GameObject eventNotificationPanel;
    [SerializeField] private TMP_Text eventTitleText;
    [SerializeField] private TMP_Text eventDescriptionText;
    [SerializeField] private TMP_Text eventTimerText;
    [SerializeField] private Image eventProgressBar;

    [Header("Event Prefabs")]
    [SerializeField] private GameObject asteroidWarningPrefab;
    [SerializeField] private GameObject[] asteroidProjectilePrefabs;
    [SerializeField] private GameObject asteroidImpactPrefab;

    [Header("Event Settings")]
    [SerializeField] private float eventChance = 0.1f;
    [SerializeField] private float minEventInterval = 30f;
    [SerializeField] private float maxEventInterval = 90f;

    private GameManager gameManager;
    private Player player;
    private bool isEventActive = false;
    private float eventTimer = 0f;
    private float lastEventTime = 0f;
    private EventType currentEventType;

    void Start()
    {
        Debug.Log($"EventSystem.Start() - UI References: Panel={eventNotificationPanel != null}, Title={eventTitleText != null}, Timer={eventTimerText != null}, Progress={eventProgressBar != null}");
        gameManager = GameManager.GetInstance();
        if (gameManager != null)
        {
            gameManager.OnGameStart += OnGameStart; 
            gameManager.OnGameOver += OnGameOver;
        }
    }

    void OnGameStart()
    {
        player = gameManager.Getplayer();
        lastEventTime = Time.time;
    }

    void OnGameOver()
    {
        StopAllEvents();
    }

    void Update()
    {
        if (!gameManager.IsPlaying()) return;

        // Check for new events only if no event is active
        if (!isEventActive)
        {
            // Check if enough time has passed since last event
            if (Time.time - lastEventTime >= minEventInterval)
            {
                // Random chance to trigger an event
                if (Random.Range(0f, 1f) < eventChance * Time.deltaTime)
                {
                    TriggerRandomEvent();
                }
            }
        }
        
        // Update event timer if event is active
        if (isEventActive && eventTimer > 0)
        {
            eventTimer -= Time.deltaTime;
            UpdateEventUI();
            
            if (eventTimer <= 0)
            {
                EndCurrentEvent();
            }
        }
    }

    public void TriggerRandomEvent()
    {
        if (isEventActive) return;

        // Choose random event type
        EventType[] eventTypes = System.Enum.GetValues(typeof(EventType)) as EventType[];
        EventType randomEvent = eventTypes[Random.Range(0, eventTypes.Length)];
        
        TriggerEvent(randomEvent);
    }

    public void TriggerEvent(EventType eventType)
    {
        if (isEventActive) return;

        currentEventType = eventType;
        isEventActive = true;
        lastEventTime = Time.time;

        switch (eventType)
        {
            case EventType.TemporalStorm:
                StartTemporalStorm();
                break;
            case EventType.AsteroidStorm:
                StartAsteroidStorm();
                break;
            case EventType.ReinforcementWave:
                StartReinforcementWave();
                break;
        }

        ShowEventNotification();
        Debug.Log($"Event Started: {eventType}");
        
        // Auto-hide event notification after 2 seconds
        StartCoroutine(HideEventNotificationAfterDelay(2f));
    }

    void StartTemporalStorm()
    {
        eventTimer = 20f;
        
        // Increase enemy speed significantly
        gameManager.multiplierEnemySpeed *= 1.75f;
        
        // Slightly increase spawn rate
        gameManager.mulitplierSpawnRate *= 1.25f;

        if (eventTitleText != null)
            eventTitleText.SetText("TEMPORAL STORM");
        
        if (eventDescriptionText != null)
            eventDescriptionText.SetText("Time flows faster for enemies! +75% enemy speed, +25% spawn rate");
    }

    void StartAsteroidStorm()
    {
        eventTimer = 30f;
        
        // Start spawning asteroid hazards
        StartCoroutine(SpawnAsteroidHazards());

        if (eventTitleText != null)
            eventTitleText.SetText("ASTEROID STORM");
        
        if (eventDescriptionText != null)
            eventDescriptionText.SetText("Asteroid hazards rain from space! Avoid the danger zones");
    }

    void StartReinforcementWave()
    {
        eventTimer = 15f;
        
        // Dramatically increase spawn rate
        gameManager.mulitplierSpawnRate *= 2.5f;
        
        // Slightly reduce enemy health to compensate
        gameManager.multiplierEnemyHealth *= 0.8f;

        if (eventTitleText != null)
            eventTitleText.SetText("REINFORCEMENT WAVE");
        
        if (eventDescriptionText != null)
            eventDescriptionText.SetText("Enemy reinforcements incoming! +150% spawn rate, -20% enemy health");
    }

    IEnumerator SpawnAsteroidHazards()
    {
        while (isEventActive && currentEventType == EventType.AsteroidStorm)
        {
            yield return new WaitForSeconds(Random.Range(2f, 5f));
            
            // Spawn asteroid warning zones
            Vector2 randomPos = new Vector2(
                Random.Range(-8f, 8f),
                Random.Range(-4f, 4f)
            );
            
            StartCoroutine(CreateAsteroidHazard(randomPos));
        }
    }

    IEnumerator CreateAsteroidHazard(Vector2 position)
    {
        // Create warning indicator using prefab
        GameObject warning = null;
        if (asteroidWarningPrefab != null)
        {
            warning = Instantiate(asteroidWarningPrefab, position, Quaternion.identity);
        }
        else
        {
            // Fallback to dynamic creation if no prefab assigned
            warning = new GameObject("AsteroidWarning");
            warning.transform.position = position;
            SpriteRenderer warningSprite = warning.AddComponent<SpriteRenderer>();
            warningSprite.color = Color.red;
        }
        
        // Warning phase (2 seconds) - spawn falling asteroid
        float warningTime = 2f;
        SpriteRenderer spriteRenderer = warning.GetComponent<SpriteRenderer>();
        Color originalColor = spriteRenderer != null ? spriteRenderer.color : Color.red;
        
        // Spawn asteroid projectile above screen
        GameObject asteroid = null;
        AsteroidHazard asteroidHazard = null;
        if (asteroidProjectilePrefabs != null && asteroidProjectilePrefabs.Length > 0)
        {
            // Randomly select an asteroid prefab
            GameObject randomAsteroidPrefab = asteroidProjectilePrefabs[Random.Range(0, asteroidProjectilePrefabs.Length)];
            if (randomAsteroidPrefab != null)
            {
                Vector2 startPos = new Vector2(position.x, position.y + 10f); // Start above target
                asteroid = Instantiate(randomAsteroidPrefab, startPos, Quaternion.identity);
                
                // Add AsteroidHazard component if it doesn't exist
                asteroidHazard = asteroid.GetComponent<AsteroidHazard>();
                if (asteroidHazard == null)
                {
                    asteroidHazard = asteroid.AddComponent<AsteroidHazard>();
                }
                
                // Set as moving asteroid toward the target position
                asteroidHazard.SetAsMoving(startPos, null);
                
                // Add movement toward target
                StartCoroutine(MoveAsteroidToTarget(asteroid, startPos, position, warningTime));
            }
        }
        
        while (warningTime > 0)
        {
            warningTime -= Time.deltaTime;
            // Flash warning
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 
                    Mathf.PingPong(Time.time * 3f, 1f));
            }
            yield return null;
        }
        
        // Clean up warning (asteroid will handle its own impact and cleanup)
        if (warning != null)
            Destroy(warning);
            
        // Create impact effect using prefab at target position
        if (asteroidImpactPrefab != null)
        {
            GameObject impact = Instantiate(asteroidImpactPrefab, position, Quaternion.identity);
            // Destroy impact effect after a short time (adjust based on your effect duration)
            Destroy(impact, 2f);
        }
    }
    
    public void CreateImpactEffect(Vector2 position)
    {
        // Public method for AsteroidHazard to create impact effects
        if (asteroidImpactPrefab != null)
        {
            GameObject impact = Instantiate(asteroidImpactPrefab, position, Quaternion.identity);
            Destroy(impact, 2f);
        }
    }

    IEnumerator MoveAsteroidToTarget(GameObject asteroid, Vector2 startPos, Vector2 targetPos, float duration)
    {
        if (asteroid == null) yield break;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < duration && asteroid != null)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            
            // Move asteroid from start to target position
            asteroid.transform.position = Vector2.Lerp(startPos, targetPos, progress);
            
            // Optional: add rotation for visual effect
            asteroid.transform.Rotate(0, 0, 180f * Time.deltaTime);
            
            yield return null;
        }
    }

    void EndCurrentEvent()
    {
        if (!isEventActive) return;

        // Revert event effects
        switch (currentEventType)
        {
            case EventType.TemporalStorm:
                // Revert speed and spawn rate changes
                gameManager.multiplierEnemySpeed /= 1.75f;
                gameManager.mulitplierSpawnRate /= 1.25f;
                break;
            case EventType.AsteroidStorm:
                // Stop asteroid spawning (handled by coroutine check)
                break;
            case EventType.ReinforcementWave:
                // Revert spawn rate and health changes
                gameManager.mulitplierSpawnRate /= 2.5f;
                gameManager.multiplierEnemyHealth /= 0.8f;
                break;
        }

        // Award event completion rewards
        GiveEventRewards();

        isEventActive = false;
        HideEventNotification();
        
        Debug.Log($"Event Ended: {currentEventType}");
    }

    void GiveEventRewards()
    {
        if (player == null) return;

        // Base rewards
        int bonusXP = 50;
        int bonusAptitude = 1;

        // Scale rewards with difficulty
        float difficultyMultiplier = Mathf.Max(1f, gameManager.multiplierEnemyHealth);
        bonusXP = Mathf.RoundToInt(bonusXP * difficultyMultiplier);

        // Give rewards
        gameManager.pickupManager.SpawnExperiencePickup(player.transform.position, bonusXP);
        player.AddAptitudePoints(bonusAptitude);

        Debug.Log($"Event rewards: {bonusXP} XP, {bonusAptitude} Aptitude Point");
    }

    void ShowEventNotification()
    {
        Debug.Log($"ShowEventNotification() called - Panel exists: {eventNotificationPanel != null}");
        
        if (eventNotificationPanel == null)
        {
            Debug.LogError("Event notification panel is null! Please assign it in the EventSystem Inspector.");
            Debug.LogError("EventSystem UI References missing: Panel={eventNotificationPanel != null}, Title={eventTitleText != null}, Timer={eventTimerText != null}, Progress={eventProgressBar != null}");
            return;
        }
        
        // Check and activate parent hierarchy if needed
        Transform parent = eventNotificationPanel.transform.parent;
        while (parent != null)
        {
            Debug.Log($"Checking parent: {parent.name} - Active: {parent.gameObject.activeInHierarchy}");
            if (!parent.gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"Event notification parent '{parent.name}' is not active! Activating it.");
                parent.gameObject.SetActive(true);
            }
            parent = parent.parent;
        }
        
        // Ensure the panel itself is enabled
        if (!eventNotificationPanel.gameObject.activeInHierarchy)
        {
            eventNotificationPanel.SetActive(true);
        }
        
        Debug.Log($"Event notification panel activated. Active in hierarchy: {eventNotificationPanel.activeInHierarchy}");
        
        // Force update the UI immediately
        UpdateEventUI();
    }

    void HideEventNotification()
    {
        Debug.Log($"HideEventNotification() called - Panel exists: {eventNotificationPanel != null}");
        if (eventNotificationPanel != null)
            eventNotificationPanel.SetActive(false);
    }

    void UpdateEventUI()
    {
        if (eventTimerText != null)
        {
            eventTimerText.SetText($"{eventTimer:F1}s");
        }
        else
        {
            Debug.LogWarning("Event timer text is null! Check Inspector assignment.");
        }

        if (eventProgressBar != null)
        {
            float maxTime = GetEventMaxTime();
            float fillAmount = eventTimer / maxTime;
            eventProgressBar.fillAmount = fillAmount;
            
            // Check if progress bar is actually visible
            if (!eventProgressBar.gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"Event progress bar exists but is not active in hierarchy! Parent active: {eventProgressBar.transform.parent?.gameObject.activeInHierarchy}");
            }
            
            Debug.Log($"Event progress bar updated: fill={fillAmount:F2}, timer={eventTimer:F1}/{maxTime:F1}, visible={eventProgressBar.gameObject.activeInHierarchy}");
        }
        else
        {
            Debug.LogError("Event progress bar is NULL! Please assign it in EventSystem Inspector.");
        }
        
        // Update title and description if they exist
        if (eventTitleText == null)
        {
            Debug.LogWarning("Event title text is null! Check Inspector assignment.");
        }
        
        if (eventDescriptionText == null)
        {
            Debug.LogWarning("Event description text is null! Check Inspector assignment.");
        }
    }

    float GetEventMaxTime()
    {
        switch (currentEventType)
        {
            case EventType.TemporalStorm:
                return 20f;
            case EventType.AsteroidStorm:
                return 30f;
            case EventType.ReinforcementWave:
                return 15f;
            default:
                return 20f;
        }
    }

    public void StopAllEvents()
    {
        if (isEventActive)
        {
            EndCurrentEvent();
        }
    }

    public bool IsEventActive()
    {
        return isEventActive;
    }

    public EventType GetCurrentEventType()
    {
        return currentEventType;
    }
    
    IEnumerator HideEventNotificationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (eventNotificationPanel != null)
        {
            eventNotificationPanel.SetActive(false);
            Debug.Log("Event notification panel auto-hidden after delay");
        }
    }
}

public enum EventType
{
    TemporalStorm,
    AsteroidStorm,
    ReinforcementWave
}