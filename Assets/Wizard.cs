using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Wizard : NetworkBehaviour
{
    private IEnumerator coroutine;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        coroutine = RepeatMessage();
        StartCoroutine(coroutine);
    }

    private IEnumerator RepeatMessage()
    {
        while (true)
        {
            yield return new WaitForSeconds(5);
            ChatManager.Instance.SendMsg("Help me find the ring!", "Wizard");
            yield return new WaitForSeconds(60);
        }
    }

}
