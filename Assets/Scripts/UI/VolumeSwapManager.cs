using UnityEngine;
using UnityEngine.Rendering;

public class VolumeSwapManager : MonoBehaviour
{
    public static VolumeSwapManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCamera;
        RenderPipelineManager.endCameraRendering   += OnEndCamera;
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCamera;
        RenderPipelineManager.endCameraRendering   -= OnEndCamera;
    }

    void OnBeginCamera(ScriptableRenderContext ctx, Camera cam)
    {
        foreach (var p in GameData.players)
        {
            if (p.playerGameObject == null) continue;
            var playerCam = p.playerGameObject.GetComponentInChildren<Camera>();
            if (playerCam != cam) continue;
            p.playerGameObject.GetComponent<PlayerEffectsSystem>()?.ActivateForCamera();
            break;
        }
    }

    void OnEndCamera(ScriptableRenderContext ctx, Camera cam)
    {
        foreach (var p in GameData.players)
        {
            if (p.playerGameObject == null) continue;
            var playerCam = p.playerGameObject.GetComponentInChildren<Camera>();
            if (playerCam != cam) continue;
            p.playerGameObject.GetComponent<PlayerEffectsSystem>()?.DeactivateForCamera();
            break;
        }
    }
}