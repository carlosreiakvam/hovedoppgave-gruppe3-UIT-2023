using QFSW.QC;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class SpawnManager : NetworkBehaviour
{
    [SerializeField] Transform playerPrefab = null;
    [SerializeField] private GameObject ringPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject hpPrefab;
    [SerializeField] private GameObject speedPrefab;

    public Tilemap tilemap;
    public int maxTries = 100;
    public int nEnemies = 200;
    public int nHealthPowerups = 100;
    public int nSpeedPowerups = 200;
    private List<GameObject> placedObjects = new List<GameObject>();

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
        try
        {
            SpawnAllPlayers();
            SpawnRing();
            SpawnPrefabs();
            return true;
        }
        catch { Debug.LogError("SPAWN UNSUCCESSFULL"); return false; }
    }

    private void SpawnRing()
    {
        Vector3 emptyTile = GetEmptyTile(enemyPrefab);
        SpawnObject(SpawnEnums.Ring, emptyTile);
    }

    public void SpawnPrefabs()
    {
        // Reset the list of placed objects
        placedObjects.Clear();

        // Place enemies
        for (int i = 0; i < nEnemies; i++)
        {
            Vector3 emptyTile = GetEmptyTile(enemyPrefab);
            SpawnObject(SpawnEnums.Enemy, emptyTile);
        }

        // Place hp
        for (int i = 0; i < nHealthPowerups; i++)
        {
            Vector3 emptyTile = GetEmptyTile(hpPrefab);
            SpawnObject(SpawnEnums.HealthPowerUp, emptyTile);
        }

        // place speed
        for (int i = 0; i < nSpeedPowerups; i++)
        {
            Vector3 emptyTile = GetEmptyTile(speedPrefab);
            SpawnObject(SpawnEnums.SpeedPowerUp, emptyTile);
        }

    }



    private Vector3 GetEmptyTile(GameObject prefab)
    {
        Vector3 emptyTile = Vector3.zero;
        const int searchRange = 1; // only check the tile and its neighbors
        for (int i = 0; i < maxTries; i++)
        {
            Vector3Int randomPosition = new Vector3Int(Random.Range(tilemap.cellBounds.xMin + searchRange, tilemap.cellBounds.xMax - searchRange),
                                                        Random.Range(tilemap.cellBounds.yMin + searchRange, tilemap.cellBounds.yMax - searchRange),
                                                        0);

            // Check if the tile and its neighbors are empty
            bool isTileEmpty = true;
            for (int dx = -searchRange; dx <= searchRange; dx++)
            {
                for (int dy = -searchRange; dy <= searchRange; dy++)
                {
                    Vector3Int currentTilePosition = new Vector3Int(randomPosition.x + dx, randomPosition.y + dy, 0);
                    if (tilemap.GetTile(currentTilePosition) != null)
                    {
                        isTileEmpty = false;
                        break;
                    }
                }
                if (!isTileEmpty) break;
            }

            if (isTileEmpty)
            {
                emptyTile = tilemap.CellToWorld(randomPosition);
                break;
            }
        }
        return emptyTile;
    }




    public List<GameObject> GetPlacedObjects()
    {
        return placedObjects;
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
