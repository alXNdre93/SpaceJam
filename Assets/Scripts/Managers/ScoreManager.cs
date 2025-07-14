using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem.Interactions;
using System;

public class ScoreManager : MonoBehaviour
{
    private int seconds;
    private int score;
    private int highScore = 0;
    public UnityEvent OnScoreUpdate;
    public UnityEvent OnHighScoreUpdated;

    public Action<float> onScoreChange;

    private void Start()
    {
        //PlayerPrefs.SetInt("HighScore",0);
        highScore = PlayerPrefs.GetInt("HighScore");
        GameManager.GetInstance().OnGameStart += OnGameStart;
    }

    public void OnGameStart()
    {
        OnHighScoreUpdated?.Invoke();
        score = 0;
        GameManager.GetInstance().uIManager.UpdateScore();
    }

    public int GetHighScore()
    {
        return highScore;
    }

    public void SetHighScore()
    {
        PlayerPrefs.SetInt("HighScore", highScore);
        GameManager.GetInstance().uIManager.UpdateHighScore();
    }

    public string timer
    {
        get
        {
            return (Mathf.Round((float)seconds / 60.0f) + "mins and " + seconds % 60 + " seconds");
        }
        private set { }
    }

    public int GetScore()
    {
        return score;
    }

    public void IncrementScore(int value = 1)
    {
        score += value;
        OnScoreUpdate?.Invoke();
        onScoreChange(score);
        if (score > highScore)
        {
            highScore = score;
            Debug.Log("New HighScore is: " + highScore.ToString());
            OnHighScoreUpdated?.Invoke();
        }
    }
}
