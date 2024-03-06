using UnityEngine;

public class HideOnAnyKey : MonoBehaviour {

    void Update() {
        if (Input.anyKeyDown) {
            gameObject.SetActive(false);
        }
    }
}
