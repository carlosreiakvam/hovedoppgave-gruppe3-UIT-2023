using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreenManager : NetworkBehaviour
{

    [SerializeField] PlayersSO playersSO;
    TextMeshProUGUI countdownText;
    GameObject loadingScreen;
    public NetworkVariable<bool> networkedGameReady = new NetworkVariable<bool>(false);


    private void ServerStart()
    {
        StartCoroutine(WaitForPlayersReady());
    }

    public void Start()
    {
        if (IsServer) ServerStart();
        loadingScreen = GameObject.Find("LoadingScreenCanvas");
        GameObject countdownTextGO = GameObject.FindWithTag("CountdownText");
        countdownText = countdownTextGO.GetComponent<TextMeshProUGUI>();

        networkedGameReady.OnValueChanged += OnGameReady;
    }


    private void OnGameReady(bool prev, bool neww)
    {
        OnGameReadyChangedClientRpc(prev, neww);

    }

    [ClientRpc]
    private void OnGameReadyChangedClientRpc(bool previousValue, bool newValue)
    {
        if (newValue == true)
        {
            StartCoroutine(StartCountdown());

            // unsubscribing from networkedGameReady
            networkedGameReady.OnValueChanged -= OnGameReadyChangedClientRpc;
        }
    }

    public IEnumerator WaitForPlayersReady()
    {
        yield return new WaitForSeconds(1f); // Wait for 1 seconds to make sure all players have time to read the loading screen

        Debug.LogWarning("WAITING FOR PLAYERS TO CONNECT");
        while (NetworkManager.Singleton.ConnectedClientsList.Count != playersSO.nPlayers)
        {
            yield return new WaitForSeconds(1f);
        }
        Debug.LogWarning("ALL PLAYERS CONNECTED");

        if (GameManager.Singleton.StartGameManager()) networkedGameReady.Value = true;
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
        StartGame();
    }

    private void StartGame()
    {
        Debug.LogWarning("REVEALING GAME SCENE");
        loadingScreen.SetActive(false);
    }

}
