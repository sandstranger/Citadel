using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;
using UnityEngine.UI;

public class ReverbZone : MonoBehaviour {
    [Inject] private Const _consts;
    void Start() {
        _consts.AddToReverbRegister(gameObject);
    }
}