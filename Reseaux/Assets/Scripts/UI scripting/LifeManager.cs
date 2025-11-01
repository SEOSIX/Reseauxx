using System;
using TMPro;
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
}