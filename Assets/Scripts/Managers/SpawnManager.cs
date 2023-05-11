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
using Mono.CSharp;

public class SpawnManager : NetworkBehaviour
{
    [SerializeField] Transform playerPrefab = null;

    [SerializeField] private GameStatusSO gameStatus;

    [SerializeField] private GameObject ringPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject hpPrefab;
    [SerializeField] private GameObject speedPrefab;
    [SerializeField] private GameObject caveDoorForestPrefab;
    [SerializeField] private GameObject caveDoorCavePrefab;
    [SerializeField] private GameObject wizardPrefab;

    public Tilemap forestTilemap;
    public Tilemap caveTilemap;

    public const int maxTries = 100;
    public const int nEnemiesOutside = 15;
    public const int nEnemiesCave = 15;
    public const int nHealthPowerupsOutdoor = 15;
    public const int nHealthPowerupsCave = 25;
    public const int nSpeedPowerupsOutdoor = 15;
    public const int nSpeedPowerupsCave = 15;


    private static Dictionary<SpawnEnums, int> mapBoundsOutdoor = new Dictionary<SpawnEnums, int>()
{
    { SpawnEnums.X_MIN, 1 },
    { SpawnEnums.X_MAX, 50 },
    { SpawnEnums.Y_MIN, 1 },
    { SpawnEnums.Y_MAX, 50 },
    { SpawnEnums.X_MIDDLE, 45/2 },
    { SpawnEnums.Y_MIDDLE, 45/2 },

};

    private static Dictionary<SpawnEnums, int> mapBoundsCave = new Dictionary<SpawnEnums, int>()
{
    { SpawnEnums.X_MIN, 95 },
    { SpawnEnums.X_MAX, 140 },
    { SpawnEnums.Y_MIN, 1 },
    { SpawnEnums.Y_MAX, 50 },
    { SpawnEnums.X_MIDDLE, 95/2 },
    { SpawnEnums.Y_MIDDLE, 45/2 },
};


    private Transform playerTransform;
    private Dictionary<ulong, NetworkObject> spawnedObjects = new Dictionary<ulong, NetworkObject>();


    public static SpawnManager Singleton;
    private void Awake()
    {
        if (Singleton == null) Singleton = this;
        else Destroy(gameObject);
    }

    private Dictionary<SpawnEnums, int> GetMidAreaFromOutdoor(int sideLength)
    {

        Dictionary<SpawnEnums, int> midArea = new Dictionary<SpawnEnums, int>()
        {
            { SpawnEnums.X_MIN, mapBoundsOutdoor[SpawnEnums.X_MIDDLE] - sideLength },
            { SpawnEnums.X_MAX, mapBoundsOutdoor[SpawnEnums.X_MIDDLE] + sideLength },
            { SpawnEnums.Y_MIN, mapBoundsOutdoor[SpawnEnums.Y_MIDDLE] - sideLength },
            { SpawnEnums.Y_MAX, mapBoundsOutdoor[SpawnEnums.Y_MIDDLE] + sideLength },
        };

        return midArea;
    }

    public bool SpawnAll()
    {
        try
        {
            SpawnPlayers();

            // SPAWN TESTRINGS
            SpawnObject(SpawnEnums.Ring, new Vector2(23, 20));
            SpawnObject(SpawnEnums.Ring, new Vector2(25, 20));

            // SPAWN WIZARD
            SpawnObject(SpawnEnums.Wizard, new Vector2(25, 26));

            SpawnCaveDoors();

            SpawnPrefabs(SpawnEnums.Ring, environment: EnvironmentEnums.Cave, nInstances: 1, searchRange: 1);


            SpawnPrefabs(SpawnEnums.Enemy, environment: EnvironmentEnums.Outdoor, nInstances: nEnemiesOutside, searchRange: 1, excludedMidAreaSideLength: 10);
            SpawnPrefabs(SpawnEnums.Enemy, environment: EnvironmentEnums.Cave, nInstances: nEnemiesCave, searchRange: 1);

            SpawnPrefabs(SpawnEnums.HealthPowerUp, environment: EnvironmentEnums.Outdoor, nInstances: nHealthPowerupsOutdoor, searchRange: 1);
            SpawnPrefabs(SpawnEnums.HealthPowerUp, environment: EnvironmentEnums.Cave, nInstances: nHealthPowerupsCave, searchRange: 1);

            SpawnPrefabs(SpawnEnums.SpeedPowerUp, environment: EnvironmentEnums.Outdoor, nInstances: nSpeedPowerupsOutdoor, searchRange: 1);
            SpawnPrefabs(SpawnEnums.SpeedPowerUp, environment: EnvironmentEnums.Cave, nInstances: nSpeedPowerupsCave, searchRange: 1);


            return true;
        }
        catch (Exception e) { Debug.Log(e); return false; }
    }

