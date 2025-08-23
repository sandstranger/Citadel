using UnityEngine;
using UnityEngine.UI;
using System;
using Zenject;

public class TextLocalization : MonoBehaviour {
    public int lingdex = 0;
    private TextMesh tM;
    private Text txt;
    private bool initialized;

    [Inject] private Const _consts;
    
    // Register with localization
    public void Awake() {
        if (initialized) return;

        if (tM == null) tM = GetComponent<TextMesh>();
        if (txt == null) txt = GetComponent<Text>();
        _consts.AddToTextLocalizationRegister(this);
        initialized = true;
        UpdateText();
    }

    // Update to match new string table contents.
    public void UpdateText() {
        if (lingdex < 0) return;
        if (_consts == null) return;
        if (_consts.stringTable == null) return;
        if (lingdex >= _consts.stringTable.Length) return;

        if (txt == null && tM != null) {
            tM.text = _consts.stringTable[lingdex];
        } else if (tM == null && txt != null) {
            txt.text = _consts.stringTable[lingdex];
        }
    }
}
