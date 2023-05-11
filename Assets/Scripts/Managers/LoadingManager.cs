using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LoadingManager : NetworkBehaviour
{

    [SerializeField] GameStatusSO gameStatusSO;
    [SerializeField] GameObject loadingScreenPanel;
    [SerializeField] GameObject windowsPanel;
    [SerializeField] GameObject androidPanel;
    [SerializeField] TextMeshProUGUI countdownText;
    public NetworkVariable<bool> networkedClientsReady = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> networkedCountdownFinished = new NetworkVariable<bool>(false);


    private void ServerStart()
    {
        StartCoroutine(WaitForPlayersReady());
        GameManager.Singleton.StartGameManagerServer();
    }

    public void Start()
    {
        if (IsServer) ServerStart();
        networkedClientsReady.OnValueChanged += OnClientsReady;
        networkedCountdownFinished.OnValueChanged += OnCountdownFinished;
        if (gameStatusSO.isWindows)
        {
            windowsPanel.SetActive(true);
            androidPanel.SetActive(false);
        }
        else if (gameStatusSO.isAndroid)
        {
            androidPanel.SetActive(true);
            windowsPanel.SetActive(false);
        }
    }

    private void OnCountdownFinished(bool previousValue, bool newValue)
    {
        loadingScreenPanel.SetActive(false);
        androidPanel.SetActive(false);
        windowsPanel.SetActive(false);

        PlayerBehaviour playerBehaviour = PlayerBehaviour.LocalInstance;
        playerBehaviour.ControlActive(true);
    }


    private void OnClientsReady(bool prev, bool newValue)
    {
        StartCoroutine(StartCountdown());
    }


    public IEnumerator WaitForPlayersReady()
    {

        yield return new WaitForSeconds(1f); // Wait  to make sure all players have time to read the instructions 
        while (NetworkManager.Singleton.ConnectedClientsList.Count != gameStatusSO.lobbyPlayers.Count)
        {
            yield return new WaitForSeconds(1f);
        }

        networkedClientsReady.Value = true;
    }



    private IEnumerator StartCountdown()
    {
        float countdownDuration = 3f;
        float remainingTime = countdownDuration;

        while (remainingTime > 0f)
        {
            countdownText.text = Mathf.CeilToInt(remainingTime).ToString();
            yield return new WaitForSeconds(1f);
            remainingTime -= 1f;
        }

        if (IsServer) networkedCountdownFinished.Value = true;
    }


}
