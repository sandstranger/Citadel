using System;
using System.Collections;
using System.Collections.Generic;
using Citadel.Game;
using Zenject;
using UnityEngine;

public class MaterialFlash : MonoBehaviour {
	public bool startFlashing = false;
	public bool startNormal = true;
	public bool stopReturnsToNormal = true;
	public float timeBetweenFlashes = 0.35f;
	public Light lit;
	private bool isFlashing = false;
	private float flashFinished; // Visual only, using Time.time
	private bool changeDone = false;
	private bool normal = true;

	[SerializeField]
	private TexturesInfo _normalTextures;
	[SerializeField]
	private TexturesInfo _alternateTextures;
	
	[Inject] private Const _consts;
	[Inject] private PauseScript _pauseScript;
	private readonly List<MaterialPropertyHelper> _materialPropertyHelpers = new();
	
	void Start () {
		foreach (var renderer in this.GetComponentsInChildren<MeshRenderer>(includeSelf: true))
		{
			_materialPropertyHelpers.Add(new MaterialPropertyHelper(renderer));
		}

		if (startFlashing) isFlashing = true;
		flashFinished = Time.time;
		changeDone = false;
		normal = true;
		if (!startNormal) {
			if (lit != null) lit.enabled = true;
			UpdateTextures(true);
			normal = false;
		}
	}

    void Update() {
		if (!_pauseScript.Paused() && !_pauseScript.MenuActive()) {
			if (_consts.questData.SelfDestructActivated) isFlashing = true;

			if (isFlashing) {
				if (flashFinished < Time.time) {
					flashFinished = Time.time + timeBetweenFlashes;
					if (normal) {
						if (lit != null) lit.enabled = true;
						UpdateTextures(true);
						normal = !normal;
					} else {
						if (lit != null) lit.enabled = false;
						UpdateTextures(false);
						normal = !normal;
					}
				}
			} else {
				if (stopReturnsToNormal && !changeDone) {
					if (lit != null) lit.enabled = false;
					changeDone = true;
					UpdateTextures(false);
				}
			}
		}
    }

	public void StartFlashing() {
		isFlashing = true;
	}

	public void StopFlashing() {
		isFlashing = false;
	}

	private void UpdateTextures(bool useAlternateTextures)
	{
		var texturesInfo = useAlternateTextures ? _alternateTextures : _normalTextures;
		foreach (var materialPropertyHelper in _materialPropertyHelpers)
		{
			materialPropertyHelper.SetMainTexture(texturesInfo.MainTexture);
			materialPropertyHelper.SetEmissionTexture(texturesInfo.EmissionTexture);
		}
	}

	[Serializable]
	private struct TexturesInfo
	{
		public Texture MainTexture;
		public Texture EmissionTexture;
	}
}
