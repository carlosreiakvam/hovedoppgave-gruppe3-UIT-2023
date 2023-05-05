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
    public const int maxTries = 100;
    public const int nEnemies = 50;
    public const int nHealthPowerups = 25;
    public const int nSpeedPowerups = 25;
    private List<GameObject> placedObjects = new List<GameObject>();

    private const int X_MIN = 0;
    private const int X_MAX = 50;
    private const int Y_MIN = 0;
    private const int Y_MAX = 50;
    private const int X_MIDDLE = X_MAX / 2;
    private const int Y_MIDDLE = Y_MAX / 2;
    private const int MIDDLE_PADDING = 5; // free space around the middle where the players will spawn
    private const int X_MIN_FREE_SPACE_MIDDLE = X_MIDDLE - MIDDLE_PADDING;
    private const int X__MAX_FREE_SPACE_MIDDLE = X_MIDDLE + MIDDLE_PADDING;
    private const int Y_MIN_FREE_SPACE_MIDDLE = Y_MIDDLE - MIDDLE_PADDING;
    private const int Y_MAX_FREE_SPACE_MIDDLE = Y_MIDDLE + MIDDLE_PADDING;


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
        Vector3 emptyTile = GetEmptyTile();
        SpawnObject(SpawnEnums.Ring, emptyTile);
    }

    public void SpawnPrefabs()
    {
        // Reset the list of placed objects
        placedObjects.Clear();

        // Place enemies
        for (int i = 0; i < nEnemies; i++)
        {
            Vector3 emptyTile = GetEmptyTile();
            SpawnObject(SpawnEnums.Enemy, emptyTile);
        }

        // Place hp
        for (int i = 0; i < nHealthPowerups; i++)
        {
            Vector3 emptyTile = GetEmptyTile();
            SpawnObject(SpawnEnums.HealthPowerUp, emptyTile);
        }

        // place speed
        for (int i = 0; i < nSpeedPowerups; i++)
        {
            Vector3 emptyTile = GetEmptyTile();
            SpawnObject(SpawnEnums.SpeedPowerUp, emptyTile);
        }

    }



    private Vector3 GetEmptyTile()
    {
        Vector3 emptyTile = Vector3.zero;
        const int searchRange = 1; // only check the tile and its neighbors
        for (int i = 0; i < maxTries; i++)
        {
            Vector3Int randomPosition = new Vector3Int(Random.Range(X_MIN + searchRange, X_MAX - searchRange),
                                                        Random.Range(Y_MIN + searchRange, Y_MAX - searchRange),
                                                        0);

            // Exclude area in the middle where players will spawn
            if ((randomPosition.x >= X_MIN_FREE_SPACE_MIDDLE && randomPosition.x <= X__MAX_FREE_SPACE_MIDDLE) && (randomPosition.y >= Y_MIN_FREE_SPACE_MIDDLE && randomPosition.y <= Y_MAX_FREE_SPACE_MIDDLE)) { continue; }

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
