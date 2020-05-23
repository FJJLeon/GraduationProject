using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectMoveControl : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float rotateSpeed = 80f;

    float moveHorizonal;
    float moveVertical;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        moveHorizonal = Input.GetAxis("Horizontal");
        moveVertical = Input.GetAxis("Vertical");

        this.transform.Translate(Vector3.forward * moveVertical * Time.deltaTime * moveSpeed);
        this.transform.Rotate(Vector3.up * moveHorizonal * Time.deltaTime * rotateSpeed);
    }
}
