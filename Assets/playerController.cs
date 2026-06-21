using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class playerController : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 10f;
    public ParticleSystem confetti;
    public CameraFollow cameraFollow;
    public GameObject cameraLookHint;

    private Rigidbody2D rb;
    private bool isGrounded;
    private Vector3 respawnPoint;
    private bool hasCameraLookReward;
    private bool controlsLocked;
    private readonly HashSet<Collider2D> groundContacts = new HashSet<Collider2D>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        respawnPoint = transform.position;

        if (cameraLookHint != null)
            cameraLookHint.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (cameraLookHint != null && cameraLookHint.activeSelf && Keyboard.current.zKey.wasPressedThisFrame)
        {
            cameraLookHint.SetActive(false);
            controlsLocked = false;
        }

        if (controlsLocked)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float move = 0f;

        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * 2f * Time.deltaTime;
        }

        if (Keyboard.current.leftArrowKey.isPressed)
            move = -1f;

        if (Keyboard.current.rightArrowKey.isPressed)
            move = 1f;

        rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);

        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        if (Keyboard.current.spaceKey.wasReleasedThisFrame && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Spike"))
        {
            Respawn();
        }

        if (other.CompareTag("Checkpoint"))
        {
            respawnPoint = other.transform.position;
        }

        if (other.CompareTag("Finish"))
        {
            UnlockCameraLookReward();
        }
    }

    void UnlockCameraLookReward()
    {
        if (hasCameraLookReward) return;

        hasCameraLookReward = true;

        if (cameraFollow != null)
            cameraFollow.UnlockLookAround();

        if (cameraLookHint != null)
        {
            cameraLookHint.SetActive(true);
            controlsLocked = true;
            rb.linearVelocity = Vector2.zero;
        }

        if (confetti != null)
            confetti.Play();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        TrackGroundContact(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        TrackGroundContact(collision);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            groundContacts.Remove(collision.collider);
            isGrounded = groundContacts.Count > 0;
        }
    }

    public void Respawn()
    {
        transform.position = respawnPoint;
        rb.linearVelocity = Vector2.zero;
        groundContacts.Clear();
        isGrounded = false;
    }

    public void SetControlsLocked(bool locked)
    {
        controlsLocked = locked;

        if (locked && rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    void TrackGroundContact(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Ground"))
            return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y > 0.5f)
            {
                groundContacts.Add(collision.collider);
                isGrounded = true;
                return;
            }
        }
    }
}
