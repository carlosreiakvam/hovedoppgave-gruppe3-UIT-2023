using QFSW.QC;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnManager : NetworkBehaviour
{
    [SerializeField] Transform playerPrefab = null;
    [SerializeField] Transform[] prefabs = null;
    public static SpawnManager Singleton;
    private Transform playerTransform;
    private HashSet<ulong> spawnedClientIds = new HashSet<ulong>(); // keeps track of which clients have been spawned


    private void Awake()
    {
        if (Singleton == null) Singleton = this;
        else Destroy(gameObject);
    }
    /*    private void Start()
        {
            if (IsServer) NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    */
    public void SpawnAllPrefabs()
    {
        if (!IsServer) return;
        foreach (Transform prefab in prefabs) { SpawnObject(prefab); }
    }


    private void SpawnObject(Transform prefab)
    {
        Transform prefabTransform = Instantiate(prefab);
        prefabTransform.GetComponent<NetworkObject>().Spawn(true);
    }

    public void SpawnAllPlayers()
    {
        foreach (ulong clientId in Unity.Netcode.NetworkManager.Singleton.ConnectedClientsIds)
        {
            playerTransform = Instantiate(playerPrefab);
            NetworkObject playerNetworkObject = playerTransform.GetComponent<NetworkObject>();
            playerNetworkObject.SpawnAsPlayerObject(clientId, true);
        }
    }

    internal void DespawnObject(NetworkObject nObj, GameObject obj)
    {
        if (!IsServer) return;
        nObj.Despawn();
        Destroy(obj);
    }


    /*    private void OnDestroy()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }
    */
    [ClientRpc]
    public void DisconnectClientRpc(ulong clientId)
    {
        NetworkObject playerNetworkObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        if (playerNetworkObject != null && playerNetworkObject.IsSpawned)
        {
            playerNetworkObject.Despawn();
            Destroy(playerNetworkObject.gameObject);
            NetworkManager.Singleton.DisconnectClient(clientId);
        }
    }
}
