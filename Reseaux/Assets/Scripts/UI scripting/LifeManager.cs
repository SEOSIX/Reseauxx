using System;
using TMPro;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using UnityEngine.UI;

public class LifeManager : MonoBehaviour
{
    public static LifeManager instance { get; private set; }
    
    public int lifeValue;
    public Slider playerSlider;
    [SerializeField] private TextMeshProUGUI lifeText;


    private void Awake()
    {
        instance = this;
        lifeValue = (int)playerSlider.value;
    }

    private void Update()
    {
        lifeValue = (int)playerSlider.value;
    }


    public void SetLife(int value)
    {
        lifeValue = value;
        if (playerSlider != null)
        {
            playerSlider.value = lifeValue;
            playerSlider.maxValue = lifeValue;
        }
        if (lifeText != null)
            lifeText.text = $"{lifeValue} / {playerSlider.maxValue}";
    }

    public void CheckLifePlayer()
    {
        if (lifeValue <= Mathf.Abs(0f))
        {
            PlayerManager.hidders--;
        }
    }
}