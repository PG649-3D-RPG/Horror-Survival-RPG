using System.Collections;
using System.Collections.Generic;
using Config;
using UnityEngine;
using UnityEngine.AI;

public class BasicCreatureController : MonoBehaviour, ICreatureController
{
    [SerializeField]
    private float _health = 100f;
    private GenericAgent _movementAgent;
    private NavMeshPath _path;
    private int _pathCornerIndex;
    private Transform _rootElementTransform;
    private Transform _target;
    private float _timeElapsed;

    private float _walkingSpeed;

    void Awake() {
        var mlAgentsConfig = FindObjectOfType<MlAgentConfig>();
        _walkingSpeed = mlAgentsConfig.TargetWalkingSpeed;
        _movementAgent = gameObject.GetComponent<GenericAgent>();
        _movementAgent.MTargetWalkingSpeed = _walkingSpeed;
    }
    // Start is called before the first frame update
    void Start()
    {
        _path = new NavMeshPath();
        _target = GameObject.Find("Player").transform;

        foreach (var bone in GetComponentsInChildren<Bone>())
        {
            if (bone.isRoot)
            {
                _rootElementTransform = bone.transform;
                break;
            }
        }

        StartCoroutine(ReCalculatePath());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 GetNextWayPoint(Vector3 oldWayPoint)
    {
        if( _path.status == NavMeshPathStatus.PathInvalid)
        {   
            return oldWayPoint;
        }
        if(_pathCornerIndex < _path.corners.Length - 1 && Vector3.Distance(_rootElementTransform.position, _path.corners[_pathCornerIndex]) < 4.5f)
        {
            Debug.Log("Increased path corner index");
            _pathCornerIndex++;
        }
        return _path.corners[_pathCornerIndex];
    }

    public float GetWalkingSpeed()
    {
        return _walkingSpeed;
    }

    public void TakeDamage(float damage)
    {
        _health -= damage;
        if(_health <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    private IEnumerator ReCalculatePath()
    {
        var oldPath = _path;
        bool pathValid = NavMesh.CalculatePath(_rootElementTransform.position, _target.position, NavMesh.AllAreas, _path);
        if (!pathValid)
        {
            _path = oldPath;
            Debug.Log($"Path invalid for {gameObject.name}");
        }
        else
        {
            _pathCornerIndex = 1;
        }

        yield return new WaitForSeconds(1);

        yield return ReCalculatePath(); 
    }
}
