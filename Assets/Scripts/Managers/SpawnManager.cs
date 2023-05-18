using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using UnityEngine.UI;

public class SpawnManager : NetworkBehaviour
{
    [SerializeField] Material caveMaterial;
    [SerializeField] Transform playerPrefab = null;

    [SerializeField] private GameStatusSO gameStatus;

    [SerializeField] private GameObject materialManagerPrefab;
    [SerializeField] private GameObject torchPrefab;
    [SerializeField] private GameObject ringPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject hpPrefab;
    [SerializeField] private GameObject speedPrefab;
    [SerializeField] private GameObject doorForestPrefab;
    [SerializeField] private GameObject doorCavePrefab;
    [SerializeField] private GameObject wizardPrefab;

    public Tilemap forestTilemap;
    public Tilemap treeTilemap;
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
            SpawnObject(SpawnEnums.Ring, new Vector2(27, 25), EnvironmentEnums.Outdoor);

            // SPAWN WIZARD
            SpawnObject(SpawnEnums.Wizard, new Vector2(25, 26), EnvironmentEnums.Outdoor);

            // SPAWN CAVE DOORS
            SpawnCaveDoors();

            // SPAWN RINGS
            SpawnPrefabs(SpawnEnums.Ring, environment: EnvironmentEnums.Cave, nInstances: 4, searchRange: 1);

            // SPAWN TORCHES
            SpawnPrefabs(SpawnEnums.TorchPowerUp, environment: EnvironmentEnums.Cave, nInstances: 15, searchRange: 1);

            // SPAWN ENEMIES
            SpawnPrefabs(SpawnEnums.Enemy, environment: EnvironmentEnums.Outdoor, nInstances: nEnemiesOutside, searchRange: 1, excludedMidAreaSideLength: 10);
            SpawnPrefabs(SpawnEnums.Enemy, environment: EnvironmentEnums.Cave, nInstances: nEnemiesCave, searchRange: 1);

            // SPAWN HEALTH POWERUPS
            SpawnPrefabs(SpawnEnums.HealthPowerUp, environment: EnvironmentEnums.Outdoor, nInstances: nHealthPowerupsOutdoor, searchRange: 1);
            SpawnPrefabs(SpawnEnums.HealthPowerUp, environment: EnvironmentEnums.Cave, nInstances: nHealthPowerupsCave, searchRange: 1);

            // SPAWN SPEED POWERUPS
            SpawnPrefabs(SpawnEnums.SpeedPowerUp, environment: EnvironmentEnums.Outdoor, nInstances: nSpeedPowerupsOutdoor, searchRange: 1);
            SpawnPrefabs(SpawnEnums.SpeedPowerUp, environment: EnvironmentEnums.Cave, nInstances: nSpeedPowerupsCave, searchRange: 1);

            // MATERIAL MANAGER HAS TO BE LAST
            SpawnObject(SpawnEnums.MaterialManager, new Vector2(), EnvironmentEnums.NoEnvironment);

