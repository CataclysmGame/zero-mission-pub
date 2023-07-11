using UnityEngine;
using UnityEngine.InputSystem;

public class MouseHider : MonoBehaviour
{
    public float hideAfterSeconds = 5.0f;
    public float movementThreshold = 5.0f;

    private float movementTimer = 0.0f;
    private Vector2 lastMousePos;

    void Start()
    {
        var mouse = Mouse.current;
        lastMousePos = mouse.position.ReadValue();
    }

    void Update()
    {
        var curPos = Mouse.current.position.ReadValue();
        var delta = curPos - lastMousePos;
        lastMousePos = curPos;

        if (delta.sqrMagnitude > movementThreshold * movementThreshold)
        {
            movementTimer = 0.0f;
        }
        else
        {
            movementTimer += Time.deltaTime;
        }

        Cursor.visible = movementTimer < hideAfterSeconds;
    }
}
