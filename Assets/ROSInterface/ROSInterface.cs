using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using Random = UnityEngine.Random;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using Unity.VisualScripting;

[StructLayout(LayoutKind.Sequential)]
[System.Serializable]
public struct NativeQuaternion
{
    public double x;
    public double y;
    public double z;
    public double w;
}

[StructLayout(LayoutKind.Sequential)]
[System.Serializable]
public struct NativeVector3
{
    public double x;
    public double y;
    public double z;
}

[StructLayout(LayoutKind.Sequential)]
[System.Serializable]
public struct NativeTwist
{
    public NativeVector3 linear;
    public NativeVector3 angular;
}

[StructLayout(LayoutKind.Sequential)]
[System.Serializable]
public struct NativeTransform
{
    public IntPtr frame_id;
    public IntPtr child_frame_id;
    public NativeVector3 translation;
    public NativeQuaternion rotation;
}

[StructLayout(LayoutKind.Sequential)]
[System.Serializable]
public struct NativeOdom
{
    public NativeTransform pose;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 36)]
    public double[] pose_covariance;

    public NativeVector3 linear_velocity;
    public NativeVector3 angular_velocity;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 36)]
    public double[] twist_covariance;
}


[StructLayout(LayoutKind.Sequential)]
[System.Serializable]
public struct NativeInt32
{
    public IntPtr topic;
    public int data;
}

[StructLayout(LayoutKind.Sequential)]
[System.Serializable]
public struct NativeImage
{
    public IntPtr topic;
    public int height;
    public int width;
    public int step;
    public IntPtr frame_id;
    public IntPtr encoding;
    public IntPtr data;
}

[StructLayout(LayoutKind.Sequential)]
[System.Serializable]
public struct NativeScan
{
    public IntPtr topic;
    public IntPtr frame_id;
    public float angle_min;
    public float angle_max;
    public float angle_increment;
    public float time_increment;
    public float scan_time;
    public float range_min;
    public float range_max;
    public int count;
    public IntPtr ranges;
    public IntPtr intensities;
}

public class ROSPublisher : MonoBehaviour
{
    public bool should_publish;

    public void Publish()
    {
        should_publish = true;
    }
}

public class ROSInterface : MonoBehaviour
{
    // interface
    public List<Transform> transforms;
    public ImagePublisher[] color_cameras;
    public DepthImagePublisher[] depth_cameras;
    public LaserScanPublisher[] laser_scanners;
    public Animator animator;
    public Transform odom_transform;

    // native
    private IntPtr handle;
    private NativeTransform native_transform;
    private NativeInt32 native_eating;
    private NativeInt32 native_taking_medicine;
    private NativeOdom native_odom;
    public NativeTwist native_twist;
    public NativeImage native_image;
    public NativeScan native_scan;

    // helper
    private Vector3 prev_forward;
    private Vector3 prev_position;

    void Start()
    {
        color_cameras = FindObjectsOfType<ImagePublisher>();
        depth_cameras = FindObjectsOfType<DepthImagePublisher>();
        laser_scanners = FindObjectsOfType<LaserScanPublisher>();
        
        prev_forward = new Vector3();
        prev_position = new Vector3();
        prev_position = odom_transform.position;
        prev_forward = odom_transform.forward;

        handle = Init();
        native_transform = new NativeTransform();

        native_eating.topic = Marshal.StringToHGlobalAnsi("person_eating");
        native_taking_medicine.topic = Marshal.StringToHGlobalAnsi("person_taking_medicine");


        native_odom = new NativeOdom();
        native_odom.pose.frame_id = Marshal.StringToHGlobalAnsi("odom");
        native_odom.pose.child_frame_id = Marshal.StringToHGlobalAnsi(odom_transform.name);
        native_odom.pose_covariance = new double[36];
        native_odom.pose_covariance[0] = 0.1;
        native_odom.pose_covariance[7] = 0.1;
        native_odom.pose_covariance[14] = 1000000000000.0;
        native_odom.pose_covariance[21] = 1000000000000.0;
        native_odom.pose_covariance[28] = 1000000000000.0;
        native_odom.pose_covariance[35] = 1000000000000.0;
        native_odom.twist_covariance = new double[36];
        native_odom.twist_covariance[0] = 1000000000000.0;
        native_odom.twist_covariance[7] = 1000000000000.0;
        native_odom.twist_covariance[14] = 1000000000000.0;
        native_odom.twist_covariance[21] = 1000000000000.0;
        native_odom.twist_covariance[28] = 1000000000000.0;
        native_odom.twist_covariance[35] = 1000000000000.0;
    }

