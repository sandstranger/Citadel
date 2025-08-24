using System;
using System.Collections;
using System.Collections.Generic;
using Citadel.Game;
using Citadel.SceneManagement;
using Zenject;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PoolPopulateByCopy : MonoBehaviour {
	public int numberOfCopies = 3;

	private readonly List<GameObject> _childs = new();

	[Inject] private Const _consts;
	
	private void Awake()
	{
		ScenesLoader.OnStartLoadScene += DisableAllChilds;
		int i = 0;
		while (i < numberOfCopies)
		{
			CreateCopy();
			i++;
		}
	}

	private void OnDestroy()
	{
		ScenesLoader.OnStartLoadScene -= DisableAllChilds;
	}

	private void DisableAllChilds(string sceneName)
	{
		foreach (var child in _childs)
		{
			child.SetActive(false);
		}
	}
	
	private void CreateCopy() {
		GameObject copy = RootInstaller.InstantiatePrefab(transform.GetChild(0).gameObject,transform.position,_consts.quaternionIdentity) as GameObject; // create a copy of a pool object
		if (copy != null) {
			copy.SetActive(false); // Ensure it is in fact, "empty" and available to return.
			var rectTransform = copy.GetComponent<RectTransform>();
			if (rectTransform != null) {
				rectTransform.SetParent(transform,true);
				rectTransform.localScale = _consts.vectorOne;
				rectTransform.localRotation = _consts.quaternionIdentity;
			} else {
				copy.transform.parent = transform;
			}
			_childs.Add(copy);
		}
	}
}
