using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitPlatform : MonoBehaviour
{
    [SerializeField] GameStatusSO gamestatusSO;
    void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            gamestatusSO.isAndroid = true;
            Debug.Log("Current platform is Android.");
        }
        else if (Application.platform == RuntimePlatform.WindowsEditor ||
         Application.platform == RuntimePlatform.WindowsPlayer)
        {
            gamestatusSO.isWindows = true;
            Debug.Log("Current platform is Windows.");
        }
        else
        {
            Debug.Log("Current platform is not supported.");
        }
    }

}
