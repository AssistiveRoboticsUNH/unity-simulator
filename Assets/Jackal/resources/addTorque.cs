using System;
using System.Collections;
using System.Collections.Generic;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using UnityEngine;


public class DiffDriveControllerJackal : MonoBehaviour
{
    UnityEngine.ArticulationBody myArticulationBody;
    public ArticulationBody wheelLeftRear;
    public ArticulationBody wheelRightRear;
    public ArticulationBody wheelLeftFront;
    public ArticulationBody wheelRightFront;
    public ArticulationBody body;

    public float wheelRadius = 0.1f;

    public float wheelSeparation = .55f;//.37f; real value
    // public float gain = .5f;

    private Vector3 linearVel = new Vector3(0, 0, 0);
    private Vector3 angularVel = new Vector3(0, 0, 0);
    // private Vector3 torque = new Vector3(0, 0, 0);

    private List<float> velocities;
    private List<float> forces;
    private List<int> startIndices;
    
    private ROSInterface ros_interface;

    // Start is called before the first frame update
    void Start()
    {
        velocities = new List<float>();
        body.GetDriveTargetVelocities(velocities);
        forces = new List<float>();
        body.GetJointForces(forces);
        startIndices = new List<int>();
        body.GetDofStartIndices(startIndices);
        ros_interface = FindObjectOfType<ROSInterface>();
    }

    void SetVelocity()
    {
        var twist = ros_interface.native_twist;
        angularVel.z = (float) twist.angular.x;
        angularVel.x = (float)twist.angular.y;
        angularVel.y = (float)twist.angular.z;

        linearVel.z = (float)twist.linear.x;
        linearVel.x = (float)twist.linear.y;
        linearVel.y = (float)twist.linear.z;

    }
    // Update is called once per frame
    void Update()
    {
        
        // linearVel.z = .0f;
        // angularVel.y = -.2f;
        SetVelocity();
        
        float velLeft = (linearVel.z - angularVel.y * wheelSeparation / 2.0f) / wheelRadius;
        float velRight = (linearVel.z + angularVel.y * wheelSeparation / 2.0f) / wheelRadius;


        velocities[startIndices[wheelLeftRear.index]] = velLeft;
        velocities[startIndices[wheelLeftFront.index]] = velLeft;
        velocities[startIndices[wheelRightRear.index]] = velRight;
        velocities[startIndices[wheelRightFront.index]] = velRight;
        body.SetDriveTargetVelocities(velocities);
        
    }
}