using UnityEngine;
using System.Collections;
using Zenject;

public class TextureStaticNoise : MonoBehaviour {
	public int resolution = 64;
	public float interval = 0.15f;
	private float updateTime;
	private Texture2D texture;
	private bool initialized = false;

	[Inject] private PauseScript _pauseScript;

	void Awake () { Initialize(); }
	void OnEnable () { Initialize(); }

	void Initialize() {
		if (initialized) return;
		initialized = true;
		texture = new Texture2D(resolution, resolution, TextureFormat.RGB24, true);
		texture.name = "ProceduralStatic";
		GetComponent<MeshRenderer>().material.mainTexture = texture;
		FillTexture();
		updateTime = _pauseScript.relativeTime + interval;
	}

	void FillTexture () {
		if (texture.width != resolution) texture.Reinitialize(resolution, resolution);
		for (int y=0; y<resolution; y++) {
			for (int x=0; x<resolution; x++) {
				texture.SetPixel(x, y, Color.white * Random.value);
			}
		}
		texture.filterMode = FilterMode.Point;
		texture.Apply();
	}

	void Update() {
		if (!_pauseScript.Paused() && !_pauseScript.MenuActive()) {
			if (updateTime < _pauseScript.relativeTime) {
				updateTime = (_pauseScript.relativeTime + interval);
				FillTexture();
			}
		}
	}
	
	void OnDestroy() {
		Destroy(texture);
	}
}
