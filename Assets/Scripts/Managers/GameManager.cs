using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

class GameManager : NetworkBehaviour
{

    [SerializeField] GameStatusSO gamestatus;
    [SerializeField] GameObject infoTextGO;
    [SerializeField] TextMeshProUGUI infoText;

    public static GameManager Singleton;
    public NetworkVariable<int> networkedPlayerIdHasRing = new NetworkVariable<int>(-1);
    public NetworkVariable<bool> networkedGameWon = new NetworkVariable<bool>(false);


    private void Awake()
    {
        if (Singleton == null) Singleton = this;
        else Destroy(gameObject);
    }



    private void Start()
    {
        networkedPlayerIdHasRing.OnValueChanged += OnPlayerIdHasRingChangedClientRpc;
        networkedGameWon.OnValueChanged += OnGameWonChangedClientRpc;
        SpawnAll();
    }

    private void SpawnAll()
    {
        if (!IsServer) return;
        Debug.Log("SPAWNALL");
        SpawnManager.Singleton.SpawnAllPrefabs();
        SpawnManager.Singleton.SpawnAllPlayers();
    }


    [ClientRpc]
    private void OnGameWonChangedClientRpc(bool previousValue, bool newValue)
    {
        Debug.LogWarning("OngameWonChangedClientRpc");
        infoTextGO.SetActive(true);
        infoText.text = "GAME WON BY PLAYER with id: " + networkedPlayerIdHasRing.Value.ToString();
    }

    [ClientRpc]
    private void OnPlayerIdHasRingChangedClientRpc(int previousValue, int newValue)
    {
        Debug.LogWarning("ONPLAYERIDHASRINGCHANGED");
        infoTextGO.SetActive(true);
        infoText.text = "A player has collected the ring!";
    }



    [ServerRpc]
    public void OnPlayerCollectedRingServerRpc(int playerId)
    {
        networkedPlayerIdHasRing.Value = playerId;
    }




    [ServerRpc]
    public void OnGameWonServerRpc()
    {
        networkedGameWon.Value = true;
    }


    public void OnPlayerDown() { }
    public void OnPlayerDead() { }



}
