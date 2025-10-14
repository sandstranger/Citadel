using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Citadel.Game;
using Zenject;

public class ButtonSwitch : MonoBehaviour {
	// Individually set values per prefab isntance within the scene
	public int securityThreshhold = 100;  // save, If security level is 
	                                      // not <= this level (100), this is
	                                      // unusable until security falls.
	public string target; // save
	public string argvalue; // save
	public int messageIndex = -1; // save
	public float delay = 0f; // save
	public int SFXIndex = 44;
	public int SFXLockedIndex = -1;
	public bool blinkWhenActive; // save
	public bool changeMatOnActive = true; // save
	public bool animateModel = false; // save
	public bool locked = false; // save
	public int lockedMessageLingdex = 193; // save
	public bool active; // save

	[SerializeField]
	private Texture _mainSwitchTexture;
	[SerializeField]
	private Texture _alternateSwitchTexture;
	[SerializeField]
	private Texture _glowDnEmissionTexture;
	[SerializeField]
	private Texture _glowUpEmissionTexture;
	
	// Internal references
	private AudioSource SFXSource;
	[HideInInspector] public Animator anim;
	private GameObject player; // Set on use, no need for initialization check.
	private const float tickTime = 1.5f;
	private bool awakeInitialized = false;

	// Externally modified
	[HideInInspector] public float delayFinished; // save
	[HideInInspector] public float tickFinished; // save
	[HideInInspector] public bool alternateOn; // save
	[HideInInspector] public string currentClipName; // save

	[Inject]
	private readonly LevelManager _levelManager;
	[Inject] private readonly Const _consts;
	[Inject] private readonly MFDManager _mfdManager;
	[Inject] private readonly PauseScript _pauseScript;
	private Color _normalColor;
	private readonly List<MaterialPropertyHelper> _materialPropertyHelpers = new();
	private readonly List<MeshRenderer> _renderers = new();
	
	private static readonly StringBuilder s1 = new StringBuilder(100 * 1024);

	public void Awake() {
		if (awakeInitialized) return;

		SFXSource = GetComponent<AudioSource>();
		if (SFXSource == null) {
		    Debug.Log("BUG: ButtonSwitch missing component for SFXSource");
		} else SFXSource.playOnAwake = false;

		_renderers.AddRange(this.GetComponentsInChildren<MeshRenderer>(includeSelf: true));
		_normalColor = _renderers[0].sharedMaterial.color;
		
		foreach (var renderer in _renderers)
		{
			_materialPropertyHelpers.Add(new MaterialPropertyHelper(renderer));
		}
		
		delayFinished = 0; // prevent using targets on awake
		if (animateModel) {
			anim = GetComponent<Animator>();
			anim.keepAnimatorStateOnDisable = true;
		}
		if (active) {
		    tickFinished = _pauseScript.relativeTime + 1.5f + Random.value;
		}
		
		awakeInitialized = true;
	}

	public void Use (UseData ud) {
	    if (_levelManager.superoverride || _consts.difficultyMission == 0) {
	        locked = false; // SHODAN can go anywhere!  Full security override!
	    } else if (_levelManager.GetCurrentLevelSecurity()
	               > securityThreshhold) {
	                   
			_mfdManager.BlockedBySecurity(transform.position);
			return;
		}

		if (locked) {
			_consts.sprint(lockedMessageLingdex);
			if (SFXLockedIndex >= 0 && SFXLockedIndex < _consts.sounds.Length) {
				Utils.PlayOneShotSavable(SFXSource,_consts.sounds[SFXLockedIndex]);
			}
			
			return;
		}

        // Set playerCamera to owner of the input (always should be the camera)
		player = ud.owner;
		Utils.PlayOneShotSavable(SFXSource,_consts.sounds[SFXIndex]);
		_consts.sprint(messageIndex);
		if (delay > 0f) delayFinished = _pauseScript.relativeTime + delay;
		else UseTargets();
	}

	public void Targetted (UseData ud) {
		Use(ud);
	}

	public void ToggleLocked() {
		string was = locked.ToString();
		locked = !locked;
	}

	public void UseTargets () {
		UseData ud = new UseData();
		ud.owner = player;
		ud.argvalue = argvalue;
		_consts.UseTargets(gameObject,ud,target);
		active = !active;
		alternateOn = active;
		if (changeMatOnActive) {
			if (blinkWhenActive) {
				ToggleMaterial ();
				if (active)
					tickFinished = _pauseScript.relativeTime + tickTime;
			} else {
				ToggleMaterial ();
			}
		}
		if (animateModel) {
			if (active) {
				anim.Play("Activating");
				currentClipName = "Activating";
			} else {
				anim.Play("Deactivating");
				currentClipName = "Deactivating";
			}
		}
	}

	void ToggleMaterial() 
	{
		UpdateButtonTextures(alternateOn);
	}

	public void SetMaterialToAlternate() 
	{
		if (!blinkWhenActive)
		{
			return;
		}

		UpdateButtonTextures(true);
	}

	public void SetMaterialToNormal() 
	{
		if (!blinkWhenActive)
		{
			return;
		}

		UpdateButtonTextures(false);
	}

	void Update() {
		if (_pauseScript.Paused() || _pauseScript.MenuActive()) return;

		if ((delayFinished < _pauseScript.relativeTime)
		    && delayFinished != 0) {

			delayFinished = 0;
			UseTargets();
		}

		// blink the switch when active
		if (blinkWhenActive) {
			if (active) {
				if (tickFinished < _pauseScript.relativeTime) {
					if (_renderers.Any(renderer => renderer.isVisible)) 
					{
						if (alternateOn)
						{
							SetMaterialToAlternate();
						}
						else
						{
							SetMaterialToNormal();
						}
					}
					alternateOn = !alternateOn;
					tickFinished = _pauseScript.relativeTime + tickTime;
				}
			}
		}
	}

