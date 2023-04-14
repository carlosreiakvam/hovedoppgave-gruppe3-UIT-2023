using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonsHolder : MonoBehaviour
{
    private int screenHeight;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(Screen.height);
    }
    private void Update()
    {
        if (Screen.height != screenHeight)
        {
            screenHeight = Screen.height;
            transform.position = new Vector3(Screen.width / 2, screenHeight / 2, 0f);
        }

    }

}
