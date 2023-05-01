using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

public class HealthPowerupBehaviour : NetworkBehaviour
{
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;
        if (!collision.CompareTag("Player")) return;

        SpawnManager.Singleton.DespawnObjectServerRpc(NetworkObject.NetworkObjectId);

        // Get and return corresponding playerHealth
        GameObject player = collision.gameObject;
        PlayerHealth playerHealth = player.GetComponentInChildren<PlayerHealth>();

        if (playerHealth != null) playerHealth.AddHealth();
    }

}
