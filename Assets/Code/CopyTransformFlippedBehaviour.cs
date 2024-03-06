using UnityEngine;

public class CopyTransformFlippedBehaviour : MonoBehaviour {

    public Transform copyForm;

    void Update() {
        copyForm.GetPositionAndRotation(out var pos, out var rot);
        var rotVec = rot.eulerAngles;
        rotVec.y = 180 + rotVec.y;
        rot = Quaternion.Euler(rotVec);
        transform.SetLocalPositionAndRotation(pos, rot);
    }
}
