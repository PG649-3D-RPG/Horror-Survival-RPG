using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Config;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgentsExamples;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class GenericAgent : Agent
{
    
    // Internal values
    protected float _otherBodyPartHeight = 1f;
    protected Vector3 _topStartingPosition;
    protected Quaternion _topStartingRotation;
    protected Transform _topTransform;
    protected Rigidbody _topTransformRb;

    protected long _episodeCounter = 0;
    protected Vector3 _dirToWalk = Vector3.right;


    // Scripts
    protected Transform _target;
    protected OrientationCubeController _orientationCube;
    protected JointDriveController _jdController;
    protected DecisionRequester _decisionRequester;
    protected Agent _agent;
    protected MlAgentConfig _mlAgentsConfig;
    protected ArenaConfig _arenaSettings;
    protected CreatureConfig _creatureConfig;

    public float MTargetWalkingSpeed;

    
    public void Awake()
    {
        if(GetComponent<JointDriveController>() != null) Destroy(GetComponent<DecisionRequester>());
        if (GetComponent<DecisionRequester>() != null) Destroy(GetComponent<JointDriveController>());

        _decisionRequester = this.AddComponent<DecisionRequester>();
        _jdController = this.AddComponent<JointDriveController>();
        _mlAgentsConfig = FindObjectOfType<MlAgentConfig>();
        _arenaSettings = FindObjectOfType<ArenaConfig>();
        _creatureConfig = FindObjectOfType<CreatureConfig>();


        // Config decision requester
        _decisionRequester.DecisionPeriod = _mlAgentsConfig.DecisionPeriod;
        _decisionRequester.TakeActionsBetweenDecisions = _mlAgentsConfig.TakeActionsBetweenDecisions;
        
        // Config jdController
        _jdController.maxJointForceLimit = _mlAgentsConfig.MaxJointForceLimit;
        _jdController.jointDampen = _mlAgentsConfig.JointDampen;
        _jdController.maxJointSpring = _mlAgentsConfig.MaxJointSpring;
        
        // Set agent settings (maxSteps)
        var mAgent = gameObject.GetComponent<Agent>();
        mAgent.MaxStep = _mlAgentsConfig.MaxStep;

        // Set behavior parameters
        var bpScript = GetComponent<BehaviorParameters>();
        bpScript.BrainParameters.ActionSpec = new ActionSpec(_mlAgentsConfig.ContinuousActionSpace, new int[_mlAgentsConfig.DiscreteBranches]);
        bpScript.BrainParameters.VectorObservationSize = _mlAgentsConfig.ObservationSpace;
        bpScript.BehaviorName = "Walker";
        // TODO bpScript.Model = _deg.NnModel;
    }

    public override void Initialize()
    {
        var parent = transform.parent;
        _agent = gameObject.GetComponent<Agent>();
        _target = parent.Find("Creature Target").transform;
        MTargetWalkingSpeed = _mlAgentsConfig.TargetWalkingSpeed;
        var oCube = transform.Find("Orientation Cube");
        _orientationCube = oCube.GetComponent<OrientationCubeController>();
        if(_orientationCube == null) _orientationCube = oCube.AddComponent<OrientationCubeController>();

        //Get Body Parts
        //and setup each body part
        var minYBodyPartCoordinate = 0f;
        foreach (var bone in GetComponentsInChildren<Bone>())
        {
            if (!bone.isRoot)
            {
                if (bone.transform.GetComponent<GroundContact>() == null) bone.transform.AddComponent<GroundContact>();
                _jdController.SetupBodyPart(bone.transform);

            }
            else
            {
                _topTransform = bone.transform;
                _topTransformRb = bone.transform.GetComponent<Rigidbody>();

                _topStartingRotation = bone.transform.localRotation;
                _topStartingPosition = bone.transform.position;
            }
            minYBodyPartCoordinate = Math.Min(minYBodyPartCoordinate, bone.transform.position.y);
        }

        foreach(var (trans, bodyPart) in _jdController.bodyPartsDict)
        {
            bodyPart.BodyPartHeight = trans.position.y - minYBodyPartCoordinate;
        }

        _otherBodyPartHeight = _topTransform.position.y - minYBodyPartCoordinate;
        SetWalkerOnGround();
    }
    
    /// <summary>
    /// Set the walker on the terrain.
    /// </summary>
    protected void SetWalkerOnGround()
    {
        var position = _topTransform.position;
        var terrainHeight = 0; // TODO _terrainGenerator.GetTerrainHeight(position);

        position = new Vector3(_topStartingPosition.x, terrainHeight + _otherBodyPartHeight + 0, _topStartingPosition.z);
        _topTransform.position = position;
        _topTransform.localRotation = _topStartingRotation;


        _topTransformRb.velocity = Vector3.zero;
        _topTransformRb.angularVelocity = Vector3.zero;

        //Reset all of the body parts
        foreach (var bodyPart in _jdController.bodyPartsDict.Values.AsParallel())
        {
            bodyPart.Reset(bodyPart, terrainHeight, 0);
        }

        var rotation = new Vector3(_topStartingRotation.eulerAngles.x, Random.Range(0.0f, 360.0f),
            _topStartingRotation.eulerAngles.z + Random.Range(-5, 5));
        while (rotation == Vector3.zero)
        {
            rotation = new Vector3(_topStartingRotation.eulerAngles.x, Random.Range(0.0f, 360.0f),
                _topStartingRotation.eulerAngles.z + Random.Range(-5, 5));
            Debug.LogError("Fixing zero vector rotation!");
        }
        _topTransform.localRotation = Quaternion.Euler(rotation);
    }

    /// <summary>
    /// Is called on episode begin.
    /// Loop over body parts and reset them to initial conditions.
    /// Regenerate terrain and place target cube randomly 
    /// </summary>
    public override void OnEpisodeBegin()
    {
        _episodeCounter++;

        SetWalkerOnGround();

        //Set our goal walking speed
        MTargetWalkingSpeed =
            _mlAgentsConfig.RandomizeWalkSpeedEachEpisode ? Random.Range(0.1f, _mlAgentsConfig.MaxWalkingSpeed) : MTargetWalkingSpeed;
    }
    
    protected Vector3 GetAvgVelocityOfCreature()
    {
        return _jdController.bodyPartsList.Select(x => x.rb.velocity)
            .Aggregate(Vector3.zero, (x, y) => x + y) / _jdController.bodyPartsList.Count;
    }

    /// <summary>
    /// Agent touched the target
    /// </summary>
    public void TouchedTarget()
    {
        AddReward(1f);
    }

    void Start()
    {
        _ = StartCoroutine(nameof(CheckWalkerOutOfArea));
    }


    private IEnumerator CheckWalkerOutOfArea()
    {
        while (true)
        {
            if (_topTransform.position.y is < -10 or > 40)
            {
                _agent.EndEpisode();
            }
            yield return new WaitForFixedUpdate();
        }
    }
}
