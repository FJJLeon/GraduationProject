using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testMoveCar : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        move();
    }

    void move()
    {
        this.gameObject.transform.Translate(Vector3.forward * Time.deltaTime * 1f);
        this.gameObject.transform.Translate(Vector3.right * Time.deltaTime * 1f);
        this.gameObject.transform.Rotate(new Vector3(0, 1, 0) * Time.deltaTime * 10f);
        //GetComponent<Rigidbody>().AddForceAtPosition(new Vector3(0, 0, 1), transform.position + transform.forward, ForceMode.VelocityChange);
    }
}
