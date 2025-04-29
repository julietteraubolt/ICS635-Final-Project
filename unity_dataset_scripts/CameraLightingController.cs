using UnityEngine;

// This script will control the movement of the camera 
// and the altering of lighting for a scene so that there are
// 15 varying images captured for each scene. This is how 
// we help with data augmnetation 
public class CameraLightingController : MonoBehaviour
{
    [HideInInspector] public bool CaptureComplete = false;

    // Connect this to an object that serves as the target 
    // that will move around 
    public Camera captureCamera;
    public Light sceneLight;
    public Transform orbitTarget;
    public int totalViews = 15;
    public float pitchRangeDegrees = 10f; // Max angle up/down

    private int currentFrame = 0;
    private RgbDepthCapture captureScript;

    void Start()
    {
        // Initializatoins 
        CaptureComplete = false;
        if (captureCamera == null) captureCamera = Camera.main;
        if (sceneLight == null) sceneLight = FindObjectOfType<Light>();
        captureScript = captureCamera.GetComponent<RgbDepthCapture>();
        captureScript.totalFrames = totalViews;
    }

    void LateUpdate()
    {
        // Stop when we get all the captures 
        if (currentFrame >= totalViews)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
            return;
        }

        // We will change the angle of the camera slightly going up and down 
        // and left and right, -5 to 5 degrees
        float t = currentFrame / (float)(totalViews - 1); 
        float pitchAngle = Mathf.Lerp(-pitchRangeDegrees, pitchRangeDegrees, t);
        float rollAngle = Mathf.Sin(t * Mathf.PI * 2f) * 5f; 

        // Make sure viewing the right way 
        Vector3 forward = orbitTarget.position - captureCamera.transform.position;
        forward.y = 0;
        forward = forward.normalized;

        // Pitch: rotate up/down
        Vector3 pitchedDir = Quaternion.AngleAxis(pitchAngle, captureCamera.transform.right) * forward;

        // Set base rotation
        Quaternion baseRot = Quaternion.LookRotation(pitchedDir, Vector3.up);

        // Roll: rotate left/right
        Quaternion rolledRot = Quaternion.AngleAxis(rollAngle, pitchedDir);

        // Put it all together 
        captureCamera.transform.rotation = rolledRot * baseRot;

        // Alter the lighting too 
        sceneLight.transform.rotation = Quaternion.Euler(
            45f + pitchAngle,
            sceneLight.transform.eulerAngles.y + Random.Range(-5f, 5f),
            0f
        );

        currentFrame++;
        if (currentFrame >= totalViews)
            CaptureComplete = true;

    }

    public void ResetCapture()
    {
        currentFrame = 0;
        CaptureComplete = false;
    }


}
