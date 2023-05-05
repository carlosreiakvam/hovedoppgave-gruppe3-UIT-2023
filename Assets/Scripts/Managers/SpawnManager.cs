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

    [SerializeField] private GameStatusSO gameStatus;

    [SerializeField] private GameObject ringPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject hpPrefab;
    [SerializeField] private GameObject speedPrefab;
    [SerializeField] private GameObject caveEntrancePrefab;

    public Tilemap tilemap;
    public const int maxTries = 100;
    public const int nEnemiesOutside = 20;
    public const int nEnemiesCave = 10;
    public const int nHealthPowerupsOutdoor = 25;
    public const int nHealthPowerupsCave = 25;
    public const int nSpeedPowerupsOutdoor = 25;
    public const int nSpeedPowerupsCave = 25;
    private List<GameObject> placedObjects = new List<GameObject>();


    private static Dictionary<SpawnEnums, int> mapBoundsOutdoor = new Dictionary<SpawnEnums, int>()
{
    { SpawnEnums.X_MIN, 5 },
    { SpawnEnums.X_MAX, 45 },
    { SpawnEnums.Y_MIN, 5 },
    { SpawnEnums.Y_MAX, 45 },
    { SpawnEnums.X_MIDDLE, 45/2 },
    { SpawnEnums.Y_MIDDLE, 45/2 },

};

    private static Dictionary<SpawnEnums, int> mapBoundsCave = new Dictionary<SpawnEnums, int>()
{
    { SpawnEnums.X_MIN, 95 },
    { SpawnEnums.X_MAX, 140 },
    { SpawnEnums.Y_MIN, 5 },
    { SpawnEnums.Y_MAX, 45 },
    { SpawnEnums.X_MIDDLE, 95/2 },
    { SpawnEnums.Y_MIDDLE, 45/2 },
};



    private const int MIDDLE_PADDING = 5; // free space around the middle where the players will spawn
    private int X_MIN_FREE_SPACE_MIDDLE = mapBoundsOutdoor[SpawnEnums.X_MIDDLE] - MIDDLE_PADDING;
    private int X__MAX_FREE_SPACE_MIDDLE = mapBoundsOutdoor[SpawnEnums.X_MIDDLE] + MIDDLE_PADDING;
    private int Y_MIN_FREE_SPACE_MIDDLE = mapBoundsOutdoor[SpawnEnums.Y_MIDDLE] - MIDDLE_PADDING;
    private int Y_MAX_FREE_SPACE_MIDDLE = mapBoundsOutdoor[SpawnEnums.Y_MIDDLE] + MIDDLE_PADDING;


    private Transform playerTransform;
    private Dictionary<ulong, NetworkObject> spawnedObjects = new Dictionary<ulong, NetworkObject>();


    public static SpawnManager Singleton;
    private void Awake()
    {
        if (Singleton == null) Singleton = this;
        else Destroy(gameObject);
    }

    public bool SpawnAll()
    {
        Debug.LogWarning("SPAWNING PREFABS");
        try
        {
            SpawnPlayers();
            SpawnPrefabs(SpawnEnums.Ring, EnvironmentEnums.Cave, 1, 1);

            SpawnCaveDoors();

            SpawnPrefabs(SpawnEnums.Enemy, EnvironmentEnums.Outdoor, nEnemiesOutside, 1);
            SpawnPrefabs(SpawnEnums.Enemy, EnvironmentEnums.Cave, nEnemiesCave, 1);

            SpawnPrefabs(SpawnEnums.HealthPowerUp, EnvironmentEnums.Outdoor, nHealthPowerupsOutdoor, 1);
            SpawnPrefabs(SpawnEnums.HealthPowerUp, EnvironmentEnums.Cave, nHealthPowerupsCave, 1);

            SpawnPrefabs(SpawnEnums.SpeedPowerUp, EnvironmentEnums.Outdoor, nSpeedPowerupsOutdoor, 1);
            SpawnPrefabs(SpawnEnums.SpeedPowerUp, EnvironmentEnums.Cave, nSpeedPowerupsCave, 1);

            return true;
        }
        catch (Exception e) { Debug.Log(e); return false; }
    }

    private void SpawnCaveDoors()
    {
        // Cavedoor outdoor
        // Set the location of the cave entrance in the game status so that the player knows where to respawn on reentering outdoors 
        Vector3 outdoorCaveEntranceLocation = GetEmptyTile(2, EnvironmentEnums.Outdoor);
        gameStatus.outdoorCaveEntrance = outdoorCaveEntranceLocation;
        SpawnObject(SpawnEnums.CaveEntrance, outdoorCaveEntranceLocation);

        // Cavedoor in cave
        Vector3 caveCaveDoorPosition = new Vector3(93, 1.5f, 0f);
        gameStatus.caveCaveEntrance = caveCaveDoorPosition;
        SpawnObject(SpawnEnums.CaveEntrance, caveCaveDoorPosition);
    }

    private void SpawnPrefabs(SpawnEnums spawnEnum, EnvironmentEnums environment, int nInstances, int searchRange)
    {
        for (int i = 0; i < nInstances; i++)
        {
            Vector3 emptyTile = GetEmptyTile(searchRange, environment);
            SpawnObject(spawnEnum, emptyTile);
        }
    }


    private Vector3 GetEmptyTile(int searchRange, EnvironmentEnums environment)
    {
        Dictionary<SpawnEnums, int> boundaries;
        if (environment == EnvironmentEnums.Outdoor) { boundaries = mapBoundsOutdoor; }
        else { boundaries = mapBoundsCave; }

        Vector3 emptyTile = Vector3.zero;
        for (int i = 0; i < maxTries; i++)
        {
            Vector3Int randomPosition = new Vector3Int(Random.Range(boundaries[SpawnEnums.X_MIN] + searchRange, boundaries[SpawnEnums.X_MAX] - searchRange),
                                                        Random.Range(boundaries[SpawnEnums.Y_MIN] + searchRange, boundaries[SpawnEnums.Y_MAX] - searchRange),
                                                        0);

            if (environment == EnvironmentEnums.Outdoor)
            {
                // Exclude area in the middle where players will spawn
                if ((randomPosition.x >= X_MIN_FREE_SPACE_MIDDLE && randomPosition.x <= X__MAX_FREE_SPACE_MIDDLE) && (randomPosition.y >= Y_MIN_FREE_SPACE_MIDDLE && randomPosition.y <= Y_MAX_FREE_SPACE_MIDDLE)) { continue; }

            }

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
            SpawnEnums.CaveEntrance => caveEntrancePrefab,
            _ => null
        };

        GameObject instance = Instantiate(prefab, spawnPoint, Quaternion.identity);
        NetworkObject networkObject = instance.GetComponent<NetworkObject>();
        networkObject.Spawn();
        spawnedObjects.Add(networkObject.NetworkObjectId, networkObject);
    }


    public void SpawnPlayers()
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
