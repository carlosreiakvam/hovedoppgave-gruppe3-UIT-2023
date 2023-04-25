using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private Transform playerPrefab;
    [SerializeField] GameStatusSO gamestatus;


    private void Start()
    {
        if (gamestatus.isShortcutUsed)
        {
            Debug.LogWarning("ShortcutManager in use");
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
        else
        {
            Debug.LogWarning("Comming from lobby");
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }



    //Shortcut to Game
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log("Spawning player");
        Transform playerTransform = Instantiate(playerPrefab);
        playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }

    //Via Lobby
    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)

    {
        if (!IsServer) return;
        Debug.Log("OnLoadEventCompleted");
        foreach (ulong clientId in NetworkManager.ConnectedClientsIds)
        {
            Transform playerTransform = Instantiate(playerPrefab);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            playerTransform.SetParent(transform);
        }

    }
}
