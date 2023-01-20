using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{

    public FPSController Controller;
    
    public void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.GetComponent<Bone>())
        {
            Controller.OnDamage();
        }
    }
}
