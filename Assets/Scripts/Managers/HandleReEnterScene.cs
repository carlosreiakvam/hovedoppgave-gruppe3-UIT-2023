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
        List<GameObject> objs = GameObject.FindObjectsOfType<GameObject>().ToList<GameObject>();
        List<GameObject> nms = new List<GameObject>();
        foreach (GameObject obj in objs)
        {
            if (obj.name.Equals("NetworkManager")) nms.Add(obj);
        }
        while (nms.Count > 1)
        {
            nms.Remove(nms.First());
            Destroy(nms.First());
            Debug.LogWarning("Destroyed a NetworkManager Multiple");
        }
    }
}

