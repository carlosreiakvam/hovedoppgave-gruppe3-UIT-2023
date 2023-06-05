using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionHelper : NetworkBehaviour
{
    [SerializeField] GameStatusSO gameStatusSO;
    NetworkVariable<int> playerReadyCount = new NetworkVariable<int>(0);
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        playerReadyCount.OnValueChanged += OnPlayerReadyCountChanged;
        LobbyManager.Singleton.RegisterLobbyToGameStatusSO();

        AddPlayerReadyServerRpc();
    }

    private void OnPlayerReadyCountChanged(int previousValue, int newValue)
    {
        if (playerReadyCount.Value == gameStatusSO.lobbyPlayers.Count && IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }

    }


    [ServerRpc(RequireOwnership = false)]
    private void AddPlayerReadyServerRpc()
    {
        playerReadyCount.Value = playerReadyCount.Value + 1;
    }


}
