using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{

    [SerializeField] GameObject gameUI;
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
            bool spawnSuccess = SpawnManager.Singleton.SpawnAll();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return false;
        }
        return true;
    }

    public void EndGameScene()
    {
        try
        {
            NetworkManager.Singleton.Shutdown();
            DestroyObjectsTaggedWithDontDestroy();
            SceneManager.LoadScene("Menus");
        }
        catch (Exception e)
        {
            Debug.LogError(e); return;
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
        gameUI.SetActive(true);
        //        infoText.text = "GAME WON BY PLAYER with id: " + networkedPlayerIdHasRing.Value.ToString();
    }

    public void OnRingChangedOwner(ulong networkedPlayerIdHasRing)
    {
        this.networkedPlayerIdHasRing = networkedPlayerIdHasRing;
        gameUI.SetActive(true);
        infoText.text = "A new player has collected the ring!";
    }

    public void OnGameWon(ulong networkedPlayerIdHasRing)
    {
        Debug.LogWarning("OnGameWon");
        gameUI.SetActive(true);
        infoText.text = "GAME WON BY PLAYER with id: " + networkedPlayerIdHasRing.ToString();
    }

}

