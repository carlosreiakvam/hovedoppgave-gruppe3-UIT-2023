using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuBackground : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    private void Start()
    {
        RectTransform rectTransformCanvas = canvas.GetComponent<RectTransform>();
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = rectTransformCanvas.sizeDelta;
    }
}
