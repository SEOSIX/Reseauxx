using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Clock : MonoBehaviour
{
    public static Clock instance { get; private set; }

    [SerializeField] private TextMeshProUGUI timerText;
    private float elapsedTime = 0f;
    private bool isRunning = false;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (!isRunning) return;

        elapsedTime += Time.deltaTime;
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    
    public void UpdateTimer(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
    
    public void StartClock()
    {
        isRunning = true;
    }

    public void StopClock()
    {
        isRunning = false;
    }

    public void ResetClock()
    {
        elapsedTime = 0f;
        isRunning = false;
        UpdateTimerDisplay();
    }

    public float GetElapsedTime()
    {
        return elapsedTime;
    }
    
    public void SetElapsedTime(float time)
    {
        elapsedTime = time;
        UpdateTimerDisplay();
    }
}