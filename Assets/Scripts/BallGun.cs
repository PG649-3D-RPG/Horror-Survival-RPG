using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Unity.VisualScripting;
using UnityEngine;

public class BallGun : MonoBehaviour, IEquipment
{

    private Camera _camera;

    public GameObject projectile;

    public void OnPrimary()
    {
        var origin = _camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
        RaycastHit hit;

        if (Physics.Raycast(origin, _camera.transform.forward, out hit, 100.0f))
        {
            GameObject ball = Instantiate(projectile);
            ball.GetComponent<BallGunProjectile>().SetNormal(hit.normal);
            ball.transform.parent = hit.collider.gameObject.transform;
            ball.transform.position = hit.point;
        }
    }

    public void OnEquip()
    {
        gameObject.SetActive(true);
    }

    public void OnUnequip()
    {
        gameObject.SetActive(false);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _camera = GameObject.Find("Camera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
