using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InitializeManager : MonoBehaviour
{
    [SerializeField] GameStatusSO gamestatusSO;
    void Awake()
    {
        gamestatusSO.Reset(); // make sure this is run before anything else
        InitializePlatform();
        DestroyNetworkManagerMultiples();
    }


    private void InitializePlatform()
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

    private void DestroyNetworkManagerMultiples()
    {
        List<GameObject> objects = GameObject.FindObjectsOfType<GameObject>().ToList<GameObject>();
        List<GameObject> networkManagers = new List<GameObject>();
        foreach (GameObject obj in objects)
        {
            if (obj.name.Equals("NetworkManager")) networkManagers.Add(obj);
        }
        while (networkManagers.Count > 1)
        {
            networkManagers.Remove(networkManagers.First());
            Destroy(networkManagers.First());
            Debug.LogWarning("Destroyed a NetworkManager Multiple");
        }
    }

}
