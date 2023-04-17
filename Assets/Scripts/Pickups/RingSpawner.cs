using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class RingSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject prefab;
    readonly Vector2[] spawnPoints = {
        new Vector2(0f,0f),
        new Vector2(5f,5f),
        new Vector2(-5f,-5f),
        new Vector2(5f,-5f)
    };


    public override void OnNetworkSpawn()
    {
        Debug.Log("OnNetworkSpawn");
        if (!IsServer) return;
        Vector3 randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject ring = Instantiate(prefab, randomSpawnPoint, prefab.transform.rotation);
        ring.GetComponent<NetworkObject>().Spawn();
    }
}
