using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour 
{
    [Header("Components")]
    [SerializeField] private Transform groundCheck = null;
    [SerializeField] private LayerMask groundMask = 512;
    private CharacterController controller = null;

    [Header("General")]
    [SerializeField] private float standUpHeight = 1.8f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float groundDistance = 0.5f;
    private Vector3 velocity = Vector3.zero;
    private bool ceilingAbove = false;
    public bool OnGround { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool IsCrouching { get; private set; }

    [Header("Movement")]
    [SerializeField] [Range(0, 15)] private float forwardSpeed = 4f;
    [SerializeField] [Range(0, 15)] private float lateralSpeed = 2f;

    [Header("Input Multipliers")]
    [SerializeField] [Range(0, 1)] private float crouchingMultiplier = 0.5f;
    [SerializeField] [Range(1, 5)] private float sprintingMultiplier = 1.5f;

    [Header("Jumping")]
    [SerializeField] [Range(0, 6)] public float jumpHeight = 0.7f;


    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        controller.height = standUpHeight;
    }

	void Update ()
    {
        ceilingAbove = Physics.CheckSphere(transform.position + transform.up * (standUpHeight / 2), groundDistance, groundMask);
        OnGround = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        IsSprinting = Controls.GetAction(UserAction.Sprint);

        // Resets the player's velocity. 
        if (OnGround)
        {
            // If on the ground, set the velocity to 0, as it will be modified by input (if any).
            velocity.x = 0f;
            velocity.z = 0f;
            // It is set to smaller than 0
            // only to make sure that the player stays firmly onto the ground.
            if (velocity.y < 0) velocity.y = -2f;
        }

        // Gather input and make sure that the diagonal movement is functioning correctly.
        Vector3 input = new Vector3(Controls.GetAxis(InputAxis.Horizontal), 0, Controls.GetAxis(InputAxis.Vertical));
        if (input.magnitude > 1) input /= input.magnitude;

        // Modify the player velocity based on input.
        if(OnGround)
        {
            // Calculating the horizontal (lateral) movement.
            float horizontalMove = input.x * lateralSpeed;

            // Calculating the vertical (forward / backward) movement.
            float verticalMove = input.z;
            if (input.z > 0) verticalMove *= forwardSpeed;
            else verticalMove *= lateralSpeed;

            // Calculating the multipliers.
            float multiplier = 1f;
            if (IsCrouching)
            {
                multiplier *= crouchingMultiplier;
            }
            else if (IsSprinting) 
            {
                multiplier *= sprintingMultiplier;
            }

            if (input.x != 0)
                velocity.x = horizontalMove * multiplier;
            if (input.z != 0)
                velocity.z = verticalMove * multiplier;
        }

        // Jump implementation
        if(Controls.GetAction(UserAction.Jump) && OnGround && !ceilingAbove)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * 2 * gravity);
        }

        // Crouch implementation
        if(Controls.GetAction(UserAction.Crouch))
        {
            if(!IsCrouching)
            {
                controller.height = standUpHeight / 2;
                IsCrouching = true;
                if(OnGround)
                {
                    velocity.y = -1000;
                }
            }
        }
        else if(IsCrouching && !ceilingAbove)
        {
            controller.height = standUpHeight;
            IsCrouching = false;
        }

        // Fixes collision issues with objects that have collision
        if (!OnGround) 
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
        // Apply the gravity onto the velocity
        velocity.y -= gravity * Time.deltaTime; 

        // Move the player based on the velocity
        controller.Move((velocity.x * transform.right + velocity.y * transform.up + velocity.z * transform.forward) * Time.deltaTime); 
	}
}
