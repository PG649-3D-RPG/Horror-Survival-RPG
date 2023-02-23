using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    private float _period;
    private float _timeAccumulator;

    public void Init(float period)
    {
        _period = period;
    }

    public bool IsReady()
    {
        return _timeAccumulator >= _period;
    }

    public void Spawn(GameObject creature)
    {
        creature.transform.position = transform.position;
        _timeAccumulator = 0;
    }
    
    void Update()
    {
        _timeAccumulator += Time.deltaTime;
    }
}
