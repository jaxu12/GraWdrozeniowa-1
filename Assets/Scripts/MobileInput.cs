using UnityEngine;

public class MobileInput : MonoBehaviour
{
    public FloatingJoystick joystick;
    public PlayerMovement playerMovement;

    private void Update()
    {
        if (joystick == null || playerMovement == null)
            return;

        Vector2 input = new Vector2(joystick.Horizontal, joystick.Vertical);
        Debug.Log("Joystick input: " + input);

        playerMovement.SetMoveInput(input);
    }
}