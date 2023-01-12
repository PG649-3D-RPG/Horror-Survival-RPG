using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class Flashlight : MonoBehaviour
{
    private bool _state;
    [SerializeField]
    private GameObject _light;

    public void Toggle()
    {
        _state = !_state;
        _light.SetActive(_state);
    }
}
