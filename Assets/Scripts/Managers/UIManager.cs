using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Slider playerSliderHealth;
    [SerializeField] private UnityEngine.UI.Slider enemysliderHealth;
    [SerializeField] private TMP_Text txtScore, txtMultiply, txtFireRate, txtHighScore;
    [SerializeField] GameObject nuke1, nuke2, nuke3, machineGunTimerUI, machineGunTimerPlayer, menuCanvas, normalModeOverText;
    [SerializeField] Camera cam;
    private Player player;
    private ScoreManager scoreManager;

    private void Start()
    {
        scoreManager = GameManager.GetInstance().scoreManager;
        GameManager.GetInstance().OnGameStart += GameStarted;
        GameManager.GetInstance().OnGameOver += GameOver;
    }


    public void GameStarted()
    {
        player = GameManager.GetInstance().Getplayer();
        player.OnHealthUpdate += UpdateHealth;
        menuCanvas.SetActive(false);
    }

    public void GameOver()
    {
        menuCanvas.SetActive(true);
    }

    public void BossSpawned(Enemy boss)
    {
        boss.OnBossHealthUpdate += UpdateEnemyHealth;
    }

    private void OnDisable()
    {
        player.OnHealthUpdate -= UpdateHealth;
    }

    public void UpdateHighScore()
    {
        txtHighScore.SetText("High Score: " + scoreManager.GetHighScore().ToString());
    }

    private void OnGUI()
    {
        if (machineGunTimerUI.activeSelf && player != null)
        {
            machineGunTimerPlayer.transform.position = cam.WorldToScreenPoint(player.transform.position);
            machineGunTimerPlayer.transform.rotation = player.transform.rotation;
        }
    }

    public void UpdateHealth(float currentHealth)
    {
        playerSliderHealth.value = currentHealth;
    }

    public void UpdateEnemyHealth(float currentHealth)
    {
        enemysliderHealth.value = currentHealth;
    }

    public void UpdateNukes(int nukeAvalaibale)
    {
        nuke1.SetActive(false);
        nuke2.SetActive(false);
        nuke3.SetActive(false);
        if (nukeAvalaibale == 1)
        {
            nuke1.SetActive(true);
            nuke2.SetActive(false);
            nuke3.SetActive(false);
        }
        else if (nukeAvalaibale == 2)
        {
            nuke1.SetActive(true);
            nuke2.SetActive(true);
            nuke3.SetActive(false);
        }
        else if (nukeAvalaibale == 3)
        {
            nuke1.SetActive(true);
            nuke2.SetActive(true);
            nuke3.SetActive(true);
        }
    }

    public void UpdateScore()
    {
        txtScore.SetText(GameManager.GetInstance().scoreManager.GetScore().ToString());
    }

    public void UpdateAugments()
    {
        txtFireRate.SetText("Fire Rate: " + (Mathf.Floor(1 / player.fireRate * 100f) / 100f).ToString() + "/s");
        txtMultiply.SetText("Damage: " + (1 + player.multiplyShot).ToString() + "x");
    }

    public void MachineGunTimer()
    {
        machineGunTimerUI.SetActive(!machineGunTimerUI.activeSelf);
    }

    public void UpdateBossSprite(Sprite bossSprite)
    {
        enemysliderHealth.gameObject.transform.GetChild(enemysliderHealth.gameObject.transform.childCount - 1).GetComponent<UnityEngine.UI.Image>().sprite = bossSprite;
        enemysliderHealth.gameObject.transform.GetChild(enemysliderHealth.gameObject.transform.childCount - 2).GetComponent<UnityEngine.UI.Image>().sprite = bossSprite;
    }

    public void ToggleBossHealth()
    {
        enemysliderHealth.gameObject.SetActive(!enemysliderHealth.gameObject.activeSelf);
    }

    public void SetBossMaxHealth(float maxHealth)
    {
        Debug.Log("bOSS mAX hEALTH "+maxHealth.ToString());
        enemysliderHealth.maxValue = maxHealth;
        enemysliderHealth.value = maxHealth;
    }

    public void SetPlayerMaxHealth(float maxHealth)
    {
        playerSliderHealth.maxValue = maxHealth;
    }

    public void ShowNormalModeOver()
    {
        normalModeOverText.SetActive(true);
        float currentTimeScale = Time.timeScale;
        Time.timeScale = 0;
        while (!Input.anyKey) { Debug.Log("Waiting For input"); }

        Time.timeScale = currentTimeScale;
        normalModeOverText.SetActive(false);
    }
}
