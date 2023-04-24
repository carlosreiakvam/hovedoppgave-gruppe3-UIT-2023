using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFSW.QC;
using Unity.Netcode;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform enemyPrefab;

    private void Start()
    {
        SpawnEnemy();
    }

    

    //[Command]
    private void SpawnEnemy()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        Transform enemyTransform = Instantiate(enemyPrefab);
        enemyTransform.GetComponent<NetworkObject>().Spawn(true);
        enemyTransform.SetParent(transform); // Set correct placement in hirearchy
    }
}
