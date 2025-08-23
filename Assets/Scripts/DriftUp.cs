using Zenject;
using UnityEngine;
using UnityEngine.UI;

public class DriftUp : MonoBehaviour {
    public float startY;
    public float endY;
    public float rate = 0.5f;
    public float fadeRate = 0.1f;
    public bool fadeImage;
    public Image img;
    public float startFade = 1f;
    public float endFade = 0f;

    private float tickFinished;
    
    [Inject] private PauseScript _pauseScript;

    void OnEnable() {
        transform.position = new Vector3(transform.position.x,
                                         startY,
                                         transform.position.z);

        if (fadeImage && img != null) {
            img.color = new Color(img.color.r,img.color.g,img.color.b,startFade);
        }

        tickFinished = _pauseScript.relativeTime;
    }

    void Update() {
        if (_pauseScript.Paused()) return;
		if (_pauseScript.MenuActive()) return;
		if (tickFinished >= _pauseScript.relativeTime) return;

        float delta = (1f / 60f);
		tickFinished = _pauseScript.relativeTime + delta;
		float drift = transform.localPosition.y;
        drift += rate;
        if (drift > endY) drift = endY;
		transform.localPosition = new Vector3(transform.localPosition.x,
                                              drift,
                                              transform.localPosition.z);
        drift = img.color.a;
        drift -= fadeRate;
        if (drift < endFade) drift = endFade;
        img.color = new Color(img.color.r,img.color.g,img.color.b,drift);
    }
}
