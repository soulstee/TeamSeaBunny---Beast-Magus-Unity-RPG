using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Assign "Overworld Player" here in the Inspector
    public float smoothSpeed = 0.125f; // Adjust for smoother movement
    public Vector3 offset; // Offset for the camera's position
    private Camera mainCamera;

    public float overworldZoom = 5f; // Adjust zoom level for overworld scenes
    public float defaultZoom = 10f; // Zoom level for non-overworld scenes

    void Start()
    {
        FindMainCamera();
        AdjustCameraZoom(SceneManager.GetActiveScene().name);
    }

    void LateUpdate()
    {
        if (IsOverworldScene(SceneManager.GetActiveScene().name) && target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindMainCamera();
        AdjustCameraZoom(scene.name);

        if (IsOverworldScene(scene.name))
        {
            GameObject player = GameObject.FindWithTag("OverworldPlayer");
            if (player != null)
            {
                target = player.transform;
            }
        }
        else
        {
            target = null; // Clear the target if not in an overworld scene
        }
    }

    private void AdjustCameraZoom(string sceneName)
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("Main Camera not found. Unable to adjust zoom.");
            return;
        }

        if (IsOverworldScene(sceneName))
        {
            mainCamera.orthographicSize = overworldZoom;
        }
        else
        {
            mainCamera.orthographicSize = defaultZoom;
        }
    }

    private void FindMainCamera()
    {
        if (mainCamera == null)
        {
            GameObject cameraObject = GameObject.FindWithTag("MainCamera");
            if (cameraObject != null)
            {
                mainCamera = cameraObject.GetComponent<Camera>();
                Debug.Log("Main Camera found and assigned.");
            }
            else
            {
                Debug.LogError("Main Camera not found! Ensure there is a camera tagged as 'MainCamera'.");
            }
        }
    }

    private bool IsOverworldScene(string sceneName)
    {
        return sceneName.Contains("Overworld");
    }
}
