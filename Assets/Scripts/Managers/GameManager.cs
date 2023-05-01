using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{

    [SerializeField] GameObject infoTextGO;
    [SerializeField] TextMeshProUGUI infoText;

    public static GameManager Singleton;
    public ulong networkedPlayerIdHasRing { get; set; }
    public NetworkVariable<bool> networkedGameWon = new NetworkVariable<bool>(false);


    private void Awake()
    {
        if (Singleton == null) { Singleton = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log("GAMEMANAGER SPAWNED");
    }

    public bool StartGameManager()
    {
        //networkedPlayerIdHasRing.OnValueChanged += OnPlayerIdHasRingChangedClientRpc;
        networkedGameWon.OnValueChanged += OnGameWonChangedClientRpc;

        try
        {
            Debug.LogWarning("GAMEMANAGER STARTED");
            SubscribeToNetworkVariables();
            bool spawnSuccess = SpawnManager.Singleton.SpawnAll();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return false;
        }
        return true;
    }

    private void SubscribeToNetworkVariables()
    {
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

    // Might be redundant
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
        //        infoText.text = "GAME WON BY PLAYER with id: " + networkedPlayerIdHasRing.Value.ToString();
    }

    [ClientRpc]
    private void OnPlayerIdHasRingChangedClientRpc()
    {
        Debug.LogWarning("CLIENT KNOWS THAT RING HAS CHANGED");
        infoTextGO.SetActive(true);
        infoText.text = "A player has collected the ring!";
    }


    internal void OnPlayerCollectedRing(ulong playerNetworkObjectId)
    {

        //networkedPlayerIdHasRing.Value = playerNetworkObjectId;
    }




    [ServerRpc]
    public void OnGameWonServerRpc()
    {
        networkedGameWon.Value = true;
    }

}
