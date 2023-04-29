using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : NetworkBehaviour
{

    [SerializeField] PlayersSO playersSO;
    TextMeshProUGUI countdownText;
    GameObject loadingScreen;
    public NetworkVariable<bool> networkedGameReadyToStart = new NetworkVariable<bool>(false);

    void Start()
    {
        Debug.Log("LOADING SCRIPT:");
        Debug.Log("IsServer:" + IsServer);
        Debug.Log("IsClient:" + IsClient);
        Debug.Log("IsLocalPlayer:" + IsLocalPlayer);
        Debug.Log("IsSpawned:" + IsSpawned);
        Debug.Log("IsHost:" + IsHost);
        Debug.Log("IsOwner:" + IsOwner);
        Debug.Log("IsOwnedByServer:" + IsOwnedByServer);

        loadingScreen = GameObject.Find("LoadingScreenCanvas");
        GameObject countdownTextGO = GameObject.FindWithTag("CountdownText");
        countdownText = countdownTextGO.GetComponent<TextMeshProUGUI>();

        networkedGameReadyToStart.OnValueChanged += OnGameReadyToStartChangedClientRpc;
        if (IsServer) StartCoroutine(WaitForPlayersReady());
    }

    [ClientRpc]
    private void OnGameReadyToStartChangedClientRpc(bool previousValue, bool newValue)
    {
        StartCoroutine(StartCountdown());
    }

    public IEnumerator WaitForPlayersReady()
    {
        Debug.LogWarning("METHOD: WAIT FOR PLAYERS READY");
        yield return new WaitForSeconds(1f); // Wait for 5 seconds to make sure all players have time to read the loading screen

        while (NetworkManager.Singleton.ConnectedClientsList.Count != playersSO.nPlayers)
        {
            yield return new WaitForSeconds(1f);
        }
        networkedGameReadyToStart.Value = true;
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
        loadingScreen.SetActive(false);
    }

}
