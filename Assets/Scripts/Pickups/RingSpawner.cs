using UnityEngine;
using Unity.Netcode;

public class RingSpawner : NetworkBehaviour
{
    [SerializeField] private Transform prefab;

    public override void OnNetworkSpawn()
    {
        Debug.Log("Spawning ring");
        if (!IsServer) return;
        Transform transform = Instantiate(prefab);
        transform.GetComponent<NetworkObject>().Spawn(true);
    }
}
