using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class PlayerMovementController : NetworkBehaviour
{
    public float Speed = 0.05f;
    public GameObject PlayerModel;

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
            }

            // Stuff that only the player can do to itself
            // If we don't do this check, every player will be able to control everyone else's movement
            if (hasAuthority)
            {
                Movement();
            }
        }
    }

    public void SetPosition()
    {
        transform.position = new Vector3(Random.Range(-5, 5), 0.8f, Random.Range(-15, 7));
    }

    public void Movement()
    {
        float xDirection = Input.GetAxis("Horizontal");
        float zDirection = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(xDirection, 0.0f, zDirection);

        transform.position += moveDirection * Speed;
    }
}
