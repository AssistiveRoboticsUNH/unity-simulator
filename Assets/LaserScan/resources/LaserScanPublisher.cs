using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine.Jobs;
using Unity.Jobs;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

[RequireComponent(typeof(RotateLidar))]
public class LaserScanPublisher : ROSPublisher
{
    [SerializeField] public string _topicName = "scan";
    [SerializeField] public string _frameId = "scan_link";
    public float angle_min;
    public float angle_max;
    public float angle_increment;
    public float time_increment;
    public float scan_time;
    public float range_min;
    public float range_max;
    public float[] ranges;
    public float[] intensities;

    private JobHandle _handle;
    private float _timeElapsed = 0f;
    private double _timeStamp = 0f;

    private RotateLidar _lidar;

    float Deg2Rad(float deg)
    {
        return deg * Mathf.PI / 180f;
    }

    void Start()
    {
        // Get Rotate Lidar
        this._lidar = GetComponent<RotateLidar>();
        this._lidar.Init();

        angle_min = Deg2Rad(this._lidar.minAzimuthAngle);
        angle_max = Deg2Rad(this._lidar.maxAzimuthAngle);
        var deg_increment = (this._lidar.maxAzimuthAngle - this._lidar.minAzimuthAngle) /
                            (float)this._lidar.numOfIncrements;
        angle_increment = Deg2Rad(deg_increment);
        time_increment = 1f / this._lidar.scanRate / (float)this._lidar.numOfIncrements;
        scan_time = 1f / this._lidar.scanRate;
        range_min = this._lidar.minRange;
        range_max = this._lidar.maxRange;
    }

    void OnDisable()
    {
        this._handle.Complete();
        this._lidar.Dispose();
    }


    void Update()
    {
        this._timeElapsed += Time.deltaTime;


        if (_timeElapsed > 1.0 / _lidar.scanRate)
        {
            _handle.Complete();
            // Update ROS Message
            int sec = (int)Math.Truncate(this._timeStamp);
            uint nanosec = (uint)((this._timeStamp - sec) * 1e+9);
            
            
            ranges = this._lidar.distances.ToArray();
            intensities = this._lidar.intensities.ToArray();
            Publish();
            
            // Update time
            this._timeElapsed = 0;
            this._timeStamp = Time.timeAsDouble;

            // Update Raycast Command
            for (int incr = 0; incr < this._lidar.numOfIncrements; incr++)
            {
                for (int layer = 0; layer < this._lidar.numOfLayers; layer++)
                {
                    int index = layer + incr * this._lidar.numOfLayers;
                    this._lidar.commands[index] =
                        new RaycastCommand(this.transform.position,
                            this.transform.rotation * this._lidar.commandDirVecs[index],
                            this._lidar.maxRange);
                }
            }

            // Update Parallel Jobs
            var raycastJobHandle = RaycastCommand.ScheduleBatch(this._lidar.commands, this._lidar.results, 360);
            // Update Distance data
            if (this._lidar.randomSeed++ == 0)
                this._lidar.randomSeed = 1;
            this._lidar.job.random.InitState(this._lidar.randomSeed);
            this._handle = this._lidar.job.Schedule(this._lidar.results.Length, 360, raycastJobHandle);
            JobHandle.ScheduleBatchedJobs();
        }
    }
}