	private void UpdateButtonTextures(bool enableButton)
	{
		foreach (var materialHelper in _materialPropertyHelpers)
		{
			materialHelper.SetMainTexture(enableButton ? _alternateSwitchTexture : _mainSwitchTexture);
			materialHelper.SetEmissionColor(enableButton ? Color.white : _normalColor);
			materialHelper.SetEmissionTexture(enableButton ? _glowUpEmissionTexture : _glowDnEmissionTexture);
		}
	}

	public static string Save(GameObject go) {
		ButtonSwitch bs = go.GetComponent<ButtonSwitch>();
		s1.Clear();
		s1.Append(Utils.UintToString(bs.securityThreshhold,"securityThreshhold"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveString(bs.target,"target"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveString(bs.argvalue,"argvalue"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.UintToString(bs.messageIndex,"messageIndex"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(bs.delay,"delay"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(bs.blinkWhenActive,"blinkWhenActive"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(bs.changeMatOnActive,"changeMatOnActive"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(bs.animateModel,"animateModel"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(bs.locked,"locked"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.UintToString(bs.lockedMessageLingdex,"lockedMessageLingdex"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(bs.active,"active"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(bs.alternateOn,"alternateOn"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveRelativeTimeDifferential(bs._pauseScript,bs.delayFinished,"delayFinished"));
	    s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveRelativeTimeDifferential(bs._pauseScript,bs.tickFinished,"tickFinished"));
		s1.Append(Utils.splitChar);
		if (bs.animateModel) {
			if (bs.anim == null) bs.anim = bs.gameObject.GetComponent<Animator>();
			if (bs.gameObject.activeInHierarchy) {
				AnimatorStateInfo asi = bs.anim.GetCurrentAnimatorStateInfo(0);
				s1.Append(Utils.FloatToString(asi.normalizedTime,"asi.normalizedTime"));
			} else {
				if (bs.active && bs.currentClipName == "Activating") s1.Append(Utils.FloatToString(1f,"asi.normalizedTime"));
				else s1.Append(Utils.FloatToString(0f,"asi.normalizedTime"));
			}
		} else {
			s1.Append(Utils.FloatToString(0f,"asi.normalizedTime"));
		}
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveString(bs.currentClipName,"currentClipName"));
		return s1.ToString();
	}

	public static int Load(GameObject go, ref string[] entries, int index) {
		ButtonSwitch bs = go.GetComponent<ButtonSwitch>(); // what a load of bs
		bs.awakeInitialized = false;
		bs.Awake();
		bs.securityThreshhold = Utils.GetIntFromString(entries[index],"securityThreshhold"); index++;
		bs.target = Utils.LoadString(entries[index],"target"); index++;
		bs.argvalue = Utils.LoadString(entries[index],"argvalue"); index++;
		bs.messageIndex = Utils.GetIntFromString(entries[index],"messageIndex"); index++;
		bs.delay = Utils.GetFloatFromString(entries[index],"delay"); index++;
		bs.blinkWhenActive = Utils.GetBoolFromString(entries[index],"blinkWhenActive"); index++;
		bs.changeMatOnActive = Utils.GetBoolFromString(entries[index],"changeMatOnActive"); index++;
		bs.animateModel = Utils.GetBoolFromString(entries[index],"animateModel"); index++;
		bs.locked = Utils.GetBoolFromString(entries[index],"locked"); index++;
		bs.lockedMessageLingdex = Utils.GetIntFromString(entries[index],"lockedMessageLingdex"); index++;
		bs.active = Utils.GetBoolFromString(entries[index],"active"); index++;
		bs.alternateOn = Utils.GetBoolFromString(entries[index],"alternateOn"); index++;
		bs.delayFinished = Utils.LoadRelativeTimeDifferential(bs._pauseScript,entries[index],"delayFinished"); index++;
		bs.delayFinished = 0;
// 		if (bs.delayFinished >= 0.1f) {
// 			bs.delayFinished = Mathf.Max(Mathf.Max(bs.delayFinished,_pauseScript.relativeTime + bs.delay),Time.time + bs.delay);
// 		}
		
		bs.tickFinished = Utils.LoadRelativeTimeDifferential(bs._pauseScript,entries[index],"tickFinished"); index++;
		if ((bs.tickFinished - bs._pauseScript.relativeTime) > tickTime) {
			bs.tickFinished = bs._pauseScript.relativeTime + tickTime;
		}

		float animTime = Utils.GetFloatFromString(entries[index],"asi.normalizedTime"); index++;
		string loadedClipName = Utils.LoadString(entries[index],"currentClipName"); index++;
		if (bs.changeMatOnActive) bs.ToggleMaterial();
		if (bs.animateModel) {
			bs.anim = bs.gameObject.GetComponent<Animator>();
			bs.anim.keepAnimatorStateOnDisable = true;
			Transform originalParent = bs.transform.parent;
			if (originalParent != null) {
				bs.transform.parent = null;
				bool wasActive = bs.gameObject.activeSelf;
				bs.gameObject.SetActive(true);
				bs.anim.Play(loadedClipName,0,animTime);
				bs.gameObject.SetActive(wasActive);
				bs.transform.parent = originalParent;
			} else {
				if (bs.gameObject.activeInHierarchy) bs.anim.Play(loadedClipName,0,animTime);
			}
		}
		return index;
	}
}
