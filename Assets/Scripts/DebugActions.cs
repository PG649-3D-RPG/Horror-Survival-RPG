using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugActions : MonoBehaviour
{
    // Start is called before the first frame update
    private Keyboard keyboard;
    void Start()
    {
        keyboard = Keyboard.current;
    }

    // Update is called once per frame
    void Update()
    {
        if (keyboard.f12Key.wasReleasedThisFrame)
        {
            ScreenCapture.CaptureScreenshot(DateTime.Now.ToString("yyyy-MM-ddTHHmmssZ") + ".png");
        }
    }
}
