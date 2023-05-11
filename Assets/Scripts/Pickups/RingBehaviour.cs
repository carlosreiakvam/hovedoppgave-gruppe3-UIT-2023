using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class RingBehaviour : NetworkBehaviour
{

    /*    private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.CompareTag("Player")) return;

            GameObject player = collision.gameObject;
            NetworkObject playerNetworkObject = player.GetComponentInParent<NetworkObject>();
            ulong playerOwnerClientId = playerNetworkObject.OwnerClientId;
            ulong playerNetworkObjectId = playerNetworkObject.NetworkObjectId;

            bool ringPickupFromLocalPlayer = LocalPlayerManager.Singleton.localPlayer.id == playerOwnerClientId;


            // Send name of player who picked up ring to GameManager
            string winnerName = LocalPlayerManager.Singleton.GetNameFromId(playerOwnerClientId);
            LocalPlayerManager.Singleton.localPlayer.playerHasRing = true;
            GameManager.Singleton.OnRingChangedOwner(LocalPlayerManager.Singleton.localPlayer.name);

            // Despawn Ring
            SpawnManager.Singleton.DespawnObjectServerRpc(NetworkObject.NetworkObjectId); // networkobject id is 10
        }
    */
}

