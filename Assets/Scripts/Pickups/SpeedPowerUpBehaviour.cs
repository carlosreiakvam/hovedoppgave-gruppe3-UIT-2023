using Unity.Netcode;
using UnityEngine;

public class SpeedPowerUpBehaviour : NetworkBehaviour
{
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (IsServer) SpawnManager.Singleton.DespawnObjectServerRpc(NetworkObject.NetworkObjectId);

        GameObject player = collision.gameObject;
        NetworkObject playerNetworkobject = player.GetComponentInParent<NetworkObject>();
        if (playerNetworkobject.IsOwner)
        {
            PlayerBehaviour playerBehaviour = player.GetComponentInParent<PlayerBehaviour>();
            playerBehaviour.IncreaseSpeed();
        }
    }

}
