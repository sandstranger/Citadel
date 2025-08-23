using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;
using UnityEngine.UI;

public class EnergyOverloadButton : MonoBehaviour {
    public Color textClickableColor;
    public Color textDisabledColor;
    public Color textOverloadColor;
    public Color textEnergySetting;
    public Color textEnergyOverloaded;
    public Sprite normalButtonSprite;
    public Sprite overloadButtonSprite;
    public Text buttonText;
    public Text energySettingText;
    private Image buttonSprite;
    private float clickFinished;

    [Inject] private Const _consts;
    [Inject] private MFDManager _mfdManager;
    [Inject] private Inventory _inventory;
    [Inject] private WeaponFire _weaponFire;
    [Inject] private WeaponCurrent _weaponCurrent;

    private void Awake() {
        buttonSprite = GetComponent<Image>();
        buttonSprite.overrideSprite = normalButtonSprite;
        buttonText.color = textClickableColor;
    }

    void Start() {
        GetComponent<Button>().onClick.AddListener(() => { OverloadEnergyClick(); });
    }

    public void OverloadEnergyClick() {
		_mfdManager.mouseClickHeldOverGUI = true;
        OverloadButtonAction();
    }

    public void OverloadButtonAction() {
        if (clickFinished >= Time.time) return;

        clickFinished = Time.time + 0.4f;
        if (_inventory.currentEnergyWeaponHeat[_weaponCurrent.weaponCurrent] > 25f) {
            _consts.sprint(_consts.stringTable[12]);
            return;
        }

        if (_weaponFire.overloadEnabled) {
            _consts.sprint(_consts.stringTable[13]);
            _weaponFire.overloadEnabled = false;
            buttonSprite.overrideSprite = normalButtonSprite;
            buttonText.color = textClickableColor;
            energySettingText.color = textEnergySetting;
            energySettingText.text = _consts.stringTable[16];
        } else { 
            _consts.sprint(_consts.stringTable[17]);
            _weaponFire.overloadEnabled = true;
            buttonSprite.overrideSprite = overloadButtonSprite;
            buttonText.color = textOverloadColor;
            energySettingText.color = textEnergyOverloaded;
            energySettingText.text = _consts.stringTable[18];
        }
    }

    public void OverloadFired() {
        buttonSprite.overrideSprite = normalButtonSprite;
        buttonText.color = textDisabledColor;
    }
}
