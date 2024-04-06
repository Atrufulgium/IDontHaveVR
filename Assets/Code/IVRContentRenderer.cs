using System;
using UnityEngine;

public interface IVRContentRenderer {
    /// <summary>
    /// This method implements the actual rendering of an eye.
    /// </summary>
    /// <param name="skyboxTexture">
    /// We render the content onto the skybox so that Unity's shaders do the
    /// decoding for us. Simply write onto here.
    /// </param>
    /// <param name="blitPartMaterial">
    /// A helper material to Blit with. Important parameters to set with
    /// <see cref="Shader.SetGlobalVector(int, Vector4)"/> etc:
    /// <list type="bullet">
    /// <item>
    /// <b>_UVSrcMinMax</b> (float4): What (umin,vmin,umax,vmax) to read from
    /// the texture send with the Blit command.
    /// </item>
    /// <item>
    /// <b>_UVDstMinMax</b> (float4): What (umin,vmin,umax,vmax) to write the
    /// texture fragment as specified by _UVSrcMinMax onto. Everything outside
    /// this range is filled with:
    /// </item>
    /// <item>
    /// <b>_FillColor</b> (color): The color to set everything outside
    /// _UVDstMinMax to.
    /// </item>
    /// </list>
    /// </param>
    /// <param name="fishmaterial">
    /// A helper material to convert fish-eyed 180° images to panoramic 180°
    /// images. It only has one parameter <b>_AspectInv</b> (float), denoting
    /// the <c>height/width</c> of <i>a single eye</i>.
    /// </param>
    /// <param name="eye"> What eye we're rendering. </param>
    // lol the materials should really be a dictionary or something
    public void OnRenderImage(RenderTexture skyboxTexture, Material blitPartMaterial, Material fishmaterial, VREye eye);

    /// <summary>
    /// The texture of this renderer.
    /// </summary>
    public Texture Source { get; set; }

    /// <summary>
    /// The name of this renderer;
    /// </summary>
    public string Name { get; }
}

public static class VRContentRendererHelper { 
    public static void ResetTexture(Texture t, Color col) {
        if (t is RenderTexture renderTexture) {
            var oldActive = RenderTexture.active;
            RenderTexture.active = renderTexture;
            GL.Clear(true, true, col);
            RenderTexture.active = oldActive;
        } else {
            throw new NotSupportedException($"Do not support clearing texture {t} yet.");
        }
    }

    public static int uvSrcID = Shader.PropertyToID("_UVSrcMinMax");
    public static int uvDstID = Shader.PropertyToID("_UVDstMinMax");
    public static int fillColorID = Shader.PropertyToID("_FillColor");
    public static int aspectinvID = Shader.PropertyToID("_AspectInv");

    /// <summary>
    /// If false, the supplied texture is a 180° VR texture.
    /// If true, it is 360°.
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static bool Is360(Texture t) {
        // Common 180° VR content is side-by-side, while commonn 360° is top-bottom.
        // Assuming videos are at least an aspect ratio of 1:1 wide, just check
        // whether it's >= 2:1 for 180°, and otherwise it's 360°.
        return t.width < 2 * t.height;
    }
}

/// <summary>
/// Renders content in 180° layout: two images side-to-side.
/// These are identified by width being at least 2x the height.
/// </summary>
public class Content180Renderer : IVRContentRenderer {
    public Texture Source { get; set; }
    public string Name => "Side-to-side 180° VR";

    public virtual void OnRenderImage(RenderTexture skyboxTexture, Material blitPartMaterial, Material fishmaterial, VREye eye) {
        // Left eye: Blit left half to skybox' left half, rest black
        // Right eye: Blit right half to skybox' left half, rest black
        // NOTE: Copied to ContentFisheye180Renderer.
        Vector4 rect;
        if (eye == VREye.Left) {
            rect = new(0f, 0f, 0.5f, 1f);
        } else {
            rect = new(0.5f, 0f, 1f, 1f);
        }
        blitPartMaterial.SetVector(VRContentRendererHelper.uvSrcID, rect);
        blitPartMaterial.SetVector(VRContentRendererHelper.uvDstID, new(0f, 0f, 0.5f, 1f));
        blitPartMaterial.SetColor(VRContentRendererHelper.fillColorID, Color.black);
        Graphics.Blit(Source, skyboxTexture, blitPartMaterial);
    }
}

/// <summary>
/// Renders content in 360° layout: two images atop eachother.
/// These are identified by not being 180°.
/// </summary>
public class Content360Renderer : IVRContentRenderer {
    public Texture Source { get; set; }
    public string Name => "Top-bottom 360° VR";

    public void OnRenderImage(RenderTexture skyboxTexture, Material blitPartMaterial, Material fishmaterial, VREye eye) {
        // Left eye: Blit top half to full skybox
        // Right eye: Blit bottom half to full skybox
        Vector4 rect;
        if (eye == VREye.Left) {
            rect = new(0f, 0.5f, 1f, 1f);
        } else {
            rect = new(0f, 0f, 1f, 0.5f);
        }
        blitPartMaterial.SetVector(VRContentRendererHelper.uvSrcID, rect);
        blitPartMaterial.SetVector(VRContentRendererHelper.uvDstID, new(0, 0, 1, 1));
        Graphics.Blit(Source, skyboxTexture, blitPartMaterial);
    }
}

/// <summary>
/// Renders two side-to-side fish-eye-projected images.
/// Not identified, pick this option yourself.
/// </summary>
// This would be so hilariously more efficient if I just made a half-sphere in
// Blender (taking 30 seconds), giving it the correct UVs (taking 30 seconds),
// and putting *that* in the scene.
public class ContentFisheye180Renderer : IVRContentRenderer {
    public Texture Source { get; set; }
    public string Name => "Fisheye 180° VR";

    public void OnRenderImage(RenderTexture skyboxTexture, Material blitPartMaterial, Material fishmaterial, VREye eye) {
        // First convert the fisheye-180 to panoramic-180
        var rt = RenderTexture.GetTemporary(Source.width, Source.height);
        fishmaterial.SetFloat(VRContentRendererHelper.aspectinvID, 2f * Source.height / Source.width);
        Graphics.Blit(Source, rt, fishmaterial);

        // Annoying mostly-copypasta from Content180Renderer to prevent a
        // useless blit.
        // ~~ignore the request of a temp rt every frame~~
        // (Otoh, unity docs say that request every frame is better than
        // reusing.. ok. Saves me writing an awkward finalizer.)
        Vector4 rect;
        if (eye == VREye.Left) {
            rect = new(0f, 0f, 0.5f, 1f);
        } else {
            rect = new(0.5f, 0f, 1f, 1f);
        }
        blitPartMaterial.SetVector(VRContentRendererHelper.uvSrcID, rect);
        blitPartMaterial.SetVector(VRContentRendererHelper.uvDstID, new(0f, 0f, 0.5f, 1f));
        blitPartMaterial.SetColor(VRContentRendererHelper.fillColorID, Color.black);
        Graphics.Blit(rt, skyboxTexture, blitPartMaterial);
        RenderTexture.ReleaseTemporary(rt);
    }
}