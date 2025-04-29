using UnityEngine;
using System.Collections.Generic;

// This script generates random scenes 
public class SceneRandomizer : MonoBehaviour
{
    // Later overwrote these to be 50 and 75 
    public int minObjects = 10;
    public int maxObjects = 40;
    public Vector3 spawnAreaSize = new Vector3(6f, 3f, 6f);

    // Make lists for the prefabs, materials, and backgrounds that can be used
    public List<GameObject> prefabPool;         
    public List<Material> materialPool;         
   
    private List<GameObject> spawnedObjects = new List<GameObject>();

    public List<Material> skyboxOptions;
    public List<Color> solidBackgroundColors;       
    [Range(0f, 1f)]
    public float skyboxProbability = 0.5f;     

    void Start()
    {

    }

    public void ClearScene()
    {
        // Make sure to always start with a clear scene 
        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        spawnedObjects.Clear();
        Debug.Log($"[SceneRandomizer] Cleared the scene.");
    }

    public void GenerateScene()
    {
        // Decide how many random objects to spawn
        int numObjects = Random.Range(minObjects, maxObjects + 1);

        // For each object choose a random object and put it at
        // a random position with a random size and rotation 
        for (int i = 0; i < numObjects; i++)
        {
            GameObject obj = SpawnRandomObject();
            obj.transform.position = GetRandomPosition();
            obj.transform.localScale = Vector3.one * Random.Range(0.3f, .9f);
            obj.transform.rotation = Random.rotation;

            // Finally figured out how to make sure that nothing was moving smh 
            foreach (var rb in obj.GetComponentsInChildren<Rigidbody>())
            {
                rb.useGravity = false;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }

            foreach (var col in obj.GetComponentsInChildren<Collider>())
            {
                col.enabled = false; 
            }

            foreach (var animator in obj.GetComponentsInChildren<Animator>())
            {
                animator.enabled = false;
            }

            foreach (var script in obj.GetComponentsInChildren<MonoBehaviour>())
            {
                if (script.GetType().Name.ToLower().Contains("move") ||
                    script.GetType().Name.ToLower().Contains("rotate") ||
                    script.GetType().Name.ToLower().Contains("physics") ||
                    script.GetType().Name.ToLower().Contains("controller"))
                {
                    Destroy(script);
                    Debug.Log($"Removed {script.GetType().Name} from {obj.name}");
                }
            }

            // Give each object a random material and add to the scene 
            ApplyRandomMaterial(obj);
            spawnedObjects.Add(obj);
        }
        // Set the background (either skybox or solif color 50/50 chance)
        if (Random.value < skyboxProbability)
        {
            RenderSettings.skybox = skyboxOptions[Random.Range(0, skyboxOptions.Count)];
            Camera.main.clearFlags = CameraClearFlags.Skybox;
        }
        else
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = solidBackgroundColors[Random.Range(0, solidBackgroundColors.Count)];
            RenderSettings.skybox = null;
        }

        Debug.Log($"[SceneRandomizer] Spawned {numObjects} random objects.");
    }

    GameObject SpawnRandomObject()
    {
        // Choose a random prefab 
        int index = Random.Range(0, prefabPool.Count);
        return Instantiate(prefabPool[index]);
    }

    Vector3 GetRandomPosition()
    {
        // Get a random position in the designated area 
        return new Vector3(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            Random.Range(0.1f, spawnAreaSize.y),
            Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
        );
    }

    void ApplyRandomMaterial(GameObject obj)
    {
        // Choose a random material to apply to the object 
        if (materialPool.Count == 0) return;

        Material mat = materialPool[Random.Range(0, materialPool.Count)];
        var renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = mat;
        }
        else
        {
            foreach (var childRenderer in obj.GetComponentsInChildren<Renderer>())
            {
                childRenderer.material = mat;
            }
        }
    }
}
