using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class RingSpawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    readonly Vector2[] spawnPoints = {
        new Vector2(0f,0f),
        new Vector2(5f,5f),
        new Vector2(-5f,-5f),
        new Vector2(5f,-5f)
    };

    private void Start()
    {
        SpawnRing();
    }


    private void SpawnRing()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        Vector3 randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject ring = Instantiate(prefab, randomSpawnPoint, Quaternion.identity);
        ring.GetComponent<NetworkObject>().Spawn();
        ring.transform.SetParent(transform);
    }
}
