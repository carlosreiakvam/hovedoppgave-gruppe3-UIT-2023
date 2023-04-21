using TMPro;
using UnityEngine;

class GameManager : MonoBehaviour
{
    public int playerIdHasRing { get; private set; }
    [SerializeField] GameObject inGameMenu;
    TextMeshProUGUI gameWonText; 

    private void Start()
    {
        playerIdHasRing = -1;
        gameWonText = inGameMenu.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void OnPlayerCollectedRing(int playerId)
    {
        playerIdHasRing = playerId;
    }


    public void OnGameWon() { 
        inGameMenu.SetActive(true);
        gameWonText.text = "Player with id: " + playerIdHasRing + " won the game";
    }


    public void OnPlayerDown() { }
    public void OnPlayerDead() { }



}
