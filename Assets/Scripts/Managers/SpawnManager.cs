using QFSW.QC;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
//using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SpawnManager : NetworkBehaviour
{
    [SerializeField] Transform playerPrefab = null;
    [SerializeField] private GameObject ringPrefab;
    [SerializeField] private GameObject healthPowerUp;
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
        SpawnHealthPowerUps();
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
        Vector2[] spawnPoints = {
        new Vector2(0f,0f),
        new Vector2(5f,5f),
        new Vector2(0f,5f),
        new Vector2(3f,5f),
    };
        Vector3 randomSpawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
        GameObject ring = Instantiate(ringPrefab, randomSpawnPoint, Quaternion.identity);
        ring.GetComponent<NetworkObject>().Spawn();
        Debug.Log("SPAWNING RING");
    }
    public void SpawnHealthPowerUps()
    {
        Vector2[] spawnPoints = {
        new Vector2(2f,2f),
        new Vector2(3f,4f),
        new Vector2(-2f,5f),
        new Vector2(4f,1f),
        new Vector2(1f,2f),
        new Vector2(1f,2f),
        new Vector2(-1f,2f),
        new Vector2(1f,-2f),
    };
        foreach (Vector2 spawnpoint in spawnPoints)
        {
            {
                GameObject hp = Instantiate(healthPowerUp, spawnpoint, Quaternion.identity);
                hp.SetActive(true);
                hp.GetComponent<NetworkObject>().Spawn();
            }
        }
        Debug.Log("Spawned HP Powerups");
    }


    private void RespawnRingOnPlayerDeath()
    {
        // get player
        // get player position
        // spawn ring at player position
        /*        GameObject ring = Instantiate(ringPrefab, randomSpawnPoint, Quaternion.identity);
                ring.GetComponent<NetworkObject>().Spawn();
                Debug.Log("RESPAWNING RING");
        */
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
