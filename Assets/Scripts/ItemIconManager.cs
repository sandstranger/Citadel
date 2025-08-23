using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Zenject;

public class ItemIconManager : MonoBehaviour {
	[Inject] private Const _consts;
	
	public void SetItemIcon (int index) {
        if (index >= 0) {
            GetComponent<Image>().overrideSprite = _consts.useableItemsIcons[index];
        }
	}
}
