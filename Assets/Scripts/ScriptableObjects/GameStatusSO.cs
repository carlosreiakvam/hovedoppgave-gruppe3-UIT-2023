using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/GameStatusSO", order = 1)]
public class GameStatusSO : ScriptableObject
{
    public bool gameIsOver = false;
    public int playerIdHasRing = -1;
    public bool isAndroid = false;
    public bool isWindows = false;
    public Vector2 outdoorCaveEntrance = new Vector2(0, 0);
    public Vector2 caveCaveEntrance = new Vector2(96f, 4f);
}
