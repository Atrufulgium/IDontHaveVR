using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraRenderBehaviour : MonoBehaviour {

    public IVRContentRenderer contentRenderer;
    public VREye vrEye;

    [HideInInspector]
    public RenderTexture SkyboxRenderTexture;
    [HideInInspector]
    public Material BlitPartMaterial;

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (contentRenderer != null && BlitPartMaterial != null && SkyboxRenderTexture != null)
            contentRenderer.OnRenderImage(SkyboxRenderTexture, BlitPartMaterial, vrEye);
        Graphics.Blit(source, destination);
    }
}