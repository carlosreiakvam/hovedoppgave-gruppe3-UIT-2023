using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class RingBehaviour : NetworkBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;
        if (!collision.CompareTag("Player")) return;

        int playerId = collision.GetInstanceID();
        GameManager.Singleton.OnPlayerCollectedRingServerRpc(playerId);

        Debug.Log("Player with instance playerId " + playerId + " collided with ring");

        SpawnManager.Singleton.DespawnObject(NetworkObject, gameObject);
        Destroy(gameObject);
    }
}

