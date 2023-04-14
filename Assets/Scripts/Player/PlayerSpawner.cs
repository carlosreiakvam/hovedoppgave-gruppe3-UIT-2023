using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private Transform playerPrefab;
    //public const int MAX_PLAYER_AMOUNT = 4;
    //private const string PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER = "PlayerNameMultiplayer";
    //public event EventHandler OnPlayerDataNetworkListChanged;

    //public event EventHandler OnStateChanged;
    //private NetworkList<PlayerData> playerDataNetworkList;
    //private string playerName;
    //private enum State
    //{
    //    WaitingToStart,
    //    CountdownToStart,
    //    GamePlaying,
    //    GameOver,
    //}


    //private void Awake()
    //{

    //    playerName = PlayerPrefs.GetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, "PlayerName" + UnityEngine.Random.Range(100, 1000));

    //    playerDataNetworkList = new NetworkList<PlayerData>();
    //    playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
    //}



    public override void OnNetworkSpawn()
    {

        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            //NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
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
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true); //NB! Not Spawn(true). It is for single player
        }

    }

    //private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    //{
    //    OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    //}


    //[ServerRpc(RequireOwnership = false)]
    //private void SetPlayerNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default)
    //{
    //    int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

    //    PlayerData playerData = playerDataNetworkList[playerDataIndex];

    //    playerData.playerName = playerName;

    //    playerDataNetworkList[playerDataIndex] = playerData;
    //}
}
