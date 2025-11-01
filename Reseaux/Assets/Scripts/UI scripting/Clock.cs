using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Clock : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    
    private NetworkVariable<float> serverTime = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private bool isRunning = false;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            serverTime.Value = 0f;
            isRunning = true;
        }
        serverTime.OnValueChanged += OnTimeUpdated;
    }

    private void OnDestroy()
    {
        serverTime.OnValueChanged -= OnTimeUpdated;
    }

    void Update()
    {
        if (!IsServer || !isRunning) return;

        serverTime.Value += Time.deltaTime;
    }

    private void OnTimeUpdated(float oldValue, float newValue)
    {
        int minutes = Mathf.FloorToInt(newValue / 60);
        int seconds = Mathf.FloorToInt(newValue % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    [ServerRpc(RequireOwnership = false)]
    public void StopTimerServerRpc()
    {
        isRunning = false;
    }
}