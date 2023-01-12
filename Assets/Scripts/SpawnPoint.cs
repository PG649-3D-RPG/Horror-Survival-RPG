using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{

    private Func<GameObject> _creatureFactory;
    private float _period;
    private int _limit;
    
    private float _timeAccumulator;

    private List<GameObject> _spawned = new();

    public void Init(Func<GameObject> factory, float period, int limit)
    {
        _creatureFactory = factory;
        _period = period;
        _limit = limit;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (_spawned.Count <= _limit)
        {
            _timeAccumulator += Time.deltaTime;
        }

        while (_timeAccumulator >= _period)
        {
            var creature = _creatureFactory();
            Debug.Log("===========");
            Debug.Log(creature.transform.position);
            Debug.Log(transform.position);
            Debug.Log(creature.transform.rotation.eulerAngles);
            Debug.Log(transform.rotation.eulerAngles);
            creature.transform.position = transform.position;
            Debug.Log(creature.transform.position);
            _spawned.Add(creature);
            _timeAccumulator -= _period;
        }
    }
}
