using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/GameStatusSO", order = 1)]
public class GameStatusSO : ScriptableObject
{
    public bool gameIsOver = false;
    public bool isShortcutUsed = true;
    public int playerIdHasRing = -1;
}
