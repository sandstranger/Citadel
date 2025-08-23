using Zenject;
using UnityEngine;

public class MinigameCursor : MonoBehaviour {
    public bool mouseOverPanel;
    public float minigameMouseX;
    public float minigameMouseY;
    public RectTransform minigameCursor;
    private float panelWidth = 256f;
    private float offset = 128f;
    private float xmin = 22f/1366f;
    private float xmax = 282f/1366f;
    private float ymin = 6f/768f;
    private float ymax = 266f/768f;
    private float deltaX;
    private float deltaY;
    private bool overX;
    private bool overY;

    [Inject] private MouseCursor _mouseCursor;

    void Start() {
        deltaX = xmax - xmin;
        deltaY = ymax - ymin;
    }

    void Update() {
        minigameMouseX = _mouseCursor.cursorPosition.x / Screen.width;
        overX = false;
        if (minigameMouseX < xmin) minigameMouseX = xmin;
        else if (minigameMouseX > xmax) minigameMouseX = xmax;
        else overX = true;

        minigameMouseX = (minigameMouseX - xmin) / deltaX;

        minigameMouseY = _mouseCursor.cursorPosition.y / Screen.height;
        overY = false;
        if (minigameMouseY < ymin) minigameMouseY = ymin;
        else if (minigameMouseY > ymax) minigameMouseY = ymax;
        else overY = true;

        minigameMouseY = (minigameMouseY - ymin) / deltaY;

        minigameMouseX = (minigameMouseX * panelWidth) - offset;
        minigameMouseY = (minigameMouseY * panelWidth) - offset;
        minigameCursor.localPosition = new Vector3(minigameMouseX,minigameMouseY,0f);
        mouseOverPanel = (overX && overY);
    }
}
