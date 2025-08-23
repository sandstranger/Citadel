using Zenject;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FireWorkThang : MonoBehaviour {
    public float minScale;
    public float maxScale;
    public float changeFracPerSecond = 0.5f;
    public float waitTimeFull = 0.8f;
    public float waitTimeMinMin = 0.8f;
    public float waitTimeMinMax = 3f;
    private Image img;
    private float tickFinished;
    private float curScale;
    private bool waitAtFull;
    private bool waitAtMin;

    [Inject] private PauseScript _pauseScript;

    void Awake() {
        img = GetComponent<Image>();
    }

    void OnEnable() {
        tickFinished = _pauseScript.relativeTime;
        curScale = Random.Range(minScale,maxScale);
        if (changeFracPerSecond < 0.001f) changeFracPerSecond = 0.5f; // 2 secs
        waitAtFull = false;
        if (minScale >= maxScale) {
            minScale = 0f;
            maxScale = 1f;
            Debug.LogWarning("FireWorkThang maxScale not set higher than min");
        }
    }

    void Update() {
        if (_pauseScript.Paused()) return;
        if (_pauseScript.MenuActive()) return;
        if (tickFinished >= _pauseScript.relativeTime) return;

        float delta = (1f / 60f);
        tickFinished = _pauseScript.relativeTime + delta;
        if (waitAtFull) {
            waitAtFull = false;
            waitAtMin = false;
            curScale = minScale;
            waitAtMin = true;
            tickFinished = _pauseScript.relativeTime
                           + Random.Range(waitTimeMinMin,waitTimeMinMax);
        } else if (waitAtMin) {
            waitAtMin = false;
            waitAtFull = false;
            curScale += (maxScale - minScale) * 0.333f;
        } else {
            curScale += (changeFracPerSecond * (maxScale - minScale)) / 60f;
        }

        if (curScale > maxScale) {
            curScale = maxScale;
            waitAtFull = true;
            tickFinished = _pauseScript.relativeTime + waitTimeFull;
        }

        transform.localScale = new Vector3(curScale,curScale,curScale);
    }
}
