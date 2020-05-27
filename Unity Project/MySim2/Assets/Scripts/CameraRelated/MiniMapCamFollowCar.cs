using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCamFollowCar : MonoBehaviour
{
    [Header("Follow Target Car")]
    public GameObject followedCar;

    // Start is called before the first frame update
    void Start()
    {
        if (followedCar == null)
        {
            Debug.Log("MiniMap Camera follow target car missing.");
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // Debug.Log("car change to x:" + followedCar.transform.position.x + ", z: " + followedCar.transform.position.z);
        this.gameObject.GetComponent<Transform>().position = new Vector3(followedCar.transform.position.x, 0, followedCar.transform.position.z);
    }
}
