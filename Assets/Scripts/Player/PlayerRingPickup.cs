using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerRingPickup : NetworkBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsOwner) return;
        if (!(collision.gameObject.tag == "Ring")) return;
        if (LocalPlayerManager.Singleton.localPlayer.playerHasRing) return;

        LocalPlayerManager.Singleton.ring.SetActive(true);
        LocalPlayerManager.Singleton.localPlayer.playerHasRing = true;

        DespawnRing(collision);

        string playerName = LocalPlayerManager.Singleton.localPlayer.name;
        ChatManager.Instance.SendMsg(playerName + " collected a ring!", "Wizard");

        // OnPlayerPickedUpRingServerRpc(localPlayerName);
    }

    private void DespawnRing(Collider2D collision)
    {
        GameObject ring = collision.gameObject;
        NetworkObject ringNetworkObject = ring.GetComponentInParent<NetworkObject>();
        SpawnManager.Singleton.DespawnObjectServerRpc(ringNetworkObject.NetworkObjectId);
    }

/*    [ServerRpc(RequireOwnership = false)]
    public void OnPlayerPickedUpRingServerRpc(string name)
    {
        OnPlayerPickedUpRingClientRpc(name);
    }

    [ClientRpc]
    public void OnPlayerPickedUpRingClientRpc(string name)
    {
        GameManager.Singleton.OnPlayerPickedUpRing(name);
    }
*/
}
