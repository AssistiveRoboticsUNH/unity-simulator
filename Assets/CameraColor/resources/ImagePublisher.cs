using UnityEngine;

public class ImagePublisher : ROSPublisher
{
    public string topicName = "unity_camera/color/image_raw";
    // public string cameraInfoTopicName = "unity_camera/rgb/camera_info";

    // The game object
    public Camera ImageCamera;
    public string FrameId = "unity_camera/color_frame";
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

    void Start()
    {
        ros_interface = FindObjectOfType<ROSInterface>();
        texture2D = new Texture2D(width, height, TextureFormat.RGBA32, false);
        data = new byte[width * height * 4];
        rect = new Rect(0, 0, width, height);

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
            string encoding = "rgba8";
            byte is_bigendian = 0;
            step = 4 * width;
            Color32[] colors = texture2D.GetPixels32();
            uint ind = 0;
            for (uint y = 0; y < height; y++)
            {
                for (uint x = 0; x < width; x++)
                {
                    long ind2 = (height - y - 1) * width + x;
                    data[ind] = colors[ind2].r;
                    data[ind + 1] = colors[ind2].g;
                    data[ind + 2] = colors[ind2].b;
                    data[ind + 3] = colors[ind2].a;
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
