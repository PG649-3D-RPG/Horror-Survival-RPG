using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Unity.MLAgents;


public class PhysicsController : MonoBehaviour
{
    // Original values
    Vector3 m_OriginalGravity;
    float m_OriginalFixedDeltaTime;
    float m_OriginalMaximumDeltaTime;
    bool m_OriginalReuseCollisionCallbacks;

    [Tooltip("Increase or decrease the scene gravity. Use ~3x to make things less floaty")]
    public float gravityMultiplier = 1.0f;

    [Header("Advanced physics settings")]
    [Tooltip("The interval in seconds at which physics and other fixed frame rate updates (like MonoBehaviour's FixedUpdate) are performed.")]
    public float fixedDeltaTime = 0.008333f;
    [Tooltip("The maximum time a frame can take. Physics and other fixed frame rate updates (like MonoBehaviour's FixedUpdate) will be performed only for this duration of time per frame.")]
    public float maximumDeltaTime = 0.01f;
    [Tooltip("Determines whether the garbage collector should reuse only a single instance of a Collision type for all collision callbacks. Reduces Garbage.")]
    public bool reuseCollisionCallbacks = true;

    void Awake()
    {
        // Save the original values
        m_OriginalGravity = Physics.gravity;
        m_OriginalFixedDeltaTime = Time.fixedDeltaTime;
        m_OriginalMaximumDeltaTime = Time.maximumDeltaTime;
        m_OriginalReuseCollisionCallbacks = Physics.reuseCollisionCallbacks;

        // Override
        Physics.gravity *= gravityMultiplier;
        Time.fixedDeltaTime = fixedDeltaTime;
        Time.maximumDeltaTime = maximumDeltaTime;
        Physics.reuseCollisionCallbacks = reuseCollisionCallbacks;

        // Make sure the Academy singleton is initialized first, since it will create the SideChannels.
        //Academy.Instance.EnvironmentParameters.RegisterCallback("gravity", f => { Physics.gravity = new Vector3(0, -f, 0); });

    }

    public void OnDestroy()
    {
        Physics.gravity = m_OriginalGravity;
        Time.fixedDeltaTime = m_OriginalFixedDeltaTime;
        Time.maximumDeltaTime = m_OriginalMaximumDeltaTime;
        Physics.reuseCollisionCallbacks = m_OriginalReuseCollisionCallbacks;
    }
}




