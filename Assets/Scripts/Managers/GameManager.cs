using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

class GameManager : NetworkBehaviour
{

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

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
        }
    }

    private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        networkedPlayerIdHasRing.OnValueChanged += OnPlayerIdHasRingChangedClientRpc;
        networkedGameWon.OnValueChanged += OnGameWonChangedClientRpc;
        SpawnManager.Singleton.SpawnAll();
    }

    public void EndGameScene()
    {
        if (IsClient)
        {
            NetworkManager.Singleton.Shutdown();
            DestroyObjectsTaggedWithDontDestroy();
            SceneManager.LoadScene("Menus");
        }
    }

    private void DestroyObjectsTaggedWithDontDestroy()
    {
        GameObject[] objs = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in objs)
        {
            if (obj.transform.parent == null && obj.transform != transform && obj.scene.name != null)
            {
                if (obj.CompareTag("DontDestroy")) Destroy(obj);
                Debug.LogWarning("Destroyed: " + obj.name);
            }
        }
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
        infoTextGO.SetActive(true);
        infoText.text = "A player has collected the ring!";
    }



    [ServerRpc]
    public void OnPlayerCollectedRingServerRpc(int playerId)
    {
        networkedPlayerIdHasRing.Value = playerId;
    }

    public void EndGameSession()
    {

    }




    [ServerRpc]
    public void OnGameWonServerRpc()
    {
        networkedGameWon.Value = true;
    }


    public void OnPlayerDown() { }
    public void OnPlayerDead() { }



}
