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
    public const int MAX_TRIES = 100;
    public const int N_ENEMIES_FOREST = 15;
    public const int N_ENEMIES_CAVE = 15;
    public const int N_HEALTH_POWERUPS_FOREST = 15;
    public const int N_HEALTH_POWERUPS_CAVE = 25;
    public const int N_SPEED_POWERUPS_FOREST = 15;
    public const int N_SPEED_POWERUPS_CAVE = 15;

    [SerializeField] Material caveMaterial;
    [SerializeField] private GameStatusSO gameStatus;
    [SerializeField] Transform playerPrefabTransform = null;

    // Prefabs
    [SerializeField] private GameObject materialManagerPrefab;
    [SerializeField] private GameObject torchPrefab;
    [SerializeField] private GameObject ringPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject hpPrefab;
    [SerializeField] private GameObject speedPrefab;
    [SerializeField] private GameObject doorForestPrefab;
    [SerializeField] private GameObject doorCavePrefab;
    [SerializeField] private GameObject wizardPrefab;



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
        try
        {

            // SPAWN PLAYERS
            SpawnPlayers();

            // SPAWN WIZARD
            SpawnObject(SpawnEnums.Wizard, new Vector2(25, 26), EnvironmentEnums.Outdoor);

            // SPAWN CAVE DOORS
            SpawnCaveDoors();

            // SPAWN RINGS
            SpawnPrefabsAtEmptyTiles(SpawnEnums.Ring, environment: EnvironmentEnums.Cave, nInstances: 1, searchRange: 1);

            // SPAWN TORCHES
            SpawnPrefabsAtEmptyTiles(SpawnEnums.TorchPowerUp, environment: EnvironmentEnums.Cave, nInstances: 15, searchRange: 1);

            // SPAWN ENEMIES
/*            SpawnPrefabsAtEmptyTiles(SpawnEnums.Enemy, environment: EnvironmentEnums.Outdoor, nInstances: N_ENEMIES_FOREST, searchRange: 1, excludedMidAreaSideLength: 10);
            SpawnPrefabsAtEmptyTiles(SpawnEnums.Enemy, environment: EnvironmentEnums.Cave, nInstances: N_ENEMIES_CAVE, searchRange: 1);
*/
            // SPAWN HEALTH POWERUPS
            SpawnPrefabsAtEmptyTiles(SpawnEnums.HealthPowerUp, environment: EnvironmentEnums.Outdoor, nInstances: N_HEALTH_POWERUPS_FOREST, searchRange: 1);
            SpawnPrefabsAtEmptyTiles(SpawnEnums.HealthPowerUp, environment: EnvironmentEnums.Cave, nInstances: N_HEALTH_POWERUPS_CAVE, searchRange: 1);

            // SPAWN SPEED POWERUPS
            SpawnPrefabsAtEmptyTiles(SpawnEnums.SpeedPowerUp, environment: EnvironmentEnums.Outdoor, nInstances: N_SPEED_POWERUPS_FOREST, searchRange: 1);
            SpawnPrefabsAtEmptyTiles(SpawnEnums.SpeedPowerUp, environment: EnvironmentEnums.Cave, nInstances: N_SPEED_POWERUPS_CAVE, searchRange: 1);

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


    private void SpawnPrefabsAtEmptyTiles(SpawnEnums spawnEnum, EnvironmentEnums environment, int nInstances, int searchRange, int excludedMidAreaSideLength = -1)
    {
        for (int i = 0; i < nInstances; i++)
        {
            Vector3 emptyTile = TileManager.Singleton.GetEmptyTile(searchRange, environment, excludedMidAreaSideLength);
            SpawnObject(spawnEnum, emptyTile, environment);
        }
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
        Transform playerTransform;
        PlayerBehaviour playerBehaviour;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            playerTransform = Instantiate(playerPrefabTransform);
            playerBehaviour = playerTransform.GetComponent<PlayerBehaviour>();
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            playerNumber++;
        }
    }

    public void DespawnAllPlayers()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
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

    // Call this method when despawning with Destroy(gameObject) on gameobject
    internal void RemoveFromSpawnedList(ulong networkId)
    {
        spawnedObjects.Remove(networkId);
    }
}
