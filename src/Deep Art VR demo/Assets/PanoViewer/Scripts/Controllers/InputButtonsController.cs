using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputButtonsController : MonoBehaviour
{
    void Update()
    {
        if (OVRInput.GetUp(OVRInput.Button.Two))
        {
            Canvas canvasMenu = GameObject.Find("CanvasMenu").GetComponent<Canvas>();
            canvasMenu.enabled = !canvasMenu.enabled;

        }
    }
}
