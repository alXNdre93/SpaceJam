using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Slider sliderHealth;
    [SerializeField] private TMP_Text txtScore, txtMultiply, txtFireRate, txtHighScore;
    [SerializeField] GameObject nuke1, nuke2, nuke3, machineGunTimerUI, machineGunTimerPlayer, menuCanvas;
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

    public void UpdateHealth(float CurrentHealth)
    {
        sliderHealth.value = CurrentHealth;
    }

    public void UpdateNukes(int nukeAvalaibale){
        nuke1.SetActive(false);
        nuke2.SetActive(false);
        nuke3.SetActive(false);
        if (nukeAvalaibale == 1){
            nuke1.SetActive(true);
            nuke2.SetActive(false);
            nuke3.SetActive(false);
        }else if (nukeAvalaibale == 2){
            nuke1.SetActive(true);
            nuke2.SetActive(true);
            nuke3.SetActive(false);           
        }else if (nukeAvalaibale == 3){
            nuke1.SetActive(true);
            nuke2.SetActive(true);
            nuke3.SetActive(true);
        }
    }

    public void UpdateScore()
    {
        txtScore.SetText(GameManager.GetInstance().scoreManager.GetScore().ToString());
    }

    public void UpdateAugments(){
        txtFireRate.SetText("Fire Rate: " + (Mathf.Floor(1 / player.fireRate*100f)/100f).ToString() + "/s");
        txtMultiply.SetText("Damage: " + (1 + player.multiplyShot).ToString() + "x");
    }

    public void MachineGunTimer(){
        machineGunTimerUI.SetActive(!machineGunTimerUI.activeSelf);
    }
}
