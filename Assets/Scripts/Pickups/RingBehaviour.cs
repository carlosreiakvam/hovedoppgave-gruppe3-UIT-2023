using Unity.Netcode;
using UnityEngine;

public class RingBehaviour : NetworkBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;
        if (!collision.CompareTag("Player")) return;

        int id = GetComponent<NetworkObject>().GetInstanceID();
        Debug.Log("Player with instance id " + id + " collided with ring");
        NetworkObject.Despawn(true);
    }

}
