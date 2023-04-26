using QFSW.QC;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
//using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnManager : NetworkBehaviour
{
    [SerializeField] Transform playerPrefab = null;
    [SerializeField] private GameObject ringPrefab;
    [SerializeField] Transform[] prefabs = null;
    public static SpawnManager Singleton;
    private Transform playerTransform;

    private void Awake()
    {
        if (Singleton == null) Singleton = this;
        else Destroy(gameObject);
    }

    public void SpawnAll()
    {
        SpawnAllPrefabs();
        SpawnRing();
        SpawnAllPlayers();
    }

    public void SpawnAllPrefabs()
    {
        foreach (Transform prefab in prefabs) { SpawnObject(prefab); }
    }


    private void SpawnObject(Transform prefab)
    {
        Transform prefabTransform = Instantiate(prefab);
        prefabTransform.GetComponent<NetworkObject>().Spawn(true);
    }


    private void SpawnRing()
    {
        Vector3[] spawnPoints = {
        new Vector3(0f,0f,0f),
        new Vector3(5f,5f,0f),
        new Vector3(0f,5f,0f),
        new Vector3(3f,5f,0f),
    };
        Vector3 randomSpawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
        GameObject ring = Instantiate(ringPrefab, randomSpawnPoint, Quaternion.identity);
        ring.GetComponent<NetworkObject>().Spawn();
        Debug.Log("SPAWNING RING");
    }


    public void SpawnAllPlayers()
    {
        foreach (ulong clientId in Unity.Netcode.NetworkManager.Singleton.ConnectedClientsIds)
        {
            Debug.Log("Spawnin player");
            Transform playerTransform = Instantiate(playerPrefab);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }
    }


    internal void DespawnObject(NetworkObject nObj, GameObject obj)
    {
        if (!IsServer) return;

        // Call the server RPC method to despawn and destroy the network object
        DisconnectObjectServerRpc(nObj.NetworkObjectId);

        // Destroy the game object locally
        Destroy(obj);
    }



    [ServerRpc(RequireOwnership = false)]
    public void DisconnectObjectServerRpc(ulong clientId)
    {
        if (!IsServer) return;

        try
        {
            // Get the player's network object
            NetworkObject playerNetworkObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;

            // If the player's network object exists and is spawned
            if (playerNetworkObject != null && playerNetworkObject.IsSpawned)
            {
                // Despawn the player's network object
                playerNetworkObject.Despawn();

                // Disconnect the player's client
            }
        }
        catch (Exception e)
        {
            Debug.Log($"Failed to disconnect client: {e}");
        }
    }

}
