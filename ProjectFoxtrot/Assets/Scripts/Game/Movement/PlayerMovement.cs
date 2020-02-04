using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour 
{
#pragma warning disable IDE0044 // Add readonly modifier
    [Header("Components")]
    [SerializeField] private Transform groundCheck = null;
    [SerializeField] private LayerMask groundMask = 512;
    private CharacterController controller = null;
    [SerializeField] private Animator headAnimator = null;

    [Header("General")]
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float groundDistance = 0.5f;
    private Vector3 velocity = Vector3.zero;
    private bool ceilingAbove = false;

    [Header("Multipliers")]
    [SerializeField] private float generalTransitionSpeed = 4f;
    [SerializeField] private float slidingTransitionSpeed = 0.75f;
    [SerializeField] [Range(1, 5)] private float sprintingMultiplier = 1.5f;
    [SerializeField] [Range(0, 1)] private float crouchingMultiplier = 0.5f;
    private readonly float baseMultiplier = 1f;
    private float currentMultiplier = 1f;
    private float targetMultiplier = 1f;

    [Header("Movement")]
    [SerializeField] [Range(0, 15)] private float forwardSpeed = 4.0f;
    [SerializeField] [Range(0, 15)] private float lateralSpeed = 2.0f;
    public bool IsSprinting { get; private set; }
    public bool IsWalking { get { return !isSliding && !IsSprinting && OnGround && (Mathf.Abs(velocity.x) > 0.35f || Mathf.Abs(velocity.z) > 0.35f); } }

    [Header("Jumping")]
    [SerializeField] [Range(0, 6)] public float jumpHeight = 1.0f;
    public bool OnGround { get; private set; }

    [Header("Crouching & sliding")]
    [SerializeField] private float standUpHeight = 1.8f;
    [SerializeField] private float crouchHeight = 1f;
    public bool IsCrouching { get; private set; }
    private bool isSliding = false;
    [SerializeField] [Range(0.05f, 1.0f)] private float slidingStartDifference = 0.30f;
    [SerializeField] [Range(0.05f, 1.0f)] private float slidingStopDifference = 0.15f;

#pragma warning restore IDE0044 // Add readonly modifier


    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        controller.height = standUpHeight;
    }

	void Update ()
    {
        // Firstly, updating the current player status
        OnGround = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        ceilingAbove = Physics.CheckSphere(transform.position + transform.up * standUpHeight, groundDistance, groundMask);


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


        // We jumping?
        if (Controls.GetActionDown(UserAction.Jump) && OnGround && !ceilingAbove)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * 2 * gravity);
            headAnimator.SetTrigger("Jumped");
        }


        // Check if currently holding down sprint
        if(!IsCrouching)
        {
            if(Controls.GetAction(UserAction.Sprint))
            {
                IsSprinting = true;
            }
            else
            {
                IsSprinting = false;
            }
        }


        // Check for crouching / unchrouching
        if (Controls.GetActionDown(UserAction.Crouch))
        {
            controller.height = crouchHeight;
            groundCheck.position += transform.up * crouchHeight / 2;
            IsCrouching = true;
            if (OnGround)
            {
                if(!IsSprinting)
                {
                    velocity.y = -1000;
                }
            }
        }
        else if (IsCrouching && !ceilingAbove && !Controls.GetAction(UserAction.Crouch))
        {
            controller.height = standUpHeight;
            groundCheck.position -= transform.up * crouchHeight / 2;
            IsCrouching = false;
            isSliding = false;
        }


        // Check if we should slide or not
        if (IsCrouching && Mathf.Abs(currentMultiplier - sprintingMultiplier) <= slidingStartDifference) 
        {
            isSliding = true;
        }


        // Transitioning speed and calculating the current multiplier.
        if(OnGround)
        {
            if (IsCrouching) targetMultiplier = crouchingMultiplier;
            else if (IsSprinting) targetMultiplier = sprintingMultiplier;
            else targetMultiplier = baseMultiplier;
            currentMultiplier = Mathf.Lerp(currentMultiplier, targetMultiplier, (isSliding ? slidingTransitionSpeed : generalTransitionSpeed) * Time.deltaTime);

            // If the speed got back to the crouching speed, stop sliding (if necessary)
            if (Mathf.Abs(currentMultiplier - crouchingMultiplier) <= slidingStopDifference)
                isSliding = false;
        }
        print("Current multiplier: " + currentMultiplier + " | IsSliding: " + isSliding + " | IsCrouching: " + IsCrouching + " | IsSprinting: " + IsSprinting);


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

            if (input.x != 0)
                velocity.x = horizontalMove * currentMultiplier;
            if (input.z != 0)
                velocity.z = verticalMove * currentMultiplier;
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

        // Update animations
        headAnimator.SetBool("IsSliding", isSliding);
        headAnimator.SetBool("IsSprinting", IsSprinting);
        headAnimator.SetBool("IsWalking", IsWalking);
        headAnimator.SetBool("OnGround", OnGround);
	}
}
