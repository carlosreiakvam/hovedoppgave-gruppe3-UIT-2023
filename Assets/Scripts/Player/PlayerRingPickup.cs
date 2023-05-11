using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerRingPickup : NetworkBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!(collision.gameObject.tag == "Ring")) return;
        DespawnRing(collision);

        string localPlayerName = LocalPlayerManager.Singleton.localPlayer.name;
        LocalPlayerManager.Singleton.localPlayer.playerHasRing = true;
        Debug.LogWarning(localPlayerName + " has ring!");
        OnPlayerPickedUpRingServerRpc(localPlayerName);
    }

    private void DespawnRing(Collider2D collision)
    {
        GameObject ring = collision.gameObject;
        NetworkObject ringNetworkObject = ring.GetComponentInParent<NetworkObject>();
        SpawnManager.Singleton.DespawnObjectServerRpc(ringNetworkObject.NetworkObjectId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnPlayerPickedUpRingServerRpc(string name)
    {
        OnPlayerPickedUpRingClientRpc(name);
    }

    [ClientRpc]
    public void OnPlayerPickedUpRingClientRpc(string name)
    {
        GameManager.Singleton.OnPlayerPickedUpRing(name);
    }

}
