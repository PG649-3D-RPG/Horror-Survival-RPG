using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class Flashlight : MonoBehaviour, IEquipment
{
    private bool _state;
    [SerializeField]
    private GameObject _light;

    public void OnPrimary()
    {
        _state = !_state;
        _light.SetActive(_state);
    }
    
    public void OnEquip()
    {
        gameObject.SetActive(true);
    }

    public void OnUnequip()
    {
        gameObject.SetActive(false);
    }
}
