using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/GameStatusSO", order = 1)]
public class GameStatusSO : ScriptableObject
{
    [HideInInspector]
    public bool gameIsOver = false;

    [HideInInspector]
    public int playerIdHasRing = -1;

    [HideInInspector]
    public bool isAndroid = false;

    [HideInInspector]
    public bool isWindows = false;

    [HideInInspector]
    public Vector2 caveDoorForest = new Vector2(25, 23);

    [HideInInspector]
    public Vector2 caveDoorInCave = new Vector2(93.5f, 10.6f);
}

