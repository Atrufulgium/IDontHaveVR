using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraRenderBehaviour : MonoBehaviour {

    public IVRContentRenderer contentRenderer;
    public VREye vrEye;

    [HideInInspector]
    public RenderTexture SkyboxRenderTexture;
    [HideInInspector]
    public Material BlitPartMaterial;
    [HideInInspector]
    public Material FishMaterial;

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (contentRenderer != null && BlitPartMaterial != null && SkyboxRenderTexture != null)
            contentRenderer.OnRenderImage(SkyboxRenderTexture, BlitPartMaterial, FishMaterial, vrEye);
        Graphics.Blit(source, destination);
    }
}