    private void SpawnCaveDoors()
    {
        gameStatus.caveDoorInCave = new Vector2(96f, 4.4f);
        gameStatus.caveDoorForest = new Vector2(25f, 24f);//  This position is changed by the prefab script
        SpawnObject(SpawnEnums.CaveDoorForest, gameStatus.caveDoorForest);
        SpawnObject(SpawnEnums.CaveDoorCave, gameStatus.caveDoorInCave);
    }

    private void SpawnPrefabs(SpawnEnums spawnEnum, EnvironmentEnums environment, int nInstances, int searchRange, int excludedMidAreaSideLength = -1)
    {
        for (int i = 0; i < nInstances; i++)
        {
            Vector3 emptyTile = GetEmptyTile(searchRange, environment, excludedMidAreaSideLength);
            SpawnObject(spawnEnum, emptyTile);
        }
    }


    private Vector2 GetEmptyTile(int searchRange, EnvironmentEnums environment, int excludedMidAreaSideLength = -1)
    {
        Dictionary<SpawnEnums, int> boundaries;
        Tilemap tilemap;
        if (environment == EnvironmentEnums.Outdoor)
        {
            boundaries = mapBoundsOutdoor;
            tilemap = forestTilemap;
        }
        else
        {
            boundaries = mapBoundsCave;
            tilemap = caveTilemap;
        }

        Vector2 emptyTile = Vector2.zero;
        for (int i = 0; i < maxTries; i++)
        {
            Vector3Int randomPosition = new Vector3Int(Random.Range(boundaries[SpawnEnums.X_MIN] + searchRange, boundaries[SpawnEnums.X_MAX] - searchRange),
                                                        Random.Range(boundaries[SpawnEnums.Y_MIN] + searchRange, boundaries[SpawnEnums.Y_MAX] - searchRange), 0
                                                        );

            if (excludedMidAreaSideLength != -1)
            {
                // 'continue' if empty tile is in middle area
                Dictionary<SpawnEnums, int> midArea = GetMidAreaFromOutdoor(excludedMidAreaSideLength);
                if (
                    (randomPosition.x >= midArea[SpawnEnums.X_MIN] && randomPosition.x <= midArea[SpawnEnums.X_MAX])
                    && (randomPosition.y >= midArea[SpawnEnums.Y_MIN] && randomPosition.y <= midArea[SpawnEnums.Y_MAX])
                    )
                { continue; }
            }

            // Check if the tile and its neighbors are empty
            bool isTileEmpty = true;
            for (int dx = -searchRange; dx <= searchRange; dx++)
            {
                for (int dy = -searchRange; dy <= searchRange; dy++)
                {
                    if (tilemap.GetTile(randomPosition) != null)
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

        return new Vector2(emptyTile.x, emptyTile.y);
    }


    private void SpawnObject(SpawnEnums spawnenum, Vector2 spawnPoint)
    {
        GameObject prefab = spawnenum switch
        {
            SpawnEnums.Enemy => enemyPrefab,
            SpawnEnums.Ring => ringPrefab,
            SpawnEnums.HealthPowerUp => hpPrefab,
            SpawnEnums.SpeedPowerUp => speedPrefab,
            SpawnEnums.CaveDoorCave => caveDoorCavePrefab,
            SpawnEnums.CaveDoorForest => caveDoorForestPrefab,
            SpawnEnums.Wizard => wizardPrefab,
            _ => null
        };

        GameObject instance = Instantiate(prefab, spawnPoint, Quaternion.identity);
        NetworkObject networkObject = instance.GetComponent<NetworkObject>();
        networkObject.Spawn();
        spawnedObjects.Add(networkObject.NetworkObjectId, networkObject);
    }


    public void SpawnPlayers()
    {

        int playerNumber = 0;
        foreach (ulong clientId in Unity.Netcode.NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform playerTransform = Instantiate(playerPrefab);
            PlayerBehaviour playerBehaviour = playerTransform.GetComponent<PlayerBehaviour>();
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            playerNumber++;
        }
    }

    public void DespawnAllPlayers()
    {
        foreach (ulong clientId in Unity.Netcode.NetworkManager.Singleton.ConnectedClientsIds)
        {
            DisconnectPlayerServerRpc(clientId);
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
