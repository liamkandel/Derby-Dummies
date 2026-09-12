using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Throwaway free-fly camera for inspecting the track in Play mode.
///   WASD / arrows : move        Q / E : down / up
///   hold Left Shift : go faster hold Right Mouse : look around
/// Delete this once a real kart + kart camera exist.
/// </summary>
[AddComponentMenu("DerbyDummies/Dev/Fly Camera")]
public class FlyCamera : MonoBehaviour
{
    [SerializeField] float moveSpeed = 25f;        // ~kart cruising speed (m/s)
    [SerializeField] float boostMultiplier = 4f;
    [SerializeField] float lookSensitivity = 0.1f;

    float yaw;
    float pitch;

    void OnEnable()
    {
        Vector3 e = transform.eulerAngles;
        yaw = e.y;
        pitch = e.x;
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        Mouse mouse = Mouse.current;
        if (kb == null) return;

        // Look only while holding the right mouse button.
        if (mouse != null && mouse.rightButton.isPressed)
        {
            Vector2 d = mouse.delta.ReadValue() * lookSensitivity;
            yaw += d.x;
            pitch = Mathf.Clamp(pitch - d.y, -89f, 89f);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        Vector3 move = Vector3.zero;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    move += Vector3.forward;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  move += Vector3.back;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  move += Vector3.left;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) move += Vector3.right;
        if (kb.eKey.isPressed) move += Vector3.up;
        if (kb.qKey.isPressed) move += Vector3.down;

        float speed = moveSpeed * (kb.leftShiftKey.isPressed ? boostMultiplier : 1f);
        transform.Translate(move.normalized * speed * Time.deltaTime, Space.Self);
    }
}
