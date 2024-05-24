using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YieldIcon : MonoBehaviour
{
    public Camera camera;


    private void Awake()
    {
        camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = camera.transform.rotation;
    }
}