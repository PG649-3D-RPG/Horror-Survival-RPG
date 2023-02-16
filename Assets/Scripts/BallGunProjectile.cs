using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallGunProjectile : MonoBehaviour
{
    public float BlowUpTime = 1.0f;
    public float DeflateTime = 1.0f;
    public float Lifetime = 10.0f;
    
    
    private float _time;
    private Vector3 _attachmentNormal;
    private float _radius;
    private Vector3 _startPosition;

    public void SetNormal(Vector3 n)
    {
        _attachmentNormal = n;
    }

    private void Start()
    {
        _radius = GetComponentInChildren<SphereCollider>().radius;
        _startPosition = transform.localPosition;
    }

    void Update()
    {
        _time += Time.deltaTime;
        transform.localScale = Vector3.one * Scale(_time);
        transform.localPosition = _startPosition + _attachmentNormal * (_radius * Scale(_time));

        if (_time > Lifetime)
        {
            Destroy(gameObject);
        }
    }

    private float Scale(float t)
    {
        if (t <= BlowUpTime)
        {
            return t / BlowUpTime;
        }
        
        if (t >= Lifetime - DeflateTime)
        {
            return (Lifetime - t) / DeflateTime;
        }

        return 1.0f;
    }
}
