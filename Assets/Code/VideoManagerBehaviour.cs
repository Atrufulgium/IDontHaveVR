using SFB;
using System;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class VideoManagerBehaviour : MonoBehaviour {
    
    VideoPlayer videoPlayer;
    RenderTexture rt = null;
    public Material SkyboxMaterial;

    public Camera[] eyes;

    string previousFolder;

    private void Awake() {
        previousFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.prepareCompleted += HandlePreparedVideo;
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        videoPlayer.Prepare();
    }

    private void HandlePreparedVideo(VideoPlayer source) {
        var tex = videoPlayer.texture;
        Debug.Log($"Video resolution {tex.width}Å~{tex.height}");
        rt = new(tex.width, tex.height, 0, RenderTextureFormat.Default);
        source.targetTexture = rt;
        SkyboxMaterial.mainTexture = rt;
    }

    static readonly (KeyCode, Action<VideoPlayer>)[] videoActions = new (KeyCode, Action<VideoPlayer>)[] {
        (KeyCode.LeftArrow, v => v.time -= 5),
        (KeyCode.RightArrow, v => v.time += 5),
        (KeyCode.J, v => v.time -= 10),
        (KeyCode.L, v => v.time += 10),
        (KeyCode.Alpha0, v => v.time = 0),
        (KeyCode.Alpha1, v => v.time = 0.1 * v.length),
        (KeyCode.Alpha2, v => v.time = 0.2 * v.length),
        (KeyCode.Alpha3, v => v.time = 0.3 * v.length),
        (KeyCode.Alpha4, v => v.time = 0.4 * v.length),
        (KeyCode.Alpha5, v => v.time = 0.5 * v.length),
        (KeyCode.Alpha6, v => v.time = 0.6 * v.length),
        (KeyCode.Alpha7, v => v.time = 0.7 * v.length),
        (KeyCode.Alpha8, v => v.time = 0.8 * v.length),
        (KeyCode.Alpha9, v => v.time = 0.9 * v.length),
        (KeyCode.K, v => { if (v.isPlaying) v.Pause(); else v.Play(); }),
        (KeyCode.Space, v => { if (v.isPlaying) v.Pause(); else v.Play(); }),
        (KeyCode.F, v => Screen.fullScreen = !Screen.fullScreen),
    };

    private void Update() {
        if (Input.GetKeyDown(KeyCode.O)) {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            if (videoPlayer.isPlaying)
                videoPlayer.Pause();

            var paths = StandaloneFileBrowser.OpenFilePanel("Open video", previousFolder, "*", false);
            if (paths.Length == 1) {
                previousFolder = System.IO.Path.GetDirectoryName(paths[0]);
                videoPlayer.Stop();
                videoPlayer.url = $"file://{paths[0]}";
                videoPlayer.Play();
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

        Math.Clamp(videoPlayer.time, 0, videoPlayer.length);
        Cursor.visible = !videoPlayer.isPlaying;
        Cursor.lockState = videoPlayer.isPlaying ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
