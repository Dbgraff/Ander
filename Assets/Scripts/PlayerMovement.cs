using UnityEngine;

/// <summary>
/// Базовое перемещение игрока
/// </summary>
[RequireComponent (typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Движение")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Ускорение")]
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftControl;

    [Header("Присед")]
    [SerializeField] private float slowSpeed = 2.5f;
    [SerializeField] private KeyCode slowKey = KeyCode.LeftShift;

    private CharacterController controller;
    private Transform cam;
    private Vector3 velocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        cam = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Move();
    }

    private float GetCurrentSpeed()
    {
        if (Input.GetKey(slowKey)) return slowSpeed;
        if (Input.GetKey(sprintKey)) return sprintSpeed;
        return moveSpeed;
    }

    private void Move()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;

        Vector3 move = (camRight * x + camForward * z).normalized;

        if (move.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        controller.Move(move * GetCurrentSpeed() * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
