using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : NetworkBehaviour
{
    [SerializeField] GameStatusSO gameStatusSO;
    [SerializeField] GameObject windowsLoadingScreen;
    [SerializeField] GameObject androidLoadingScreen;


    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void OnEnable()
    {
        Debug.Log(gameStatusSO.isWindows);
        windowsLoadingScreen.SetActive(gameStatusSO.isWindows);
        androidLoadingScreen.SetActive(!gameStatusSO.isWindows);
    }
}
