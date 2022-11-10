using System.Collections;
using Config;
using Unity.MLAgents;
using Unity.VisualScripting;
using UnityEngine;
using Random = System.Random;

public class WalkTargetScript : MonoBehaviour
{
    private GameObject ArenaTerrain { get; set; }

    private Random Rng { get; set; }

    private Vector3 TargetDirection { get; set; }

    private Rigidbody ThisRigidbody { get; set; }

    
    private GenericAgent _agent;

    private const string TagToDetect = "Agent";

    private ArenaConfig _arenaSettings;
    
    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    /// <returns></returns>
    public void Start()
    {
        _arenaSettings = FindObjectOfType<ArenaConfig>();
        
        Rng = new Random();
        var parent = transform.parent;
        ThisRigidbody = transform.GetComponentInChildren<Rigidbody>();
        _agent = parent.GetComponentInChildren<GenericAgent>();

    }

    /// <summary> 
    /// Change direction randomly every x seconds
    /// </summary>
    /// <returns></returns>
    private IEnumerator ChangeDirection()
    {
        while (true)
        {
            TargetDirection = Vector3.Normalize(new Vector3((float)(Rng.NextDouble() * 2) - 1, 0, (float)(Rng.NextDouble() * 2) - 1));
            yield return new WaitForSeconds(UnityEngine.Random.Range(1, _arenaSettings.TargetMaxSecondsInOneDirection));
        }
    }


    /// <summary>
    /// Move to random direction if target collided with walls
    /// Signal if the agent touched the target
    /// </summary>
    /// <returns></returns>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag(TagToDetect))
        {
            _agent.TouchedTarget();
        }
        if (collision.gameObject.name != "Terrain")
        {
            TargetDirection = Quaternion.AngleAxis(UnityEngine.Random.Range(60,170), Vector3.up) * TargetDirection;
        }
    }
}
