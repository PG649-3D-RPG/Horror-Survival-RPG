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
        var loader = GetComponent<Loader>();
        loader.Load(SetupLevel());
    }

    private IEnumerator SetupLevel()
    {
        GameObject terrain = WorldGenerator.Generate(WGSettings);
        terrain.layer = LayerMask.NameToLayer("Ground");
        Debug.Log(terrain.GetComponent<MiscTerrainData>().SpawnPoints.Count);
        yield return null;
        
        GameObject player = SetupPlayer(terrain);
        yield return null;
        
        var creatureFactory = FindObjectOfType<CreatureFactory>();
        //GameObject c1 = CreatureGenerator.ParametricBiped(CGSettings, BipedSettings, null);
        //creatureFactory.AddPrototype(c1);
        var dogPrefab = Resources.Load("Prefabs/4B_Creature_Dog2V2") as GameObject;
        GameObject dog = GameObject.Instantiate(dogPrefab);
        creatureFactory.AddPrototype(dog);
        yield return null;

        List<SpawnPoint> spawnPoints = new();
        foreach (var spawnLocation in terrain.GetComponent<MiscTerrainData>().SpawnPoints.Skip(1)) 
        {
            var spawner = new GameObject
            {
                transform =
                {
                    position = spawnLocation.Item1
                }
            };
            var spawnPoint = spawner.AddComponent<SpawnPoint>();
            spawnPoint.Init(10.0f);
            spawnPoints.Add(spawnPoint);
        }

        yield return null;

        var spawnManager = FindObjectOfType<SpawnManager>();
        spawnManager.Init(spawnPoints, creatureFactory, player);
        spawnManager.enabled = true;
    }
    private GameObject SetupPlayer(GameObject terrain)
    {
        var playerPrefab = Resources.Load("Prefabs/Player") as GameObject;
        var spawnPoint = terrain.GetComponent<MiscTerrainData>().SpawnPoints[0].Item1;
        var spawnLifted = spawnPoint + new Vector3(0, 0.1f, 0);
        var player = GameObject.Instantiate(playerPrefab, spawnLifted, transform.rotation);
        Debug.Log(player.name);
        player.name = "Player";
        // let fpscam follow player
        var fpscam = GameObject.Find("FPSCam");
        var playercam = player.transform.Find("PlayerCameraRoot");
        // fpscam.GetComponent<CinemachineVirtualCamera>().LookAt = c.transform;
        fpscam.GetComponent<CinemachineVirtualCamera>().Follow = playercam.transform;
        return player;
    }
}
