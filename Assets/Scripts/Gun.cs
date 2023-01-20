using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Unity.VisualScripting;
using UnityEngine;

public class Gun : MonoBehaviour, IEquipment
{

    private Camera _camera;

    public void OnPrimary()
    {
        var origin = _camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
        RaycastHit hit;

        if (Physics.Raycast(origin, _camera.transform.forward, out hit, 100.0f))
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.parent = hit.collider.gameObject.transform;
            sphere.transform.position = hit.point + hit.normal * sphere.GetComponent<SphereCollider>().radius;
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
