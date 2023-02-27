using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UnityEngine;

public class Init : MonoBehaviour
{
    public CreatureGeneratorSettings CGSettings;
    public BipedSettings BipedSettings;
    public WorldGeneratorSettings WGSettings;

    // Start is called before the first frame update
    void Start()
    {
        SceneTransition.ToLevel(WGSettings);
    }
}
