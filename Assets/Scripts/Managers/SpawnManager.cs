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
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject hpPrefab;
    [SerializeField] private GameObject speedPrefab;
    public static SpawnManager Singleton;
    private Transform playerTransform;
    private Dictionary<ulong, NetworkObject> spawnedObjects = new Dictionary<ulong, NetworkObject>();


    private void Awake()
    {
        if (Singleton == null) Singleton = this;
        else Destroy(gameObject);
    }

    public bool SpawnAll()
    {
        if (IsServer)
        {
            try
            {
                Debug.LogWarning("SPAWNMANAGER STARTED");
                SpawnAllPlayers();
                SpawnEnemy();
                SpawnRing();
                SpawnHealthPowerUps();
                SpawnSpeedPowerUps();
                Debug.LogWarning("SPAWNMANAGER SPAWNED ALL");
            }
            catch { return false; }
        }
        return true;
    }

    public void SpawnEnemy()
    {
        //for (int i = 0; i < 3; i++)
        //{
        //    SpawnObject(SpawnEnums.Enemy, new Vector2(4f + i * 2, 3f));
        //}

        SpawnObject(SpawnEnums.Enemy, new Vector2(4f * 2, 3f));
    }


    private void SpawnObject(SpawnEnums spawnenum, Vector2 spawnPoint)
    {
        GameObject prefab = spawnenum switch
        {
            SpawnEnums.Enemy => enemyPrefab,
            SpawnEnums.Ring => ringPrefab,
            SpawnEnums.HealthPowerUp => hpPrefab,
            SpawnEnums.SpeedPowerUp => speedPrefab,
            _ => null
        };

        GameObject instance = Instantiate(prefab, spawnPoint, Quaternion.identity);
        NetworkObject networkObject = instance.GetComponent<NetworkObject>();
        networkObject.Spawn();
        spawnedObjects.Add(networkObject.NetworkObjectId, networkObject);
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
        SpawnObject(SpawnEnums.Ring, randomSpawnPoint);
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
            SpawnObject(SpawnEnums.HealthPowerUp, spawnpoint);
        }
    }

    public void SpawnSpeedPowerUps()
    {
        Vector2[] spawnPoints = {
        new Vector2(0f,4f),
        new Vector2(4f,0f),
    };
        foreach (Vector2 spawnpoint in spawnPoints)
        {
            SpawnObject(SpawnEnums.SpeedPowerUp, spawnpoint);
        }
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
            Debug.LogWarning("Spawning player with id: " + clientId);
            Transform playerTransform = Instantiate(playerPrefab);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    internal void DespawnObjectServerRpc(ulong networkObjectId)
    {
        NetworkObject networkObject;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out networkObject))
        {
            networkObject.Despawn();
            spawnedObjects.Remove(networkObjectId);
        }
        else
        {
            Debug.LogWarning($"NetworkObject with ID {networkObjectId} not found.");
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void DisconnectPlayerServerRpc(ulong clientId)
    {
        if (!IsServer) return;

        try
        {
            // Get the player's network object
            NetworkObject playerNetworkObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;

            // If the player's network object exists and is spawned
            if (playerNetworkObject != null && playerNetworkObject.IsSpawned)
            {
                playerNetworkObject.Despawn();
            }
        }
        catch (Exception e)
        {
            Debug.Log($"Failed to disconnect client: {e}");
        }
    }

}
