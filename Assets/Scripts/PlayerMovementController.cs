using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class PlayerMovementController : NetworkBehaviour
{
    public float Speed = 20.0f;
    public GameObject PlayerModel;
    public float MouseSensitivity = 200f;
    public float xRotation = 0f;
    public Camera Camera;
    public CharacterController Controller;

    private void Start()
    {
        PlayerModel.SetActive(false);
    }

    private void Update()
    {
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
        HandleWalking();
        HandleLooking();
    }

    private void HandleWalking()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.right * x + transform.forward * z;

        Controller.Move(Speed * Time.deltaTime * moveDirection);
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
}
