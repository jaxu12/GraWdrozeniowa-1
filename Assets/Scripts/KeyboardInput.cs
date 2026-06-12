using UnityEngine;
using UnityEngine.InputSystem;

public class KeyboardInput : MonoBehaviour
{
    public PlayerMovement playerMovement;

    private void Update()
    {
        Vector2 input = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed) input.x = -1;
            if (Keyboard.current.dKey.isPressed) input.x = 1;
            if (Keyboard.current.sKey.isPressed) input.y = -1;
            if (Keyboard.current.wKey.isPressed) input.y = 1;
        }

        playerMovement.SetMoveInput(input);
    }
}