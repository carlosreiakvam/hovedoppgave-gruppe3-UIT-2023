using Unity.Netcode;
using UnityEngine;

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
        windowsLoadingScreen.SetActive(gameStatusSO.isWindows);
        androidLoadingScreen.SetActive(!gameStatusSO.isWindows);
    }
}
