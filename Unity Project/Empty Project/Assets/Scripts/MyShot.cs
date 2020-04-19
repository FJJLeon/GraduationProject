using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyShot : MonoBehaviour
{

    public GameObject bullet;

    public float ShotSpeed = 40;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Hello World");
        // GameObject.Instantiate(bullet, transform.position, transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameObject b = GameObject.Instantiate(bullet, transform.position, transform.rotation);
            Rigidbody rgd = b.GetComponent<Rigidbody>();
            rgd.velocity = transform.forward * ShotSpeed;
        }
    }
}
