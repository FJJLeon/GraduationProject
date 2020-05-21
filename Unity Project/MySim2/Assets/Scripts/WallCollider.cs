using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCollider : MonoBehaviour
{
    [Tooltip("The amount by which the kart bounces off the wall.  A value of 0.1 means 1.1 times the velocity into the wall is the velocity away from the wall.  A minimum value of 0.1 is suggested.")]
    public float bounciness = 0.1f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