    void FixedUpdate()
    {
        var velocity = (odom_transform.position - prev_position) / Time.deltaTime;
        var tmp = Vector3.Dot(odom_transform.forward, prev_forward);
        tmp = Math.Max(tmp, 0.0f);
        tmp = Math.Min(tmp, 1.0f);
        var omega = Math.Acos(tmp) / Time.deltaTime;
        var angular_velocity = Vector3.Cross(odom_transform.forward, prev_forward);
        angular_velocity.Normalize();

        Vector3 tmp2 = new Vector3();
        tmp2.x = Vector3.Dot(odom_transform.forward, velocity);
        tmp2.y = Vector3.Dot(odom_transform.up, velocity);
        tmp2.z = Vector3.Dot(odom_transform.right, velocity);
        var flu = tmp2; //.To<FLU>();
        native_odom.linear_velocity.x = flu.x;
        native_odom.linear_velocity.y = flu.y;
        native_odom.linear_velocity.z = flu.z;

        Vector3 tmp3 = new Vector3();
        tmp3.x = (float)omega * angular_velocity.x;
        tmp3.y = (float)omega * angular_velocity.y;
        tmp3.z = (float)omega * angular_velocity.z;
        flu = tmp3; //.To<FLU>();
        native_odom.angular_velocity.x = flu.x;
        native_odom.angular_velocity.y = flu.y;
        native_odom.angular_velocity.z = flu.z;

        prev_position = odom_transform.position;
        prev_forward = odom_transform.forward;
    }


