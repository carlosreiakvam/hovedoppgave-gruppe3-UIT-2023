using Unity.Netcode;

public class TorchPowerUpBehaviour : NetworkBehaviour
{
    /*    public void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.CompareTag("Player")) return;
            if (IsServer) SpawnManager.Singleton.DespawnObjectServerRpc(NetworkObject.NetworkObjectId);

            GameObject player = collision.gameObject;
            NetworkObject playerNetworkobject = player.GetComponentInParent<NetworkObject>();
            if (playerNetworkobject.IsOwner)
            {
                PlayerBehaviour playerBehaviour = collision.gameObject.GetComponentInParent<PlayerBehaviour>();
                playerBehaviour.ActivateTorchPowerUp();
            }
        }
    */

}
