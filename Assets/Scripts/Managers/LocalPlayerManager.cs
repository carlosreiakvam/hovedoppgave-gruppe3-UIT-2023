using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LocalPlayerManager : MonoBehaviour
{
    public struct LocalPlayer
    {
        public string name;
        public ulong id;
        public bool playerHasRing;
        public bool playerWonGame;
        public bool isDead;
    }


    [SerializeField] public GameObject torchPowerupUI;
    [SerializeField] public GameObject ring;
    [SerializeField] public GameObject speedPowerup;

    [SerializeField] GameStatusSO gameStatusSO;
    public static LocalPlayerManager Singleton;
    public LocalPlayer localPlayer = new LocalPlayer();

    public void Awake()
    {
        localPlayer.isDead = false;
        if (Singleton == null) Singleton = this;
        else Destroy(gameObject);

        // PowerupUI
        torchPowerupUI.SetActive(false);
        speedPowerup.SetActive(false);
        ring.SetActive(false);
    }

    /// <summary>
    /// Checks if the provided player ID matches the local player's ID.
    /// </summary>
    /// <param name="id">ID of the player.</param>
    /// <returns>True if the provided ID matches the local player's ID, false otherwise.</returns>
    public bool IsThisIdForLocalPlayer(ulong id)
    {
        return localPlayer.id == id;
    }

    /// <summary>
    /// Gets the name associated with the provided player ID.
    /// </summary>
    /// <param name="id">ID of the player.</param>
    /// <returns>The name of the player if the provided ID matches the local player's ID, an empty string otherwise.</returns>

    public string GetNameFromId(ulong id)
    {
        if (localPlayer.id == id) return localPlayer.name;
        else return "";
    }

    /// <summary>
    /// Sets the name of the local player.
    /// </summary>
    /// <param name="name">The player name to set.</param>
    public void SetName(string name) { localPlayer.name = name; }

    /// <summary>
    /// Sets the ID of the local player.
    /// </summary>
    /// <param name="id">The ID to set.</param>
    public void SetId(ulong id) { localPlayer.id = id; }

    /// <summary>
    /// Sets the 'HasRing' status of the local player.
    /// </summary>
    public void SetHasRing(bool playerHasRing) { localPlayer.playerHasRing = playerHasRing; }

    /// <summary>
    /// Sets the 'PlayerWonGame' status of the local player.
    /// </summary>
    public void SetPlayerWonGame(bool playerWonGame) { localPlayer.playerWonGame = playerWonGame; }


    /// <summary>
    /// Registers the local player in the gameStatusSO scriptable object using the data from the AuthenticationService.
    /// </summary>
    public void RegisterPlayerInScriptableObject()
    {
        string authId = AuthenticationService.Instance.PlayerId;
        ulong localClientid = NetworkManager.Singleton.LocalClientId;


        foreach (Player player in gameStatusSO.lobbyPlayers)
        {
            if (player.Data[LobbyStringConst.PLAYER_ID].Value == authId)
            {
                Debug.LogWarning("FOUND PLAYER WITH NAME " + player.Data[LobbyStringConst.PLAYER_NAME].Value + " and client id " + localClientid);
                localPlayer.name = player.Data[LobbyStringConst.PLAYER_NAME].Value;
                localPlayer.id = localClientid;
            }
        }

    }

}
