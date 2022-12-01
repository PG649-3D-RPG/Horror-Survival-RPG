using System;
using Unity.MLAgentsExamples;
using UnityEngine;

public class CreatureFactory : MonoBehaviour
{

    public string AgentScriptName = "AgentNew";
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
        
        if (root.AddComponent(Type.GetType(AgentScriptName)) == null)
        {
            throw new ArgumentException("Agent class name is wrong or does not exist in this context.");
        }

        // TODO: Figure out if this is actually needed and/or harmful
        if (root.GetComponent<ModelOverrider>() == null)
        {
            root.AddComponent<ModelOverrider>();
        }
        
        // TODO(markus): Place somewhere it cant be seen
        // TODO(markus): Add to some type of datastructure to keep track of creatures
    } 
}