            return true;
        }
        catch (Exception e) { Debug.Log(e); return false; }
    }

    private void SpawnCaveDoors()
    {
        List<Vector2> spawnPositions = new List<Vector2>
    {
        new Vector2(10, 9.6f),
        new Vector2(10, 23.6f),
        new Vector2(44.5f, 50.6f),
        new Vector2(3.5f, 50.6f),
        new Vector2(8, 35.6f),
        new Vector2(37, 37.6f),
        new Vector2(41, 18.5f),
        new Vector2(44, 10.6f),
        new Vector2(46.6f, 18.6f)
    };
        Vector2 doorForestPosition = spawnPositions[Random.Range(0, spawnPositions.Count)];
        gameStatus.doorForestPosition = doorForestPosition;
        SpawnObject(SpawnEnums.DoorForest, doorForestPosition, EnvironmentEnums.Outdoor);
        SpawnObject(SpawnEnums.DoorCave, gameStatus.doorCavePosition, EnvironmentEnums.Cave);
    }


    private void SpawnPrefabs(SpawnEnums spawnEnum, EnvironmentEnums environment, int nInstances, int searchRange, int excludedMidAreaSideLength = -1)
    {
        for (int i = 0; i < nInstances; i++)
        {
            Vector3 emptyTile = GetEmptyTile(searchRange, environment, excludedMidAreaSideLength);
            SpawnObject(spawnEnum, emptyTile, environment);
        }
    }


    public Vector2 GetEmptyTile(int searchRange, EnvironmentEnums environment, int excludedMidAreaSideLength = -1)
    {
        Dictionary<SpawnEnums, int> boundaries;
        List<Tilemap> tilemaps;
        List<Tilemap> outdoorTilemaps = new List<Tilemap> { forestTilemap, treeTilemap };
        List<Tilemap> caveTilemaps = new List<Tilemap> { caveTilemap };

        // Set the correct boundaries and tilemaps based on the environment.
        if (environment == EnvironmentEnums.Outdoor)
        {
            boundaries = mapBoundsOutdoor;
            tilemaps = outdoorTilemaps;
        }
        else
        {
            boundaries = mapBoundsCave;
            tilemaps = caveTilemaps;
        }

        Vector2 emptyTile = Vector2.zero;

        // Try to find an empty tile maxTries times.
        for (int i = 0; i < maxTries; i++)
        {

            // Generate a random position within the boundaries.
            Vector3Int randomPosition = new Vector3Int(Random.Range(boundaries[SpawnEnums.X_MIN] + searchRange, boundaries[SpawnEnums.X_MAX] - searchRange),
                                                        Random.Range(boundaries[SpawnEnums.Y_MIN] + searchRange, boundaries[SpawnEnums.Y_MAX] - searchRange), 0
                                                        );

            // If an area in the middle should be excluded, check if the random position is within that area and if so, skip this iteration.
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

            // Assume the tile is empty until proven otherwise.
            bool isTileEmpty = true;

            // Check the tile at the random position and its neighboring tiles.
            for (int dx = -searchRange; dx <= searchRange; dx++)
            {
                for (int dy = -searchRange; dy <= searchRange; dy++)
                {
                    Vector3Int position = randomPosition + new Vector3Int(dx, dy, 0);

                    // If a tile is not empty, break out of the loop.
                    if (!IsTileEmptyAt(position, tilemaps))
                    {
                        isTileEmpty = false;
                        break;
                    }
                }

                if (!isTileEmpty) break;
            }

            // If an empty tile was found, convert its position to world coordinates and break the loop.
            if (isTileEmpty)
            {
                emptyTile = tilemaps[0].CellToWorld(randomPosition);
                break;
            }
        }

        return new Vector2(emptyTile.x, emptyTile.y);
    }

    // This method checks if a tile at a given position in a list of tilemaps is empty.
    private bool IsTileEmptyAt(Vector3Int position, List<Tilemap> tilemaps)
    {
        foreach (Tilemap tilemap in tilemaps)
        {
            if (tilemap.GetTile(position) != null)
            {
                return false;
            }
        }

        return true;
    }


    private void SpawnObject(SpawnEnums spawnenum, Vector2 spawnPoint, EnvironmentEnums environment)
    {
        GameObject prefab = spawnenum switch
        {
            SpawnEnums.Enemy => enemyPrefab,
            SpawnEnums.Ring => ringPrefab,
            SpawnEnums.HealthPowerUp => hpPrefab,
            SpawnEnums.SpeedPowerUp => speedPrefab,
            SpawnEnums.DoorCave => doorCavePrefab,
            SpawnEnums.DoorForest => doorForestPrefab,
            SpawnEnums.Wizard => wizardPrefab,
            SpawnEnums.TorchPowerUp => torchPrefab,
            SpawnEnums.MaterialManager => materialManagerPrefab,
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

    // Might remove this. Better to use Destroy(gameObject)?
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

    // Call this method when despawning with Destroy(gameObject) on gameobject
    internal void RemoveFromSpawnedList(ulong networkId)
    {
        spawnedObjects.Remove(networkId);
    }
}
