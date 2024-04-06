using UnityEngine;
using UnityEngine.UI;

public class AnnounceManagerBehaviour : MonoBehaviour {
    public Text[] texts;

    static AnnounceManagerBehaviour singleton;
    static float currentTime;
    static float totalTime;

    private void Awake() {
        singleton = this;
    }

    private void Update() {
        currentTime += Time.deltaTime;
        if (currentTime <= totalTime + 1f) {
            float progress = currentTime / totalTime;
            float alpha = Mathf.Clamp01(2 * (1 - progress));
            SetAlpha(alpha);
        }
    }

    public static void Announce(string message, float duration = 1.5f) {
        currentTime = 0;
        totalTime = duration;
        foreach (var text in singleton.texts) {
            text.text = message;
        }
        SetAlpha(1);
    }

    static void SetAlpha(float value) {
        foreach (var text in singleton.texts) {
            var c = text.color;
            c.a = value;
            text.color = c;
        }
    }
}