using SFB;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class VideoManagerBehaviour : MonoBehaviour {
    
    VideoPlayer videoPlayer;
    RenderTexture rt = null;
    public Material SkyboxMaterial;
    public Material BlitPartMaterial;
    public Material FishMaterial;

    public Camera leftEye;
    public Camera rightEye;
    Camera[] eyes;
    CameraRenderBehaviour[] renderers;

    string previousFolder;

    private void Awake() {
        previousFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        eyes = new Camera[] { leftEye, rightEye };
        renderers = new CameraRenderBehaviour[eyes.Length];
        for (int i = 0; i < eyes.Length; i++)
            renderers[i] = eyes[i].GetComponent<CameraRenderBehaviour>();

        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.prepareCompleted += HandlePreparedVideo;
        videoPlayer.errorReceived += OpenMediaNonVideo;
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        videoPlayer.Prepare();
    }

    static readonly (KeyCode, Action<VideoPlayer>)[] videoActions = new (KeyCode, Action<VideoPlayer>)[] {
        (KeyCode.LeftArrow, v => Seek(v, v.time - 5)),
        (KeyCode.RightArrow, v => Seek(v, v.time + 5)),
        (KeyCode.J, v => Seek(v, v.time - 10)),
        (KeyCode.L, v => Seek(v, v.time + 10)),
        (KeyCode.Alpha0, v => Seek(v, 0)),
        (KeyCode.Alpha1, v => Seek(v, 0.1 * v.length)),
        (KeyCode.Alpha2, v => Seek(v, 0.2 * v.length)),
        (KeyCode.Alpha3, v => Seek(v, 0.3 * v.length)),
        (KeyCode.Alpha4, v => Seek(v, 0.4 * v.length)),
        (KeyCode.Alpha5, v => Seek(v, 0.5 * v.length)),
        (KeyCode.Alpha6, v => Seek(v, 0.6 * v.length)),
        (KeyCode.Alpha7, v => Seek(v, 0.7 * v.length)),
        (KeyCode.Alpha8, v => Seek(v, 0.8 * v.length)),
        (KeyCode.Alpha9, v => Seek(v, 0.9 * v.length)),
        (KeyCode.K, v => { if (v.isPlaying) v.Pause(); else v.Play(); AnnouncePlaying(v); }),
        (KeyCode.Space, v => { if (v.isPlaying) v.Pause(); else v.Play(); AnnouncePlaying(v); }),
        (KeyCode.F, v => { Screen.fullScreen = !Screen.fullScreen; AnnounceFullscreen(Screen.fullScreen); }),
    };

    

    private void Update() {
        if (Input.GetKeyDown(KeyCode.O)) {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            if (videoPlayer.isPlaying)
                videoPlayer.Pause();

            var paths = StandaloneFileBrowser.OpenFilePanel("Open media", previousFolder, "*", false);
            if (paths.Length == 1) {
                previousFolder = Path.GetDirectoryName(paths[0]);
                OpenMedia($"file://{paths[0]}");
            }
        }

        if (Input.GetKeyDown(KeyCode.S)) {
            foreach (var camera in eyes) {
                var rect = camera.rect;
                if (rect.x == 0) {
                    rect.x = 0.5f;
                    rect.width = 0.5f;
                } else {
                    rect.x = 0f;
                    rect.width = 0.5f;
                }
                camera.rect = rect;
            }
            // Excessively magic and dependent on the editor order
            // I'm not going for any decent architecture, fortunately
            AnnounceCrosseye(eyes[0].rect.x == 0f);
        }

        if (Input.GetKeyDown(KeyCode.R) && renderers.Length != 0) {
            var cr = renderers[0].contentRenderer;
            if (cr is Content180Renderer)
                UpdateRenderers(cr.Source, new ContentFisheye180Renderer());
            else if (cr is ContentFisheye180Renderer)
                UpdateRenderers(cr.Source, new Content360Renderer());
            else
                UpdateRenderers(cr.Source, new Content180Renderer());
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            if (!videoPlayer.isPrepared) {
                Debug.Log("Preparing video.");
                videoPlayer.Prepare();
                return;
            }
        }

        foreach (var (key, action) in videoActions)
            if (Input.GetKeyDown(key))
                action(videoPlayer);

        Cursor.visible = false;// !videoPlayer.isPlaying;
        Cursor.lockState = CursorLockMode.Locked;// videoPlayer.isPlaying ? CursorLockMode.Locked : CursorLockMode.None;
    }

    private void OpenMedia(string path) {
        videoPlayer.Stop();
        // VideoPlayer does not report not being able to open the video with
        // an exception, but by using an event.
        // I get using events for errors along the stream, but the initial
        // decoding? Bleh.
        // I don't want to trust file extensions, and would love to just let
        // Unity handle everything, so `OpenMediaNonVideo` is subscribed to
        // the error event.
        videoPlayer.url = path;
        videoPlayer.Play();
    }
    
    private void HandlePreparedVideo(VideoPlayer source) {
        UpdateRenderers(source.texture, DetectRenderer(source.texture));
        source.isLooping = true;
    }

    // Triggers on error, in particular on "not a video"-errors.
    private void OpenMediaNonVideo(VideoPlayer source, string message) {
        // Make sure this actually has to do with the file format sent.
        if (!message.StartsWith("VideoPlayer cannot play url"))
            return;
        Debug.Log("Attempting to recover with non-video formats...");

        string path = source.url;
        if (path.StartsWith("file://")) {
            path = path[7..];
        }

        // Try other things.
        // Currently tried: images
        try {
            Texture2D tex = new(2, 2);
            if (!ImageConversion.LoadImage(tex, File.ReadAllBytes(path)))
                throw new InvalidOperationException($"Could not load image file at {path}");
            UpdateRenderers(tex, DetectRenderer(tex));
        } catch (Exception e) {
            // (This catches both IO exceptions and the custom one above)
            Debug.LogError($"Something went wrong while loading {path}:\n{e}");
            return;
        }
    }

    IVRContentRenderer DetectRenderer(Texture tex) {
        if (VRContentRendererHelper.Is360(tex))
            return new Content360Renderer { Source = tex };
        else
            return new Content180Renderer { Source = tex };
    }

    static void Seek(VideoPlayer v, double time) {
        time = Math.Clamp(time, 0, v.length);
        v.time = time;
        AnnounceTime(v, time);
    }

    void UpdateRenderers(Texture tex, IVRContentRenderer result) {
        Debug.Log($"Content resolution {tex.width}Å~{tex.height}");
        rt = new(tex.width, tex.height, 0, RenderTextureFormat.Default);
        
        result.Source = tex;

        foreach (var renderer in renderers) {
            renderer.contentRenderer = result;
            SkyboxMaterial.mainTexture = rt;
            renderer.SkyboxRenderTexture = rt;
            renderer.BlitPartMaterial = BlitPartMaterial;
            renderer.FishMaterial = FishMaterial;
        }

        AnnounceRenderer(result);
    }

    static void AnnounceTime(VideoPlayer videoPlayer, double newTime) {
        var length = (int)videoPlayer.length;
        var current = (int)newTime;

        AnnounceManagerBehaviour.Announce(
            $"Seek: {current/60}:{current%60:D2} / {length/60}:{length%60:D2}"
        );
    }

    static void AnnouncePlaying(VideoPlayer videoPlayer) {
        if (videoPlayer.isPlaying)
            AnnounceManagerBehaviour.Announce("Playback: Resumed");
        else
            AnnounceManagerBehaviour.Announce("Playback: Paused");
    }

    static void AnnounceFullscreen(bool fullScreen) {
        if (fullScreen)
            AnnounceManagerBehaviour.Announce("Display: Full screen");
        else
            AnnounceManagerBehaviour.Announce("Display: Windowed");
    }

    static void AnnounceRenderer(IVRContentRenderer renderer) {
        AnnounceManagerBehaviour.Announce($"Rendering: {renderer.Name}");
    }

    static void AnnounceCrosseye(bool isCrossEyed) {
        if (isCrossEyed)
            AnnounceManagerBehaviour.Announce("Stereo: Cross-eyed");
        else
            AnnounceManagerBehaviour.Announce("Stereo: Parallel-eyed");
    }
}
