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
    [SerializeField] private PropMorpher propMorpher;
    [SerializeField] private Canvas canvasPlayer;
    
    private bool isDead = false;

    private void Awake()
    {
        instance = this;
        propMorpher.enabled = true;
        canvasPlayer.enabled = true;
        
    }

    public void SetMaxLife(int max)
    {
        if (playerSlider != null)
            playerSlider.maxValue = max;
    }

    public void SetLife(int value)
    {
        lifeValue = value;

        if (playerSlider != null)
            playerSlider.value = lifeValue;

        if (lifeText != null)
            lifeText.text = $"{lifeValue} / {playerSlider.maxValue}";
        if (!isDead && lifeValue <= 0)
            HandlePlayerDeath();
    }

    private void HandlePlayerDeath()
    {
        isDead = true;
        var playerNet = gameObject.GetComponent<PlayerNetwork>();
        PlayerManager.hidders -= 1;
        propMorpher.enabled = false;
        canvasPlayer.enabled = false;
        playerNet.DestroyColliderServerRpc();
        NetwordkSetup.instance.CheckNumberHidder();
    }
}