using UnityEngine;
using System.IO;

// This script handles the capturing of the rgb images
// and the matching depth image for the scene (using the shader)
[RequireComponent(typeof(Camera))]
public class RgbDepthCapture : MonoBehaviour
{
    // The shader is how the depth image is generated
    public Shader depthShader;
    public int totalFrames = 15;
    public string outputFolder = "meta_dataset/task_001";
    public int width = 640;
    public int height = 480;

    private Camera rgbCam;
    private Camera depthCam;
    private RenderTexture renderTex;
    private Texture2D rgbTex;
    private Texture2D depthTex;
    private int frameCount = 0;

    void Start()
    {
        rgbCam = GetComponent<Camera>();

        // Initialize RenderTexture
        renderTex = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        rgbTex = new Texture2D(width, height, TextureFormat.RGB24, false);
        depthTex = new Texture2D(width, height, TextureFormat.RGB24, false);

        // Assign to RGB camera
        rgbCam.targetTexture = renderTex;

        // Create and configure depth camera
        GameObject depthCamGO = new GameObject("DepthCam");
        depthCam = depthCamGO.AddComponent<Camera>();
        depthCam.CopyFrom(rgbCam);
        depthCam.clearFlags = CameraClearFlags.SolidColor;
        depthCam.backgroundColor = Color.white;
        depthCam.cullingMask = rgbCam.cullingMask;
        depthCam.targetTexture = renderTex;
        depthCam.SetReplacementShader(depthShader, ""); 
        depthCam.enabled = false; 
    }

    void LateUpdate()
    {
        if (frameCount >= totalFrames)
            return;

        // Sync depth camera with RGB camera
        depthCam.transform.position = rgbCam.transform.position;
        depthCam.transform.rotation = rgbCam.transform.rotation;

        // Capture RGB 
        rgbCam.Render();
        RenderTexture.active = renderTex;
        rgbTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        rgbTex.Apply();
        File.WriteAllBytes(Path.Combine(Application.dataPath, "..", outputFolder, $"rgb_{frameCount:D4}.png"), rgbTex.EncodeToPNG());

        // Capture Depth
        depthCam.Render();
        RenderTexture.active = renderTex;
        depthTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        depthTex.Apply();
        File.WriteAllBytes(Path.Combine(Application.dataPath, "..", outputFolder, $"depth_{frameCount:D4}.png"), depthTex.EncodeToPNG());

        Debug.Log($"Captured frame {frameCount}");
        frameCount++;
    }


    public void ResetCapture()
    {
        frameCount = 0;
        // Make sure output folder is ready
        Directory.CreateDirectory(Path.Combine(Application.dataPath, "..", outputFolder));
    }


}
