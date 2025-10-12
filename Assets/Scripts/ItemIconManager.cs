using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Citadel.Game;
using Zenject;

public class ItemIconManager : MonoBehaviour {

	private Image _image;
	
	[Inject]
	private readonly UsableIconsStorage _usableIconsStorage;

	private void Awake()
	{
		_image = GetComponent<Image>();
	}

	public void SetItemIcon (int index) {
        if (index >= 0) {
            _image.overrideSprite = _usableIconsStorage.GetItemIcon(index);
        }
	}
}
