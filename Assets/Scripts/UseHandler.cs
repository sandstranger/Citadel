using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class UseHandler : MonoBehaviour
{
    // Check all active and existing scripts on this gameObject that can be used and use them
    // Called by MouseLookScript.cs from a Use() raycast
    private ButtonSwitch _buttonSwitch;
    private ChargeStation _chargeStation;
    private Door _door;
    private HealingBed _healingBed;
    private KeypadElevator _keypadElevator;
    private KeypadKeycode _keypadKeycode;
    private PaperLog _paperLog;
    private PuzzleGridPuzzle _puzzleGridPuzzle;
    private PuzzleWirePuzzle _puzzleWirePuzzle;
    private UseableObjectUse _useableObjectUse;
    private UseableAttachment _useableAttachment;
    private CyberAccess _cyberAccess;
    private InteractablePanel _interactablePanel;

    private void Awake()
    {
        _buttonSwitch = GetComponent<ButtonSwitch>();
        _chargeStation = GetComponent<ChargeStation>();
        _door = GetComponent<Door>();
        _healingBed = GetComponent<HealingBed>();
        _keypadElevator = GetComponent<KeypadElevator>();
        _keypadKeycode = GetComponent<KeypadKeycode>();
        _paperLog = GetComponent<PaperLog>();
        _puzzleGridPuzzle = GetComponent<PuzzleGridPuzzle>();
        _puzzleWirePuzzle = GetComponent<PuzzleWirePuzzle>();
        _useableObjectUse = GetComponent<UseableObjectUse>();
        _useableAttachment = GetComponent<UseableAttachment>();
        _cyberAccess = GetComponent<CyberAccess>();
        _interactablePanel = GetComponent<InteractablePanel>();
    }

    public void Use(UseData ud)
    {
        _buttonSwitch?.Use(ud);
        _chargeStation?.Use(ud);
        _door?.Use(ud);
        _healingBed?.Use(ud);
        _keypadElevator?.Use(ud);
        _keypadKeycode?.Use(ud);
        _paperLog?.Use(ud);
        _puzzleGridPuzzle?.Use(ud);
        _puzzleWirePuzzle?.Use(ud);
        _useableObjectUse?.Use(ud);
        _useableAttachment?.Use(ud);
        _cyberAccess?.Use(ud);
        _interactablePanel?.Use(ud);
    }
}