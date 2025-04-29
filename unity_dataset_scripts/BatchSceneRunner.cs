using UnityEngine;
using System.Collections;

// This is like the master script that connects 
// everything together. It will generate the 200
// seperate scenes by calling the corresponding 
// scenes that will generate a random scene and
// then adjust the camera and lighting and then 
// captuer the RGB and depth images 
public class BatchSceneRunner : MonoBehaviour
{
    // 200 scenes with 15 images each for 3,000 image pairs
    public int totalScenes = 200;
    public int imagesPerScene = 15;

    // Scripts for generating scene, moving camera, and capturing images 
    public SceneRandomizer sceneRandomizer;
    public CameraLightingController cameraController;
    public RgbDepthCapture captureScript;

    private int currentScene = 0;
    private bool isGenerating = false;

    void Start()
    {
        StartCoroutine(RunAllScenes());
    }

    IEnumerator RunAllScenes()
    {
        // Do 200 scenes 
        for (currentScene = 1; currentScene <= totalScenes; currentScene++)
        {
            Debug.Log($"[BatchRunner] Starting Scene {currentScene}");

            // Save each scene in its own folder 
            string folder = $"meta_dataset/task_{currentScene:D3}";
            captureScript.outputFolder = folder;
            cameraController.totalViews = imagesPerScene;
            captureScript.totalFrames = imagesPerScene;
            Debug.Log($"[BatchRunner] Output folder: {folder}");

            // Generate the scene
            sceneRandomizer.ClearScene();
            // Debug.Log($"[BatchRunner] Here a.");
            // Debug.Log($"[BatchRunner] Here c.");
            sceneRandomizer.GenerateScene();
            // Debug.Log($"[BatchRunner] Here b.");

            // Reset for next scene 
            cameraController.ResetCapture();
            captureScript.ResetCapture();

            while (!cameraController.CaptureComplete)
                yield return null;
        }

        Debug.Log("[BatchRunner] All scenes complete!");

        // Stop after all the scenes are done
        if (currentScene >= totalScenes)
        {
            Debug.Log("[BatchRunner] All scenes complete!");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }

    }
}
