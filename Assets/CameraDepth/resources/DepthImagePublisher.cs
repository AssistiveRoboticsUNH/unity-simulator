using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class DepthImagePublisher : ROSPublisher
{
    public string topicName = "unity_camera/depth/image_raw";
    // public string cameraInfoTopicName = "unity_camera/rgb/camera_info";

    // The game object
    public Camera ImageCamera;
    public string FrameId = "unity_camera/depth_frame";
    public int width = 640;
    public int height = 480;
    public float publishMessageFrequency = 1.0f/20.0f;
    
    public byte[] data;
    public int step;
    private float timeElapsed;
    private Texture2D texture2D;
    private Rect rect;
    private RenderTexture finalRT;
    private ROSInterface ros_interface;

    [Header("Shader Setup")] public Shader uberReplacementShader;

    static private void SetupCameraWithReplacementShader(Camera cam, Shader shader, Color clearColor)
    {
        
        int mode = 2;
        var cb = new CommandBuffer();
        cb.SetGlobalFloat("_OutputMode", mode); 
        cam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, cb);
        cam.AddCommandBuffer(CameraEvent.BeforeFinalPass, cb);
        cam.SetReplacementShader(shader, "");
        cam.backgroundColor = clearColor;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.allowHDR = false;
        cam.allowMSAA = false;
    }

    void Start()
    {
        ros_interface = FindObjectOfType<ROSInterface>();
        texture2D = new Texture2D(width, height, TextureFormat.RFloat, false);
        data = new byte[width * height * 4];
        rect = new Rect(0, 0, width, height);
        
        if (!uberReplacementShader)
            uberReplacementShader = Shader.Find("Hidden/UberReplacement");
        SetupCameraWithReplacementShader(ImageCamera, uberReplacementShader, Color.white);
        
        bool supportsAntialiasing = true;
        bool needsRescale = false;
        var depth = 32;
        var format = RenderTextureFormat.Default;
        var readWrite = RenderTextureReadWrite.Default;
        var antiAliasing = (supportsAntialiasing) ? Mathf.Max(1, QualitySettings.antiAliasing) : 1;

        finalRT = RenderTexture.GetTemporary(width, height, depth, format, readWrite, antiAliasing);
    }


    void Update()
    {
        if (texture2D != null)
            UpdateMessage();
    }
    
    private IEnumerator WaitForEndOfFrameAndSave()
    {
        yield return new WaitForEndOfFrame();
        UpdateMessage();
    }

    private void UpdateMessage()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency)
        {
            RenderTexture.active = finalRT;
            ImageCamera.targetTexture = finalRT;
            ImageCamera.Render();
            texture2D.ReadPixels(rect, 0, 0);
            height = texture2D.height;
            width = texture2D.width;
            if (data.Length != height * width * 4)
            {
                data = new byte[height * width * 4];
            }
            string encoding = "32FC1";
            byte is_bigendian = 0;
            step = 4 * width;
            byte[] data_local = texture2D.GetRawTextureData();
            uint ind = 0;
            for (uint y = 0; y < height; y++)
            {
                for (uint x = 0; x < width; x++)
                {
                    long ind2 = (height - y - 1) * width + x;
                    data[ind] = data_local[4 * ind2];
                    data[ind + 1] = data_local[4 * ind2 + 1];
                    data[ind + 2] = data_local[4 * ind2 + 2];
                    data[ind + 3] = data_local[4 * ind2 + 3];
                    ind += 4;
                }
            }

            ros_interface.native_image.height = height; 
            ros_interface.native_image.width = width;

            Publish();
            timeElapsed = 0;
        }
    }
}
