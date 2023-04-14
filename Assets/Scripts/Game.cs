using QFSW.QC;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine;

public class Game : NetworkBehaviour
{
    public object NetworkServer { get; private set; }


    public void OnPlayerHasRing()
    {
        // let everyone know?
    }

    public void OnPlayerHasReturnedRing()
    {
        // end game
    }


}
