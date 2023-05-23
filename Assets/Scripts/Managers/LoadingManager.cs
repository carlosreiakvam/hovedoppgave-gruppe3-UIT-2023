using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages loading screen and pre-game countdown.
/// </summary>
public class LoadingManager : NetworkBehaviour
{
    [SerializeField] GameObject touchControlsPrefab;
    [SerializeField] GameObject powerupUI;
    [SerializeField] GameStatusSO gameStatusSO;
    [SerializeField] GameObject loadingScreenPanel;
    [SerializeField] TextMeshProUGUI countdownText;

    // Network variables to sync readiness of clients and completion of countdown across network
    public NetworkVariable<bool> networkedClientsReady = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> networkedCountdownFinished = new NetworkVariable<bool>(false);
    GameObject chatPanel;
    GameObject touchUIInstantiated;


    private void Awake()
    {
        loadingScreenPanel.SetActive(true);
        powerupUI.SetActive(false);
        if (gameStatusSO.isAndroid)
        {
            touchUIInstantiated = Instantiate(touchControlsPrefab);
            touchUIInstantiated.SetActive(false);
        }
    }

    /// <summary>
    /// Starts server-specific behaviors.
    /// </summary>
    private void ServerStart()
    {
        StartCoroutine(WaitForPlayersReady());
        GameManager.Singleton.StartGameManagerServer();
    }

    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    public void Start()
    {
        if (IsServer) ServerStart();
        networkedClientsReady.OnValueChanged += OnClientsReady;
        networkedCountdownFinished.OnValueChanged += OnCountdownFinished;
        chatPanel = GameObject.Find("ChatPanel");
        chatPanel.SetActive(false);
    }

    /// <summary>
    /// Handles the event when countdown is finished.
    /// </summary>
    private void OnCountdownFinished(bool previousValue, bool newValue)
    {
        loadingScreenPanel.SetActive(false);
        powerupUI.SetActive(true);
        chatPanel.SetActive(true);
        PlayerBehaviour playerBehaviour = PlayerBehaviour.LocalInstance;
        playerBehaviour.ControlActive(true);

        // Display touch controls if on Android
        if (gameStatusSO.isAndroid) touchUIInstantiated.SetActive(true);
    }

    /// <summary>
    /// Handles the event when clients are ready.
    /// </summary>
    private void OnClientsReady(bool prev, bool newValue)
    {
        StartCoroutine(StartCountdown());
    }

    /// <summary>
    /// Waits for all players to be ready before proceeding.
    /// </summary>
    public IEnumerator WaitForPlayersReady()
    {
        yield return new WaitForSeconds(1f); // Wait  to make sure all players have time to read the instructions 
        while (NetworkManager.Singleton.ConnectedClientsList.Count != gameStatusSO.lobbyPlayers.Count)
        {
            yield return new WaitForSeconds(1f);
        }

        networkedClientsReady.Value = true;
    }

    /// <summary>
    /// Starts the pre-game countdown.
    /// </summary>
    private IEnumerator StartCountdown()
    {
        float countdownDuration = 3f;
        float remainingTime = countdownDuration;

        while (remainingTime > 0f)
        {
            countdownText.text = Mathf.CeilToInt(remainingTime).ToString();
            yield return new WaitForSeconds(1f);
            remainingTime -= 1f;
        }

        if (IsServer) networkedCountdownFinished.Value = true;
    }
}
