using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFSW.QC;
using Unity.Netcode;

public class RingSpawner : NetworkBehaviour
{
    [SerializeField] private Transform prefab;
    [Command]
    private void SpawnRing()
    {
        if (!IsServer) return;
        Transform enemyTransform = Instantiate(prefab);
        enemyTransform.GetComponent<NetworkObject>().Spawn(true);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        Transform transform = Instantiate(prefab);
        transform.GetComponent<NetworkObject>().Spawn(true);
    }
}
