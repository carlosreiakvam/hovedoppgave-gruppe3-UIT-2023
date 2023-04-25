using QFSW.QC;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class DebugManager : NetworkBehaviour
{
    public static DebugManager Singleton;
    private void Awake()
    {
        if (Singleton == null) Singleton = this;
        else Destroy(gameObject);
    }

    [Command]
    public void WhoAmI()
    {
        Debug.LogWarning("WHO AM I?");
        Debug.Log("IsServer:" + IsServer);
        Debug.Log("IsClient:" + IsClient);
        Debug.Log("IsLocalPlayer:" + IsLocalPlayer);
        Debug.Log("IsSpawned:" + IsSpawned);
        Debug.Log("IsHost:" + IsHost);
        Debug.Log("IsOwner:" + IsOwner);
        Debug.Log("IsOwnedByServer:" + IsOwnedByServer);
    }

}
