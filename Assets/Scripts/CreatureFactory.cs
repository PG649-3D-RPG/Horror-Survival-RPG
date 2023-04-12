using System;
using System.Collections.Generic;
using Unity.MLAgents.Policies;
using Unity.MLAgentsExamples;
using Unity.VisualScripting;
using UnityEngine;

public class CreatureFactory : MonoBehaviour
{

    public string AgentScriptName = "AgentNavMeshWalking";
    private static readonly Vector3 StoragePosition = new(10000.0f, 0.0f, 0.0f);
    private static readonly Vector3 StorageOffset = new(1000.0f, 0.0f, 0.0f);

    private List<GameObject> Prototypes = new();

    public void AddPrototype(GameObject root)
    {
        if (root.transform.Find("Orientation Cube") == null)
        {
            var orientationCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            orientationCube.name = "Orientation Cube";
            Destroy(orientationCube.GetComponent<Collider>());
            Destroy(orientationCube.GetComponent<MeshRenderer>());
            orientationCube.transform.parent = root.transform;
        } 
        
        //if (root.AddComponent(Type.GetType(AgentScriptName)) == null)
        //{
            //throw new ArgumentException("Agent class name is wrong or does not exist in this context.");
        //}
        
        // TODO: Figure out if this is actually needed and/or harmful
        if (root.GetComponent<ModelOverrider>() == null)
        {
            root.AddComponent<ModelOverrider>();
        }
        
        SetKinematic(root, true);

        var index = Prototypes.Count;
        Prototypes.Add(root);

        root.transform.position = StoragePosition + index * StorageOffset;
    }

    public int GetNumberOfCreatureTypes()
    {
        return Prototypes.Count;
    }

    public GameObject CreateCreature(int index)
    {
        var result = GameObject.Instantiate(Prototypes[index]);
        var controller = result.AddComponent<BasicCreatureController>();
        result.GetComponent<AgentNavMeshWalking>()._creatureController = controller;
        SetKinematic(result, false);
        return result;
    }

    public Func<GameObject> FactoryFor(int index)
    {
        return () => CreateCreature(index);
    }

    private void SetKinematic(GameObject root, bool kinematic)
    {
        var skeleton = root.GetComponentInChildren<Skeleton>();

        foreach (var (_, _, rb, _) in skeleton.Iterator())
        {
            rb.isKinematic = kinematic;
        }
    }
}