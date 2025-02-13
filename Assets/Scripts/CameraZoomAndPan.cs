using UnityEngine;
using System.Collections;

public class CameraZoomAndPan : MonoBehaviour
{
    public Camera cam;
    public Transform[] focusPoints; // Assign focus points
    public float[] zoomSizes; // Different zoom levels per focus point
    public float[] transitionSpeeds; // Different transition speeds per focus point

    private int currentFocusIndex = 0;
    private bool isTransitioning = false;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        MoveAndZoomTo(focusPoints[currentFocusIndex], zoomSizes[currentFocusIndex], transitionSpeeds[currentFocusIndex]); // Start at first focus point
    }

    void Update()
    {
        if (!isTransitioning)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                currentFocusIndex = (currentFocusIndex + 1) % focusPoints.Length;
                StartCoroutine(MoveAndZoomTo(focusPoints[currentFocusIndex], zoomSizes[currentFocusIndex], transitionSpeeds[currentFocusIndex]));
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                currentFocusIndex = (currentFocusIndex - 1 + focusPoints.Length) % focusPoints.Length;
                StartCoroutine(MoveAndZoomTo(focusPoints[currentFocusIndex], zoomSizes[currentFocusIndex], transitionSpeeds[currentFocusIndex]));
            }
        }
    }

    IEnumerator MoveAndZoomTo(Transform target, float targetZoom, float transitionTime)
    {
        isTransitioning = true; // Prevent multiple transitions at once

        Vector3 startPosition = cam.transform.position;
        float startSize = cam.orthographicSize;
        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, cam.transform.position.z);

        float elapsedTime = 0f;
        while (elapsedTime < transitionTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionTime;
            cam.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            cam.orthographicSize = Mathf.Lerp(startSize, targetZoom, t);
            yield return null;
        }

        cam.transform.position = targetPosition;
        cam.orthographicSize = targetZoom;

        isTransitioning = false; // Allow new transitions
    }
}
