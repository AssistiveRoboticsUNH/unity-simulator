using System;
using UnityEngine;

public class SendTrasnform : MonoBehaviour
{
    public GameObject obj;
    void Start()
    {
        
    }

    void Update()
    {
        ROSInterface.SendTransform(obj.transform);
        
    }
}