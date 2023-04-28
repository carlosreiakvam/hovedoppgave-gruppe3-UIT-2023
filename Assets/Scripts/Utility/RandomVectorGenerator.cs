using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomVectorGenerator : MonoBehaviour
{
    private System.Random rnd = new System.Random();
    public static RandomVectorGenerator Singleton;

    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Singleton = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public Vector3 GenerateRandomVector3()
    {
        float worldWidth = Camera.main.orthographicSize * Camera.main.aspect * 2.0f;
        float worldHeight = Camera.main.orthographicSize * 2.0f;
        float x = (float)(rnd.NextDouble() * worldWidth - worldWidth / 2.0);
        float y = (float)(rnd.NextDouble() * worldHeight - worldHeight / 2.0);
        return new Vector3(x, y, 0.0f);
    }

    public Vector3 GenerateRandomVector2()
    {
        float worldWidth = Camera.main.orthographicSize * Camera.main.aspect * 2.0f;
        float worldHeight = Camera.main.orthographicSize * 2.0f;
        float x = (float)(rnd.NextDouble() * worldWidth - worldWidth / 2.0);
        float y = (float)(rnd.NextDouble() * worldHeight - worldHeight / 2.0);
        return new Vector2(x, y);
    }
}
