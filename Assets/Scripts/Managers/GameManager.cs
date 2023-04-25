using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

class GameManager : NetworkBehaviour
{

    [SerializeField] ShortcutManager shortcutManager;
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
        if (gamestatus.isShortcutUsed)
        {
            try
            {

                ShortcutManager.Singleton.gameObject.SetActive(true);
            }
            catch { }
        }

        else
        {
            try
            {

                ShortcutManager.Singleton.gameObject.SetActive(false);
            }
            catch { }

            SpawnAll();
        }

        networkedPlayerIdHasRing.OnValueChanged += OnPlayerIdHasRingChanged;
        networkedGameWon.OnValueChanged += OnGameWonChanged;
    }

    private void OnGameWonChanged(bool previousValue, bool newValue)
    {
        infoTextGO.SetActive(true);
        infoText.text = "GAME WON BY PLAYER with id: " + networkedPlayerIdHasRing.Value.ToString();
    }

    private void OnPlayerIdHasRingChanged(int previousValue, int newValue)
    {
        infoText.text = "A player has collected the ring!";
        infoTextGO.SetActive(true);
    }

    private void SpawnAll()
    {
        Debug.Log("SPAWNALL");
        SpawnManager.Singleton.SpawnAllPrefabs();
        SpawnManager.Singleton.SpawnAllPlayers();
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
