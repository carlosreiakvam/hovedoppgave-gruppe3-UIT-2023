using Unity.Netcode;
using UnityEngine;

public class RingBehaviour : NetworkBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (!collision.CompareTag("Player")) return;

        NetworkObject obj = collision.attachedRigidbody.gameObject.GetComponent<NetworkObject>();
        int id = obj.GetInstanceID();
        Debug.Log("Player with instance id " + id + " collided with ring");
        NetworkObject.Despawn(true);
    }

}
