using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

// Note: Needs to be a MonoBehaviour to run coroutines
public class SceneTransition : MonoBehaviour
{

    private static SceneTransition Self;
    
    public void Awake()
    {
        Self = this;
    }

    public static void ToLevel(WorldGeneratorSettings worldGeneratorSettings)
    {
        Self.LoadingScreen(SetupLevel(worldGeneratorSettings));
    }

    public static void ToGameOver()
    {
        Self.Instant(ShowGameOver());
    }

    public static void ToWinScreen()
    {
        Self.Instant(ShowWinScreen());
    }

    private void LoadingScreen(IEnumerator workCoroutine)
    {
        StartCoroutine(LoaderCoroutine(workCoroutine));
    }

    private void Instant(IEnumerator workCoroutine)
    {
        StartCoroutine(workCoroutine);
    }

    private IEnumerator LoaderCoroutine(IEnumerator workCoroutine)
    {
        SceneManager.LoadScene("Scenes/LoadingScreen/LoadingScreen", LoadSceneMode.Additive);
        SceneManager.LoadScene("Scenes/Level", LoadSceneMode.Additive);
        yield return null;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Level"));
        // Run the actual loading work. Nested coroutines are necessary to that we can wait for the
        // workCoroutine to finish.
        yield return StartCoroutine(workCoroutine);
        SceneManager.UnloadSceneAsync("Scenes/LoadingScreen/LoadingScreen");
    }
    
    private static IEnumerator ShowGameOver()
    {
            SceneManager.LoadScene("Scenes/GameOverScreen/GameOverScreen", LoadSceneMode.Additive);
            yield return null;
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("GameOverScreen"));
            SceneManager.UnloadSceneAsync("Scenes/Level");
    }

    private static IEnumerator ShowWinScreen()
    {
            SceneManager.LoadScene("Scenes/WinScreen/WinScreen", LoadSceneMode.Additive);
            yield return null;
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("WinScreen"));
            SceneManager.UnloadSceneAsync("Scenes/Level");
    }
        
    private static IEnumerator SetupLevel(WorldGeneratorSettings worldGeneratorSettings)
    {
        GameObject terrain = WorldGenerator.Generate(worldGeneratorSettings);
        terrain.layer = LayerMask.NameToLayer("Ground");
        Debug.Log(terrain.GetComponent<MiscTerrainData>().SpawnPoints.Count);
        yield return null;
        
        GameObject player = SetupPlayer(terrain);
        yield return null;
        
        var creatureFactory = FindObjectOfType<CreatureFactory>();
        //GameObject c1 = CreatureGenerator.ParametricBiped(CGSettings, BipedSettings, null);
        //creatureFactory.AddPrototype(c1);
        //var dogPrefab = Resources.Load("Prefabs/4B_Creature_Dog2V2") as GameObject;
        //GameObject dog = GameObject.Instantiate(dogPrefab);
        //creatureFactory.AddPrototype(dog);
        var quadrupedPrefab = Resources.Load("Prefabs/Creature") as GameObject;
        var quadruped = GameObject.Instantiate(quadrupedPrefab); 
        creatureFactory.AddPrototype(quadruped);
        yield return null;
        
        //var bipedPrefab = Resources.Load("Prefabs/DebugCreature_1 shock absorb") as GameObject;
        //var biped = GameObject.Instantiate(bipedPrefab); 
        //creatureFactory.AddPrototype(biped);
        //yield return null;

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
    private static GameObject SetupPlayer(GameObject terrain)
    {
        var playerPrefab = Resources.Load("Prefabs/Player") as GameObject;
        var spawnPoint = terrain.GetComponent<MiscTerrainData>().SpawnPoints[0].Item1;
        var spawnLifted = spawnPoint + new Vector3(0, 0.1f, 0);
        var player = GameObject.Instantiate(playerPrefab, spawnLifted, Quaternion.identity);
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
