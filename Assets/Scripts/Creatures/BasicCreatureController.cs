using System.Collections;
using System.Collections.Generic;
using Config;
using Unity.VisualScripting;
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
    private Rigidbody _rootElementRigidbody;
    private Skeleton _skeleton;
    private Transform _target;
    private float _timeElapsed;

    private float _walkingSpeed;

    public float despawnRadius = 30.0f;
    public float despawnTimer = 10.0f;
    private float _stuckTimer;

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
                _rootElementRigidbody = bone.GetComponent<Rigidbody>();
                _skeleton = bone.GetComponent<Skeleton>();
                break;
            }
        }

        StartCoroutine(ReCalculatePath());
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovementSpeed();
        HandleDespawn();
    }

    public void Despawn()
    {
        StartCoroutine(Die());
    }

    private void HandleDespawn()
    {
        var velocity = _rootElementRigidbody.velocity;
        velocity.y = 0;
        if (velocity.magnitude < 0.33f * _movementAgent.MTargetWalkingSpeed)
        {
            _stuckTimer += Time.deltaTime;
        }
        else
        {
            _stuckTimer = 0.0f;
        }

        if (_stuckTimer >= despawnTimer && Vector3.Distance(_rootElementTransform.position, _target.position) >= despawnRadius)
        {
            StartCoroutine(Die());
        }
         
    }

    private IEnumerator Die()
    {
       foreach (var (go, _, rb, _) in _skeleton.Iterator())
       {
           rb.isKinematic = true;
           Destroy(go.GetComponent<Collider>());
       }

       const float sinkSpeed = 0.5f;
       var distance = 0.0f;
       // Sink into ground
       while (distance <= 5.0f)
       {
           this.transform.position += Vector3.down * (sinkSpeed * Time.deltaTime);
           distance += sinkSpeed * Time.deltaTime;
           yield return null;
       }
       
       // Remove self
       // NOTE: If we ever start storing references to creatures somewhere, me will want to start keeping a list of gameobjects
       // to be destroyed and only destroy them at the end of the frame so that
       // 1. Other gameobjects can finish their update
       // 2. We can notify other gameobjects that this one has been destroyed
       Destroy(this.gameObject);
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

    private void HandleMovementSpeed()
    {
        if(Vector3.Distance(_target.position, _rootElementTransform.position) >= 10)
        {
            _movementAgent.MTargetWalkingSpeed = 5f;
        }
        else
        {
            _movementAgent.MTargetWalkingSpeed = 10f;
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
