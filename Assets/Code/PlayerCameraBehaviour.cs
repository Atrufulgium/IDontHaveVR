using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// A temporary behaviour for moving the camera around.
/// </summary>
public class PlayerCameraBehaviour : MonoBehaviour {

    [Range(0f, 10f)]
    public float horizontalSensitivity = 1;
    [Range(0f, 10f)]
    public float verticalSensitivity = 1;
    [Range(0f, 10f)]
    public float scrollSensitivity = 3;
    public bool flipHorizontal = false;
    public bool flipVertical = false;
    public bool flipScroll = false;

    public Camera[] cameras;

    public VideoPlayer pauseQuerier;

    public Vector2 verticalClampDegrees = new(-70f, 70f);

    public void Update() {
        HandleMouseMovement();
    }

    void HandleMouseMovement() {
        float h = Input.GetAxis("Mouse X") * horizontalSensitivity;
        float v = Input.GetAxis("Mouse Y") * verticalSensitivity;
        if (flipHorizontal)
            h *= -1;
        if (flipVertical)
            v *= -1;
        Vector3 eulerAngles = transform.rotation.eulerAngles;
        // Editor shows [-180,180], method returns [0,360], oof.
        if (eulerAngles.x + v > 180f)
            v -= 360f;
        eulerAngles.x = Mathf.Clamp(eulerAngles.x + v, verticalClampDegrees.x, verticalClampDegrees.y);
        eulerAngles.y += h;
        transform.rotation = Quaternion.Euler(eulerAngles);

        if (Input.mouseScrollDelta.y != 0) {
            float fovChange = Mathf.Sign(Input.mouseScrollDelta.y);
            if (flipScroll)
                fovChange *= -1;

            foreach (var camera in cameras) {
                camera.fieldOfView += scrollSensitivity * fovChange;
            }
        }
    }
}