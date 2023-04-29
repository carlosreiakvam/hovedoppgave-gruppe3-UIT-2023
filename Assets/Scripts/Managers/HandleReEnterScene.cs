using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class HandleReEnterScene : MonoBehaviour
{
    void Start()
    {
        DestroyNetworkManagerMultiples();
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

