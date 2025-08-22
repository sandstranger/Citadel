using System;
using UnityEngine;
using System.Collections;

public class ExplosionLife : MonoBehaviour {
	public float lightLife = 0.15f;
	public float brightness = 8f;
	public float delayBeforeDestroy = 0.8f;
	public float thinkInterval = 0.05f;
	public bool dontDestroy = false;
	private Light lite;

	private WaitForSeconds _thinkDelayWaiter;
	private WaitForSeconds _delayBeforeDestroyWaiter;
	private Light _light;
	private bool _wasDisabled = false;
	
	private void Awake ()
	{
		_light = GetComponent<Light>();
		_thinkDelayWaiter = new WaitForSeconds(thinkInterval);
		_delayBeforeDestroyWaiter = new WaitForSeconds(delayBeforeDestroy);
	}

	private void OnEnable()
	{
		if (lightLife > 0f)
		{
			StartCoroutine(LifeTime( 0f, brightness, lightLife));
		}

		_wasDisabled = false;
		StartCoroutine(DelayedDestroy());
	}

	private void OnDisable()
	{
		StopAllCoroutines();
		if (!_wasDisabled)
		{
			DestroyExplosionLife();
		}
	}

	private IEnumerator LifeTime (float fadeStart, float fadeEnd, float fadeTime) {
		float t = 0.0f;
		
		while (t < fadeTime) {
			t += Time.deltaTime;

			if (_light != null)
			{
				_light.intensity = Mathf.Lerp(fadeStart, fadeEnd, t / fadeTime);
			}
			yield return _thinkDelayWaiter;
		}

		if (_light != null)
		{
			_light.intensity = 0f;
		}
	}

	private IEnumerator DelayedDestroy ()
	{
		yield return _delayBeforeDestroyWaiter;
		DestroyExplosionLife();
	}

	private void DestroyExplosionLife()
	{
		if (dontDestroy)
		{
			_wasDisabled = true;
			gameObject.SetActive(false);
		} else {
			Utils.SafeDestroy(this.gameObject);
		}
	}
}
