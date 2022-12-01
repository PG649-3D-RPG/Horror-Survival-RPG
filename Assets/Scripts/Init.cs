using System;
using System.Collections;
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
        var creatureFactory = FindObjectOfType<CreatureFactory>();
        GameObject terrain = WorldGenerator.Generate(WGSettings);
        terrain.layer = LayerMask.NameToLayer("Ground");
        GameObject c1 = CreatureGenerator.ParametricBiped(CGSettings, BipedSettings, null);
        creatureFactory.AddPrototype(c1);
        
        GameObject player = SetupPlayer(terrain);
        yield return null;
    }
    private GameObject SetupPlayer(GameObject terrain)
    {
        var playerPrefab = Resources.Load("Prefabs/Player") as GameObject;
        var spawnPoint = terrain.GetComponent<MiscTerrainData>().SpawnPoints[0].Item1;
        var spawnLifted = spawnPoint + new Vector3(0, 0.1f, 0);
        var player = GameObject.Instantiate(playerPrefab, spawnLifted, transform.rotation);
        // let fpscam follow player
        var fpscam = GameObject.Find("FPSCam");
        var playercam = player.transform.Find("PlayerCameraRoot");
        // fpscam.GetComponent<CinemachineVirtualCamera>().LookAt = c.transform;
        fpscam.GetComponent<CinemachineVirtualCamera>().Follow = playercam.transform;
        return player;
    }
}
