using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Phase2Camera : MonoBehaviour
{
    [Header("Settings")]
    public float padding = 3f;
    public float minSize = 5f;
    public float maxSize = 20f;
    public float smoothSpeed = 5f;

    private Camera cam;
    private List<Transform> targets = new List<Transform>();

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        // Wait one frame for PlayerSpawner to finish spawning
        StartCoroutine(FindPlayers());
    }

    private IEnumerator FindPlayers()
    {
        yield return null; // wait one frame

        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        Debug.Log($"Phase2Camera found {players.Length} players");

        foreach (var p in players)
        {
            targets.Add(p.transform);

            // Disable player child camera since Phase2Camera takes over
            var playerCam = p.GetComponentInChildren<Camera>();
            if (playerCam != null)
                playerCam.gameObject.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (targets.Count == 0) return;
        MoveCamera();
        ZoomCamera();
    }

    private void MoveCamera()
    {
        Vector3 centerPoint = GetCenterPoint();
        centerPoint.z = transform.position.z;
        transform.position = Vector3.Lerp(transform.position, centerPoint, smoothSpeed * Time.deltaTime);
    }

    private void ZoomCamera()
    {
        Bounds bounds = GetBounds();
        float requiredHeight = bounds.size.y * 0.5f + padding;
        float requiredWidth = bounds.size.x * 0.5f / cam.aspect + padding;
        float requiredSize = Mathf.Max(requiredHeight, requiredWidth);
        requiredSize = Mathf.Clamp(requiredSize, minSize, maxSize);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, requiredSize, smoothSpeed * Time.deltaTime);
    }

    private Vector3 GetCenterPoint()
    {
        if (targets.Count == 1)
            return targets[0].position;
        return GetBounds().center;
    }

    private Bounds GetBounds()
    {
        var bounds = new Bounds(targets[0].position, Vector3.zero);
        foreach (var t in targets)
            bounds.Encapsulate(t.position);
        return bounds;
    }
}