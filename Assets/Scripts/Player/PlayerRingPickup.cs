using Unity.Netcode;
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

        string playerName = LocalPlayerManager.Singleton.localPlayer.name;
        ChatManager.Instance.SendMsg(playerName + " collected a ring!", "Wizard");
    }

    private void DespawnRing(Collider2D collision)
    {
        GameObject ring = collision.gameObject;
        NetworkObject ringNetworkObject = ring.GetComponentInParent<NetworkObject>();
        SpawnManager.Singleton.DespawnObjectServerRpc(ringNetworkObject.NetworkObjectId);
    }
}
