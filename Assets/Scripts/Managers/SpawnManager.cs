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
    private HashSet<ulong> spawnedClientIds = new HashSet<ulong>(); // keeps track of which clients have been spawned

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


    /*        new Vector3(5f,5f,0f),
            new Vector3(10f,15f,0f),
            new Vector3(5f,15f,0f),
    */
    private void SpawnRing()
    {
        Vector3[] spawnPoints = {
        new Vector3(0f,0f,0f),
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
        nObj.Despawn();
        Destroy(obj);
    }



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
