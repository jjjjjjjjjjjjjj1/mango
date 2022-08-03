using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class PlayerMovementController : NetworkBehaviour
{
    public float Speed = 20.0f;
    public float gravity = -9.81f;
    public float MouseSensitivity = 200f;
    public float jumpHeight = 3f;

    public GameObject PlayerModel;
    float xRotation = 0f;
    public Camera Camera;
    public CharacterController Controller;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    Vector3 velocity;
    bool isGrounded;


    private void Start()
    {
        PlayerModel.SetActive(false);
    }

    private void Update()
    {
        // TODO: optimize by subscribing to SceneManager.activeSceneChanged instead of getting active scene every frame
        if (SceneManager.GetActiveScene().name == "Game")
        {
            if (!PlayerModel.activeSelf)
            {
                SetPosition();
                PlayerModel.SetActive(true);
                if (hasAuthority)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }

            // Stuff that only the player can do to itself
            // If we don't do this check, every player will be able to control everyone else's movement
            if (hasAuthority)
            {
                HandleMovement();
            }
        }
    }

    public void SetPosition()
    {
        transform.position = new Vector3(Random.Range(-5, 5), 0.8f, Random.Range(-15, 7));
    }

    public void HandleMovement()
    {
        HandleVelocity();
        HandleWalking();
        HandleJumping();
        HandleLooking();
        HandleGravity();
    }

    private void HandleVelocity()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    private void HandleWalking()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.right * x + transform.forward * z;

        Controller.Move(Speed * Time.deltaTime * moveDirection);
    }

    private void HandleJumping()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void HandleLooking()
    {
        float mouseX = Input.GetAxis("Mouse X") * MouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        Camera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        gameObject.transform.Rotate(Vector3.up * mouseX);

    }

    private void HandleGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        Controller.Move(velocity * Time.deltaTime);
    }
}
