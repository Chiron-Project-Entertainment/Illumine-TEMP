using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    /// <summary>
    /// StepOfset causes issues with collision e.g. steo ofset etc
    /// </summary>

    // Variables

    public CharacterController controller;

    public float speed = 12f; // Player movement speed

    // Velocity implementation

    public float gravity = -9.81f;

    Vector3 velocity;

    // Ground check implementation

    public Transform groundCheck; // References the Ground Check object on the First Person Player for use later in script

    public float groundDistance = 0.4f; // Radius of the sphere that will check for collision 

    public LayerMask groundMask; // Controlls what objects the sphere should check for

    bool isGrounded;

    // Jump Implementation

    public float lowJumpHeight = 2f;

    public float highJumpHeight = 3.5f;

    private float timeHeld { get { return jumpHoldStart != -1 ? (Time.time - jumpHoldStart) : 0f; } }

    private float jumpHoldStart = -1f;

    public float timeToHold = 0.3f;
	void Update ()
    {
        // Checks if the player is grounded

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask); // Uses the result of the physics check and creates a tiny invisible sphere ->
        //beneath the player with the radius specified and if it collides with anything in the groundMask then isGrounded will be true.

        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Forces the player down on the ground 
        }

        // Gathers input
        float x = Controls.GetAxis(InputAxis.Horizontal);
        float z = Controls.GetAxis(InputAxis.Vertical);

        Vector3 move = transform.right * x + transform.forward * z; // Takes the direction the player is facing and goes to the right and forward
        
        if(move.sqrMagnitude > 1) // Fixes diagnal movement speed
        {
            move = move / move.sqrMagnitude; // sqrMagnitude is the vectors squred length
        }

        controller.Move(move * speed * Time.deltaTime); // NEED EXPLANTION // Time.deltaTime provides frame rate independence by providing the time between the current and previous frame

        // Jump implementation
        
        if(Controls.GetActionDown(UserAction.Jump))
        {
            jumpHoldStart = Time.time;
        }


        if(Controls.GetActionUp(UserAction.Jump) && isGrounded)
        {
            if (timeHeld < timeToHold)
            {
                velocity.y = Mathf.Sqrt(lowJumpHeight * -2 * gravity);
            }
            else
            {
                velocity.y = Mathf.Sqrt(highJumpHeight * -2 * gravity);
            }
            jumpHoldStart = -1f;
        }

        if (!isGrounded) // Fixes collision issues with objects that have collision
        {
            controller.slopeLimit = 0f;
            controller.stepOffset = 0f;
        }
        else
        {
            controller.stepOffset = 0.7f;
            controller.slopeLimit = 45f;
        }

        // Velocity implementation

        velocity.y += gravity * Time.deltaTime; // Increases velocity by the gravity number 

        controller.Move(velocity * Time.deltaTime); // Adds velocity to the player // Player moves based on velocity 
	}
}