    void SetTransform(Transform trans, ref NativeTransform native)
    {
        var translation = new NativeVector3();
        var position_FLU = trans.position.To<FLU>();
        translation.x = position_FLU.x;
        translation.y = position_FLU.y;
        translation.z = position_FLU.z;
        native.translation = translation;

        var rotation = new NativeQuaternion();
        var rotation_FLU = trans.rotation.To<FLU>();
        rotation.w = rotation_FLU.w;
        rotation.x = rotation_FLU.x;
        rotation.y = rotation_FLU.y;
        rotation.z = rotation_FLU.z;
        native.rotation = rotation;
    }

    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Space)) {
        foreach (var trans in transforms)
        {
            native_transform.frame_id = Marshal.StringToHGlobalAnsi(trans.name);
            native_transform.child_frame_id = Marshal.StringToHGlobalAnsi("odom");
            SetTransform(trans, ref native_transform);
            PublishTF(handle, ref native_transform);

            Marshal.FreeHGlobal(native_transform.frame_id);
            Marshal.FreeHGlobal(native_transform.child_frame_id);
        }

        native_eating.data = 0;
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("eating"))
        {
            native_eating.data = 1;
        }
        PublishInt32(handle, ref native_eating);
        
        native_taking_medicine.data = 0;
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("pill"))
        {
            native_taking_medicine.data = 1;
        }
        PublishInt32(handle, ref native_taking_medicine);
        

        SetTransform(odom_transform, ref native_odom.pose);
        PublishOdom(handle, ref native_odom);

        // if (Input.GetKeyDown(KeyCode.Space))
        ReceiveCmdVel(handle, ref native_twist);
        
        foreach (var cam in color_cameras)
        {
            if (cam.should_publish)
            {
                native_image.height = cam.height;
                native_image.width = cam.width;
                native_image.step = cam.step;
                native_image.topic = Marshal.StringToHGlobalAnsi(cam.topicName);
                native_image.frame_id = Marshal.StringToHGlobalAnsi(cam.FrameId);
                native_image.data = Marshal.AllocHGlobal(cam.data.Length);
                native_image.encoding = Marshal.StringToHGlobalAnsi("rgba8");
                Marshal.Copy(cam.data, 0, native_image.data, cam.data.Length);
                PublishImage(handle, ref native_image);
                Marshal.FreeHGlobal(native_image.topic);
                Marshal.FreeHGlobal(native_image.frame_id);
                Marshal.FreeHGlobal(native_image.data);
                Marshal.FreeHGlobal(native_image.encoding);
                cam.should_publish = false;
            }
        }
        
        foreach (var cam in depth_cameras)
        {
            if (cam.should_publish)
            {
                native_image.height = cam.height;
                native_image.width = cam.width;
                native_image.step = cam.step;
                native_image.topic = Marshal.StringToHGlobalAnsi(cam.topicName);
                native_image.frame_id = Marshal.StringToHGlobalAnsi(cam.FrameId);
                native_image.data = Marshal.AllocHGlobal(cam.data.Length);
                Marshal.Copy(cam.data, 0, native_image.data, cam.data.Length);
                native_image.encoding = Marshal.StringToHGlobalAnsi("32FC1");
                PublishImage(handle, ref native_image);
                Marshal.FreeHGlobal(native_image.topic);
                Marshal.FreeHGlobal(native_image.frame_id);
                Marshal.FreeHGlobal(native_image.data);
                Marshal.FreeHGlobal(native_image.encoding);
                cam.should_publish = false;
            }
        }
        
        foreach (var laser in laser_scanners)
        {
            if (laser.should_publish)
            {
                native_scan.topic = Marshal.StringToHGlobalAnsi(laser._topicName);
                native_scan.frame_id = Marshal.StringToHGlobalAnsi(laser._frameId);
                native_scan.angle_min = laser.angle_min;
                native_scan.angle_max = laser.angle_max;
                native_scan.angle_increment = laser.angle_increment;
                native_scan.time_increment= laser.time_increment;
                native_scan.scan_time = laser.scan_time;
                native_scan.range_min = laser.range_min;
                native_scan.range_max = laser.range_max;
                int sizeInBytes = Marshal.SizeOf(laser.ranges[0]) * laser.ranges.Length;
                native_scan.ranges = Marshal.AllocHGlobal(sizeInBytes);
                Marshal.Copy(laser.ranges, 0, native_scan.ranges, laser.ranges.Length);
                sizeInBytes = Marshal.SizeOf(laser.intensities[0]) * laser.intensities.Length;
                native_scan.intensities = Marshal.AllocHGlobal(sizeInBytes);
                Marshal.Copy(laser.intensities, 0, native_scan.intensities, laser.intensities.Length);
                native_scan.count = laser.intensities.Length;
                PublishScan(handle, ref native_scan);
                Marshal.FreeHGlobal(native_scan.ranges);
                Marshal.FreeHGlobal(native_scan.intensities);
                Marshal.FreeHGlobal(native_scan.topic);
                laser.should_publish = false;
            }
        }

    }

    [DllImport("libROSInterface.so", EntryPoint = "Init", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr Init();

    [DllImport("libROSInterface.so", EntryPoint = "Destroy", CallingConvention = CallingConvention.Cdecl)]
    private static extern void Destroy(IntPtr handle);

    [DllImport("libROSInterface.so", EntryPoint = "PublishTF", CallingConvention = CallingConvention.Cdecl)]
    private static extern void PublishTF(IntPtr handle, ref NativeTransform input);

    [DllImport("libROSInterface.so", EntryPoint = "PublishInt32", CallingConvention = CallingConvention.Cdecl)]
    private static extern void PublishInt32(IntPtr handle, ref NativeInt32 input);

    [DllImport("libROSInterface.so", EntryPoint = "PublishOdom", CallingConvention = CallingConvention.Cdecl)]
    private static extern void PublishOdom(IntPtr handle, ref NativeOdom input);

    [DllImport("libROSInterface.so", EntryPoint = "PublishImage", CallingConvention = CallingConvention.Cdecl)]
    private static extern void PublishImage(IntPtr handle, ref NativeImage input);
    
    [DllImport("libROSInterface.so", EntryPoint = "PublishScan", CallingConvention = CallingConvention.Cdecl)]
    private static extern void PublishScan(IntPtr handle, ref NativeScan input);
    
    [DllImport("libROSInterface.so", EntryPoint = "ReceiveCmdVel", CallingConvention = CallingConvention.Cdecl)]
    private static extern void ReceiveCmdVel(IntPtr handle, ref NativeTwist output);
}
