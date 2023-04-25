using QFSW.QC;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnManager : NetworkBehaviour
{
    [SerializeField] GameStatusSO gamestatus;
    [SerializeField] Transform playerPrefab = null;
    [SerializeField] Transform[] prefabs = null;
    public static SpawnManager Singleton;
    private HashSet<ulong> spawnedClientIds = new HashSet<ulong>(); // keeps track of which clients have been spawned


    private void Awake()
    {
        if (Singleton == null) Singleton = this;
        else Destroy(gameObject);
    }


    public void SpawnAllPrefabs()
    {
        if (!IsServer) return;
        foreach (Transform prefab in prefabs) { SpawnObject(prefab); }
    }


    private void SpawnObject(Transform prefab)
    {
        Transform prefabTransform = Instantiate(prefab);
        prefabTransform.GetComponent<NetworkObject>().Spawn(true);
    }

    /*    public void SpawnPlayer(ulong obj)
        {
            Debug.Log("Spawn player");
            if (!IsServer) return;
            Transform playerTransform = Instantiate(playerPrefab);
            NetworkObject networkObject = playerTransform.GetComponent<NetworkObject>();
            NetworkObject.SpawnAsPlayerObject(obj, true);
        }
    */
    internal void SpawnAllPlayers()
    {
        if (!IsServer) return;
        foreach (ulong clientId in Unity.Netcode.NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (spawnedClientIds.Contains(clientId))
            {
                continue; // Skip spawning if the player object is already spawned
            }

            Transform playerTransform = Instantiate(playerPrefab);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

            // Add the clientId to the spawnedClientIds HashSet
            spawnedClientIds.Add(clientId);
        }

    }
}
