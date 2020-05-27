using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class guideUnity : MonoBehaviour
{
    public GameObject car;
    public GameObject dataTran;
    public GameObject canvas;

    // Start is called before the first frame update
    void Start()
    {
        if (car == null || dataTran == null || canvas == null)
        {
            Debug.Log("guide module missing gameobject");
            Assert.IsTrue(false);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("b"))
        {
            car.SetActive(true);
            dataTran.SetActive(true);
            canvas.SetActive(true);
        }
    }
}
