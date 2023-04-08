using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;

public class RingSpawner : NetworkBehaviour
{
    [SerializeField] private Transform prefab;
    Vector3[] spawnPoints = {
        new Vector3(0f,0f,0f),
        new Vector3(5f,5f,0f),
        new Vector3(-5f,-5f,0f),
        new Vector3(5f,-5f,0f),
    };

    public override void OnNetworkSpawn()
    {

        Debug.Log("Spawning ring");
        if (!IsServer) return;
        prefab.transform.position = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
        Transform transform = Instantiate(prefab);
        transform.GetComponent<NetworkObject>().Spawn(true);
    }
}
