using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 1, -10);
    public bool canLookAround;
    public float lookDistance = 2f;
    public float lookSmoothSpeed = 6f;

    private Vector3 lookOffset;

    void LateUpdate()
    {
        if (target == null) return;

        UpdateLookOffset();

        Vector3 targetPosition = target.position + offset + lookOffset;
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );
    }

    public void UnlockLookAround()
    {
        canLookAround = true;
    }

    void UpdateLookOffset()
    {
        Vector3 targetLookOffset = Vector3.zero;

        if (canLookAround && Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed)
                targetLookOffset.x -= lookDistance;

            if (Keyboard.current.dKey.isPressed)
                targetLookOffset.x += lookDistance;

            if (Keyboard.current.wKey.isPressed)
                targetLookOffset.y += lookDistance;

            if (Keyboard.current.sKey.isPressed)
                targetLookOffset.y -= lookDistance;
        }

        lookOffset = Vector3.Lerp(
            lookOffset,
            targetLookOffset,
            lookSmoothSpeed * Time.deltaTime
        );
    }
}
