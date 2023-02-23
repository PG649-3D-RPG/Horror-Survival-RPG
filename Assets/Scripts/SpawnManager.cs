using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class SpawnManager : MonoBehaviour
{
    private static Random Rand = new();
    
    public int MaximumCreatures = 50;
    /// <summary>
    /// Creatures spawn within this radius around the player
    /// </summary>
    public float SpawnRadius = 100.0f;

    /// <summary>
    /// Creatures despawn outside this radius around the player
    /// </summary>
    public float DespawnRadius = 300.0f;

    private List<GameObject> SpawnedCreatures = new();
    
    private GameObject _player;
    private CreatureFactory _creatureFactory;
    private List<SpawnPoint> _spawnPoints;

    private bool _initialized = false;

    public void Init(List<SpawnPoint> spawnPoints, CreatureFactory factory, GameObject player)
    {
        _spawnPoints = spawnPoints;
        _creatureFactory = factory;
        _player = player;
        _initialized = true;
    }

    void Start()
    {
        _player = GameObject.Find("Player");
        _creatureFactory = GameObject.Find("CreatureFactory").GetComponent<CreatureFactory>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_initialized) return;
        
        // Remove destroyed creatures from list of spawned creatures
        SpawnedCreatures = SpawnedCreatures.FindAll(go => go != null);
        
        // Despawning
        SpawnedCreatures.FindAll(c => DistToPlayer(c) > DespawnRadius).ForEach(c => c.GetComponent<BasicCreatureController>().Despawn());  
        
        // Spawning
        int budget = MaximumCreatures - SpawnedCreatures.Count;

        if (budget <= 0) return;

        var spawnPointQueue = _spawnPoints
            .FindAll(s => s.IsReady())
            .FindAll(s => DistToPlayer(s) < SpawnRadius)
            .OrderBy(DistToPlayer)
            .ToList();

        for (int i = 0; i < Math.Min(budget, spawnPointQueue.Count); i++)
        {
            int creatureIndex = Rand.Next(_creatureFactory.GetNumberOfCreatureTypes());
            var creature = _creatureFactory.CreateCreature(creatureIndex);
            spawnPointQueue[i].Spawn(creature);
            SpawnedCreatures.Add(creature);
        }

    }
    
    private float DistToPlayer(SpawnPoint s)
    {
        return Vector3.Distance(s.transform.position, _player.transform.position);
    }

    private float DistToPlayer(GameObject o)
    {
        return Vector3.Distance(o.transform.position, _player.transform.position);
    }
}
