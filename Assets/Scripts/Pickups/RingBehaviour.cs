using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class RingBehaviour : NetworkBehaviour
{
    public NetworkVariable<ulong> networkedPlayerIdHasRing = new NetworkVariable<ulong>(0);


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        networkedPlayerIdHasRing.OnValueChanged += OnPlayerIdHasRingChangedClientRpc;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;
        if (!collision.CompareTag("Player")) return;

        GameObject player = collision.gameObject;
        NetworkObject playerNetworkObject = player.GetComponentInParent<NetworkObject>();
        ulong playerNetworkObjectId = playerNetworkObject.NetworkObjectId;

        Debug.LogWarning("Player with networkObjectId " + playerNetworkObjectId + " collided with ring");

        Debug.Log("IsClient: " + IsClient);
        if (IsClient) OnPlayerCollectedRingServerRpc(playerNetworkObjectId);
        SpawnManager.Singleton.DespawnObjectServerRpc(NetworkObject.NetworkObjectId); // networkobject id is 10
    }

    [ServerRpc(RequireOwnership = false)]
    internal void OnPlayerCollectedRingServerRpc(ulong playerNetworkObjectId)
    {
        networkedPlayerIdHasRing.Value = playerNetworkObjectId;
    }

    [ClientRpc]
    private void OnPlayerIdHasRingChangedClientRpc(ulong previousValue, ulong newValue)
    {
        GameManager.Singleton.OnRingChangedOwner(newValue);
    }

}

