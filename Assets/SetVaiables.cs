using System;
using System.Runtime.InteropServices;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;

public class SendVariables : MonoBehaviour
{
    public bool eating = false;
    public bool taking_medicine = false;
    public GameObject obj;

    public float publishMessageFrequency = 1.0f / 60.0f;
    public string topicName_eating = "person_eating";
    public string topicName_medicine = "person_taking_medicine";
    private float timeElapsed;

    private std_msgs_Bool msg_eating;
    private std_msgs_Bool msg_taking_med;

    void Start()
    {
        msg_eating = new std_msgs_Bool();
        msg_eating.data = eating;
        msg_taking_med = new std_msgs_Bool();
        msg_taking_med.data = taking_medicine;
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency)
        {
            msg_eating.data = eating;
            msg_taking_med.data = taking_medicine;
            ROSInterface.PublishROS(ref msg_eating, topicName_eating);
            ROSInterface.PublishROS(ref msg_taking_med, topicName_medicine);
            timeElapsed = 0 ;

        }
    }
}