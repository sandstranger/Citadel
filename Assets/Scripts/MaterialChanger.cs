using System;
using System.Collections;
using System.Collections.Generic;
using Citadel.Game;
using Zenject;
using UnityEngine;

// Used to change gravity lift from red to green.
public class MaterialChanger : MonoBehaviour {
	[HideInInspector] public bool alreadyDone = false;
	public int levelIndex = 0;

	[Inject] private readonly Const _consts;
	[Inject] private readonly ITexturesStorage _texturesStorage;
	
	private MaterialPropertyHelper _materialPropertyHelper;
	private static readonly WaitForSeconds _waitForSeconds = new WaitForSeconds(0.2f);

	private void Awake()
	{
		_materialPropertyHelper = new MaterialPropertyHelper(GetComponent<MeshRenderer>());
	}

	IEnumerator SetMaterialFromCode(int index){
        yield return _waitForSeconds; // give Const a time to populate it's questdata
		switch (index) {
			case 1: UpdateTextures(_consts.questData.lev1SecCode); break;
			case 2: UpdateTextures(_consts.questData.lev2SecCode); break;
			case 3: UpdateTextures(_consts.questData.lev3SecCode); break;
			case 4: UpdateTextures(_consts.questData.lev4SecCode); break;
			case 5: UpdateTextures(_consts.questData.lev5SecCode); break;
			case 6: UpdateTextures(_consts.questData.lev6SecCode); break;
		}
		alreadyDone = true;
	}

	public void Targetted (UseData ud) {
		if (alreadyDone) return; // Gravity lift is already on.

		ImageSequenceTextureArray ista = GetComponent<ImageSequenceTextureArray>();
		alreadyDone = true;
		StartCoroutine(SetMaterialFromCode(levelIndex));
	}

	private void UpdateTextures(int index)
	{
		var texture = _texturesStorage.GetScreenCode(index);
		_materialPropertyHelper.SetMainTexture(texture);
		_materialPropertyHelper.SetEmissionTexture(texture);
	}
	
	public static string Save(GameObject go) {
		MaterialChanger mch = go.GetComponent<MaterialChanger>();
		return Utils.BoolToString(mch.alreadyDone,"alreadyDone"); // and much already yet remaining
	}

	public static int Load(GameObject go, ref string[] entries, int index) {
		MaterialChanger mch = go.GetComponent<MaterialChanger>(); // ... ado about nothing
		mch.alreadyDone = Utils.GetBoolFromString(entries[index],"alreadyDone"); index++;
		if (mch.alreadyDone) {
			ImageSequenceTextureArray ista = mch.GetComponent<ImageSequenceTextureArray>();
			if (ista != null) ista.enabled = false;
			mch.StartCoroutine(mch.SetMaterialFromCode(mch.levelIndex));
		} else {
			ImageSequenceTextureArray ista = mch.GetComponent<ImageSequenceTextureArray>();
			if (ista != null) ista.enabled = true;
			mch.StopCoroutine("SetMaterialFromCode");
		}
		return index;
	}
}
