using System.Collections;
using System.Collections.Generic;
using Citadel.Game;
using UnityEngine;

public class TextureChanger : MonoBehaviour {
	public Texture2D mainTexture;
	public Texture2D mainTexture2;
	public Texture2D mainTextureGlow;
	public Texture2D mainTextureGlow2;
	public bool startAlternate = false;
	[HideInInspector] public bool currentTexture = false;
	public Renderer rMainLod0;
	/*[DTValidator.Optional] */public Renderer rMainLod1;
	/*[DTValidator.Optional] */public Renderer rMainLod2;
	public bool useGlow;

	private List<MaterialPropertyHelper> _materialPropertyHelpers;
	
	public void Awake()
	{
		_materialPropertyHelpers = BuildMaterialPropertyHelpers();
		
		if (startAlternate) 
		{
			currentTexture = true;
			foreach (var materialProperty in _materialPropertyHelpers)
			{
				materialProperty.SetMainTexture(mainTexture2);
				if (useGlow)
				{
					materialProperty.SetEmissionTexture(mainTextureGlow2);
				}
			}
		}
	}

    public void Toggle() {
	    
	    foreach (var materialProperty in _materialPropertyHelpers)
	    {
		    materialProperty.SetMainTexture(currentTexture ? mainTexture : mainTexture2);
		    
		    if (useGlow)
		    {
			    materialProperty.SetEmissionTexture(currentTexture ? mainTextureGlow : mainTextureGlow2);
		    }
	    }
	    
		currentTexture = !currentTexture;
	}

	private List<MaterialPropertyHelper> BuildMaterialPropertyHelpers()
	{
		var result = new List<MaterialPropertyHelper>();
		
		AddValue(rMainLod0);
		AddValue(rMainLod1);
		AddValue(rMainLod2);

		return result;
		
		void AddValue(Renderer renderer)
		{
			if (renderer != null)
			{
				result.Add(new MaterialPropertyHelper(rMainLod0));
			}
		}
	}
    
	public static string Save(GameObject go) {
		TextureChanger tex = go.GetComponent<TextureChanger>();
		return Utils.BoolToString(tex.currentTexture,"currentTexture");
	}

	public static int Load(GameObject go, ref string[] entries, int index) {
		TextureChanger tex = go.GetComponent<TextureChanger>();
		tex.currentTexture = Utils.GetBoolFromString(entries[index],"currentTexture"); index++;
		tex.currentTexture = !tex.currentTexture; // gets done again in Toggle()
		tex.Toggle(); // set it again since this does other stuff than just change the bool
		return index;
	}
}
