using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    #region Variables
    [Header("Components")]
    [SerializeField] private Transform groundCheck = null;
    [SerializeField] private LayerMask groundMask = 512;
    private CharacterController controller = null;
    [SerializeField] private Animator headAnimator = null;

    [Header("General")]
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float gravityWhileWallRunning = 4f;
    private float currentGravity = 0f;
    [SerializeField] private float groundDistance = 0.5f;
    private Vector3 velocity = Vector3.zero;
    private bool ceilingAbove = false;
    private float ignoreOnGroundTime = 0f;

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
    public bool IsWallRunning { get; private set; }
    /// <summary> Used for proper movement and calculations of wall sticking and running. </summary>
    private Vector3 wallRunningGlobalDirection = Vector3.zero;
    /// <summary> Used only for finding the correct wall running animations. </summary>
    private Vector3 wallRunningLocalDirection = Vector3.zero;

    [Header("Jumping")]
    [SerializeField] [Range(0, 6)] public float jumpHeight = 1.0f;
    public bool OnGround { get; private set; }
    private float timeInAir = 0f;

    [Header("Crouching & sliding")]
    [SerializeField] private float standUpHeight = 1.8f;
    [SerializeField] private float crouchHeight = 1f;
    public bool IsCrouching { get; private set; }
    private bool isSliding = false;
    [SerializeField] [Range(0.05f, 1.0f)] private float slidingStartDifference = 0.30f;
    [SerializeField] [Range(0.05f, 1.0f)] private float slidingStopDifference = 0.15f;
    #endregion



    #region Methods
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        controller.height = standUpHeight;
        currentGravity = gravity;
    }



    /// <summary> 
    /// Checks if the player can wall run in the given direction. Returns true to the wall if able to, else returns false.
    /// </summary>
    private bool CanWallRun(Vector3 direction)
    {
        return Physics.SphereCast(new Ray(transform.position, direction), controller.radius, controller.radius * 1.5f, groundMask, QueryTriggerInteraction.Ignore);
    }



    private void Update()
    {
        #region Updating status
        // Firstly, updating the current player status
        OnGround = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask, QueryTriggerInteraction.Ignore);
        ceilingAbove = Physics.CheckSphere(transform.position + transform.up * standUpHeight, groundDistance, groundMask, QueryTriggerInteraction.Ignore);
        #endregion

        #region Resetting velocity, timeInAir and IsWallRunning.
        // Resets the player's velocity. 
        if (OnGround)
        {
            // If on the ground, set the velocity to 0, as it will be modified by input (if any).
            velocity.x = 0f;
            velocity.z = 0f;
            // It is set to smaller than 0
            // only to make sure that the player stays firmly onto the ground.
            if (velocity.y < 0) velocity.y = -2f;

            if (timeInAir != 0f)
            {
                headAnimator.SetTrigger("Fell");
                headAnimator.SetFloat("TimeInAir", timeInAir);
                timeInAir = 0f;
            }
        }
        #endregion

        #region Check for action input

        // Check if we wanna wall run or jump
        if (Controls.GetActionDown(UserAction.Jump) && OnGround && !IsWallRunning && !ceilingAbove)
        {
            if (IsSprinting && !IsCrouching)
            {
                // Check if there is any direction in which we can wall run.
                foreach (var actionDir in new Dictionary<UserAction, Vector3>() { { UserAction.Left, -transform.right }, { UserAction.Right, transform.right }, { UserAction.Forward, transform.forward } })
                {
                    if (Controls.GetAction(actionDir.Key))
                    {
                        if (CanWallRun(actionDir.Value))
                        {
                            wallRunningGlobalDirection = actionDir.Value;
                            if (actionDir.Value == -transform.right) wallRunningLocalDirection = Vector3.left;
                            else if (actionDir.Value == transform.right) wallRunningLocalDirection = Vector3.right;
                            else wallRunningLocalDirection = Vector3.forward;
                            break;
                        }
                    }
                }
            }

            // If we can wall run, let's do it!
            if (wallRunningGlobalDirection != Vector3.zero)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * 2 * currentGravity);
                ignoreOnGroundTime = 2f;
                IsWallRunning = true;
            }
            // Then, we jump.
            else
            {
                velocity.y = Mathf.Sqrt(jumpHeight * 2 * currentGravity);
                headAnimator.SetTrigger("Jumped");
            }
        }

        // Make sure we stick to the wall if we're wall running
        if (IsWallRunning)
        {
            controller.Move(wallRunningGlobalDirection * Time.deltaTime);
            currentGravity = gravityWhileWallRunning;
            ignoreOnGroundTime -= Time.deltaTime;
            if ((OnGround && ignoreOnGroundTime <= 0f) || !CanWallRun(wallRunningGlobalDirection))
            {
                currentGravity = gravity;
                IsWallRunning = false;
                wallRunningGlobalDirection = Vector3.zero;
                wallRunningLocalDirection = Vector3.zero;
                ignoreOnGroundTime = 0f;
            }
        }

        // Check if currently holding down sprint
        if (!IsCrouching)
        {
            if (Controls.GetAction(UserAction.Sprint))
            {
                IsSprinting = true;
            }
            else
            {
                IsSprinting = false;
            }
        }

        // Check for crouching
        if (Controls.GetActionDown(UserAction.Crouch))
        {
            controller.height = crouchHeight;
            groundCheck.position += transform.up * crouchHeight / 2;
            IsCrouching = true;
            if (OnGround)
            {
                if (!IsSprinting)
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
            headAnimator.SetBool("IsSliding", false);
        }

        // Check if we should slide or not
        if (IsCrouching && Mathf.Abs(currentMultiplier - sprintingMultiplier) <= slidingStartDifference)
        {
            isSliding = true;
            headAnimator.SetBool("IsSliding", true);
        }

        // Transitioning speed and calculating the current multiplier.
        if (OnGround)
        {
            if (IsCrouching) targetMultiplier = crouchingMultiplier;
            else if (IsSprinting) targetMultiplier = sprintingMultiplier;
            else targetMultiplier = baseMultiplier;
            currentMultiplier = Mathf.Lerp(currentMultiplier, targetMultiplier, (isSliding ? slidingTransitionSpeed : generalTransitionSpeed) * Time.deltaTime);

            // If the speed got back to the crouching speed, stop sliding (if necessary)
            if (Mathf.Abs(currentMultiplier - crouchingMultiplier) <= slidingStopDifference)
            {
                isSliding = false;
                headAnimator.SetBool("IsSliding", false);
            }
        }
        else
        {
            timeInAir += Time.deltaTime;
            headAnimator.SetFloat("TimeInAir", timeInAir);
        }
        #endregion

        #region Update velocity and getting axis input
        // Gather input and make sure that the diagonal movement is functioning correctly.
        Vector3 input = new Vector3(Controls.GetAxis(InputAxis.Horizontal), 0, Controls.GetAxis(InputAxis.Vertical));
        if (input.magnitude > 1) input /= input.magnitude;

        // Modify the player velocity based on input.
        if (OnGround)
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
        velocity.y -= currentGravity * Time.deltaTime;
        #endregion

        #region Moving
        // Move the player based on the velocity
        controller.Move((velocity.x * transform.right + velocity.y * transform.up + velocity.z * transform.forward) * Time.deltaTime);
        #endregion

        #region Updating animations
        // Update animations
        headAnimator.SetBool("IsSprinting", IsSprinting);
        headAnimator.SetBool("IsWalking", IsWalking);
        headAnimator.SetBool("OnGround", OnGround);
        headAnimator.SetBool("IsWallRunning", IsWallRunning);
        headAnimator.SetFloat("WallRunningDirX", wallRunningLocalDirection.x);
        headAnimator.SetFloat("WallRunningDirZ", wallRunningLocalDirection.z);
        #endregion

        #region Debugging
        string str = "";
        // Wall running
        str += "Is " + (IsWallRunning ? "" : "not ") + "wall running";
        str += "\t|\t";
        // Status
        List<string> currentStatuses = new List<string>();
        if (isSliding) currentStatuses.Add("sliding");
        if (IsCrouching) currentStatuses.Add("crouching");
        if (IsWalking) currentStatuses.Add("walking");
        if (IsSprinting) currentStatuses.Add("sprinting");
        if (IsWallRunning) currentStatuses.Add("wall running");
        if (OnGround) currentStatuses.Add("on ground");
        //else currentStatuses.Add("in air for " + timeInAir + " seconds");
        currentStatuses.Add("current gravity: " + currentGravity);
        str += "Current status: ";
        currentStatuses.ForEach(e => str += e + ", ");
        str = str.Remove(str.Length - 2, 2);

        print(str);
        #endregion
    }
    #endregion
}
