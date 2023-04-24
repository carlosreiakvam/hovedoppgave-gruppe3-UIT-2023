using QFSW.QC;
using Unity.Netcode;
using UnityEngine;

public class EnemySpawnerTempForShortcut : NetworkBehaviour
{
    [SerializeField] private Transform enemyPrefab;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        Transform enemyTransform = Instantiate(enemyPrefab);
        enemyTransform.GetComponent<NetworkObject>().Spawn(true);
    }
}