using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectMoveControl : MonoBehaviour
{
    public float moveSpeed = 12f;
    public float rotateSpeed = 50f;
    public bool direct = true;

    private Rigidbody carRigid;
    float moveHorizonal;
    float moveVertical;
    // Start is called before the first frame update
    void Start()
    {
        carRigid = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        moveVertical = Input.GetAxis("Vertical"); // up, down
        moveHorizonal = Input.GetAxis("Horizontal"); // left right
        //Debug.Log("input v:" + moveVertical + " h:" + moveHorizonal);

        if (direct)
        {
            directMove();
        }
        else
        {
            velocMove();
        }
        
    }

    void directMove()
    {
        // move
        this.transform.Translate(Vector3.forward * moveVertical * Time.deltaTime * moveSpeed / 3);
        // rotate
        this.transform.Rotate(Vector3.up * moveHorizonal * Time.deltaTime * rotateSpeed);
    }

    void velocMove()
    {
        carRigid.velocity = this.transform.forward * moveVertical * moveSpeed;
        carRigid.angularVelocity = this.transform.up * moveHorizonal * rotateSpeed;
    }
}
