using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;

public class RingSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject prefab;
    Vector3[] spawnPoints = {
        new Vector3(0f,0f,0f),
        new Vector3(5f,5f,0f),
        new Vector3(-5f,-5f,0f),
        new Vector3(5f,-5f,0f),
    };


    public override void OnNetworkSpawn()
    {
        Debug.Log("OnNetworkSpawn");
        if (!IsServer) return;
        Vector3 randomSpawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
        GameObject ring = Instantiate(prefab, randomSpawnPoint, Quaternion.identity);
        ring.GetComponent<NetworkObject>().Spawn();
    }
}
