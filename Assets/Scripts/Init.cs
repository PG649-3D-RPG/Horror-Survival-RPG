using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;

public class Init : MonoBehaviour
{
    public CreatureGeneratorSettings CGSettings;
    public BipedSettings BipedSettings;
    
    // Start is called before the first frame update
    void Start()
    {
        var loader = GetComponent<Loader>();
        loader.Load(SetupLevel());
    }

    private IEnumerator SetupLevel()
    {
        GameObject c1 = CreatureGenerator.ParametricBiped(CGSettings, BipedSettings, null);
        yield return null;
    }
}
