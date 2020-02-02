using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRigidbodyMovement : MonoBehaviour 
{
    // Components
    private Rigidbody rb = null;
    private CapsuleCollider capsuleCollider = null;
    private float originalColliderHeight = 2f;

    // Movement variables
    [SerializeField][Range(0, 12f)]
    private float forwardMovementSpeed = 6f;
    [SerializeField][Range(0, 12f)]
    private float lateralMovementSpeed = 3f;
    [SerializeField]
    private Transform groundCheck = null;
    [SerializeField]
    private float groundDistance = 0.4f;
    [SerializeField]
    private LayerMask groundMask = 512;
    [SerializeField][Range(0, 4f)]
    private float jumpHeight = 0.7f;

    // Input variables
    private Vector3 input = Vector3.zero;

    private bool onGround { get { return Physics.CheckSphere(groundCheck.position, groundDistance, groundMask, QueryTriggerInteraction.Ignore); } }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        originalColliderHeight = capsuleCollider.height;
    }

    private void Update()
    {
        input = new Vector3(Controls.GetAxis(InputAxis.Horizontal), 0, Controls.GetAxis(InputAxis.Vertical));
        if (input.magnitude > 1) input /= input.magnitude;

        if (Controls.GetAction(UserAction.Jump) && onGround)
        {
            rb.velocity = new Vector3(rb.velocity.x, Mathf.Sqrt(2 * -Physics.gravity.y * jumpHeight), rb.velocity.z);
        }

        if(Controls.GetActionDown(UserAction.Crouch))
        {
            capsuleCollider.height = originalColliderHeight / 2;
        }
        else if (Controls.GetActionUp(UserAction.Crouch))
        {
            capsuleCollider.height = originalColliderHeight;
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + (
            transform.right * input.x * lateralMovementSpeed + 
            transform.forward * input.z * (input.z > 0f ? forwardMovementSpeed : lateralMovementSpeed)) * 
            Time.fixedDeltaTime);
    }
}
