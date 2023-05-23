using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatCanvas : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

}
