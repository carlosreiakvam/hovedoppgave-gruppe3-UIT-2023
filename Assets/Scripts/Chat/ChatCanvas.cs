using UnityEngine;

public class ChatCanvas : MonoBehaviour
{
    public static ChatCanvas Instance;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void ShowChatCanvas(bool active)
    {
        gameObject.SetActive(active);

    }

}
