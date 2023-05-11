using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LoadingManager : NetworkBehaviour
{

    [SerializeField] GameStatusSO gameStatusSO;
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
    }

    private void OnCountdownFinished(bool previousValue, bool newValue)
    {
        Debug.LogWarning("ON COUNTDOWN FINISHED");
        PlayerBehaviour playerBehaviour = PlayerBehaviour.LocalInstance;
        if (playerBehaviour == null) { Debug.LogWarning("PLAYER BEHAVIOUR IS NULL"); return; }
        playerBehaviour.ControlActive(true);
    }


    private void OnClientsReady(bool prev, bool newValue)
    {
        StartCoroutine(StartCountdown());

    }


    public IEnumerator WaitForPlayersReady()
    {

        yield return new WaitForSeconds(3f); // Wait  to make sure all players have time to read the wizzard instructions 
        while (NetworkManager.Singleton.ConnectedClientsList.Count != gameStatusSO.lobbyPlayers.Count)
        {
            yield return new WaitForSeconds(1f);
        }
        Debug.LogWarning("ALL PLAYERS CONNECTED");

        networkedClientsReady.Value = true;
    }



    private IEnumerator StartCountdown()
    {
        float countdownDuration = 3f;
        float remainingTime = countdownDuration;

        while (remainingTime > 0f)
        {
            //countdownText.text = Mathf.CeilToInt(remainingTime).ToString();
            yield return new WaitForSeconds(1f);
            remainingTime -= 1f;
        }

        if (IsServer) networkedCountdownFinished.Value = true;
    }


}
