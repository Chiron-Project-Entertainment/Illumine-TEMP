///////////////////////////////////////////////////////////////
///                                                         ///
///             Script coded by Hakohn (Robert).            ///
///                                                         ///
///////////////////////////////////////////////////////////////

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    #region Variables
    [Header("Components")]
    [SerializeField] private LayerMask groundMask = 512;
    [SerializeField] private Animator headAnimator = null;
    [SerializeField] private MouseLook mouseLook = null;
    private CharacterController controller = null;

    /// <summary>
    /// Collision checks
    /// </summary>
    private struct SphereCheck
    {
        public Vector3 position;
        public float radius;
        public SphereCheck(Vector3 position, float radius) { this.position = position; this.radius = radius; }
    }
    private SphereCheck groundCheck { get { return new SphereCheck(transform.TransformPoint(controller.center) - transform.up * (controller.height / 2), 0.4f); } }
    private SphereCheck ceilingCheck { get { return new SphereCheck(transform.TransformPoint(controller.center) + transform.up * crouchHeight, 0.4f); } }
    private HashSet<Vector3> wallRunningDirectionalPossibilities = null;

    [Header("General")]
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float gravityWhileWallRunning = 4f;
    private float currentGravity = 0f;
    [HideInInspector] public Vector3 velocity = Vector3.zero;
    private bool ceilingAbove = false;
    private float ignoreOnGroundTime = 0f;

    [Header("Multipliers")]
    [SerializeField] private float generalTransitionSpeed = 4.0f;
    [SerializeField] private float slidingTransitionSpeed = 0.75f;
    [SerializeField][Range(1, 5)] private float sprintingMultiplier = 2.0f;
    [SerializeField][Range(0, 1)] private float crouchingMultiplier = 0.5f;
    private readonly float baseMultiplier = 1f;
    private float currentMultiplier = 1f;
    private float targetMultiplier = 1f;

    [Header("Movement")]
    [SerializeField][Range(0, 15)] private float forwardSpeed = 4.0f;
    [SerializeField][Range(0, 15)] private float lateralSpeed = 2.0f;
    public bool IsSprinting { get; private set; }
    public bool IsWalking { get { return !isSliding && !IsSprinting && OnGround && (Mathf.Abs(velocity.x) > 0.35f || Mathf.Abs(velocity.z) > 0.35f); } }
    public bool IsWallRunning { get; private set; }
    /// <summary> Local wall running direction used for proper movement and calculations of wall sticking and running. </summary>
    private Vector3 wallRunningDirection = Vector3.zero;

    [Header("Jumping")]
    [SerializeField][Range(0, 6)] public float jumpHeight = 0.9f;
    public bool OnGround { get; private set; }
    private float timeInAir = 0f;
    private bool wallFell = false;

    [Header("Crouching & sliding")]
    [SerializeField] private float standUpHeight = 1.8f;
    [SerializeField] private float crouchHeight = 1f;
    private float timeSliding = 0f;
    public bool IsCrouching { get; private set; }
    public bool isSliding { get; private set; }
    #endregion

    #region Methods
    private void Awake()
    {
        // Setting up the components
        controller = GetComponent<CharacterController>();
        controller.height = standUpHeight;

        // Setting up the general variables
        currentGravity = gravity;

        // Setting up the movement variables
        wallRunningDirectionalPossibilities = new HashSet<Vector3>() { Vector3.left, Vector3.right, Vector3.forward };
    }

    /// <summary> 
    /// Checks if the player can wall run in the given direction. Returns true to the wall if able to, else returns false.
    /// </summary>
    private bool CanWallRun(Vector3 localDirection)
    {
        bool able = Physics.Raycast(
            transform.TransformPoint(controller.center),
            transform.TransformDirection(localDirection),
            controller.radius * 2,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
        return able;
    }

    /// <summary>
    /// Stop wall running, by resetting the gravity, wall running direction and velocity, updating the boolean, and "removing" ignoreOnGroundTime.
    /// </summary>
    private void StopWallRunning()
    {
        currentGravity = gravity;
        IsWallRunning = false;
        headAnimator.SetFloat("LastWallDirectionX", wallRunningDirection.x);
        headAnimator.SetFloat("LastWallDirectionZ", wallRunningDirection.z);
        velocity.y = 0;
        wallRunningDirection = Vector3.zero;
        ignoreOnGroundTime = 0f;
    }

    /// <summary>
    /// Reset the TimeInAir back to 0, while updating the LastTimeInAir and TimeInAir values of the animator.
    /// </summary>
    private void ResetTimeInAir()
    {
        headAnimator.SetFloat("LastTimeInAir", timeInAir);
        timeInAir = 0f;
        headAnimator.SetFloat("TimeInAir", timeInAir);
    }

    private void Update()
    {
        #region Updating status
        // Firstly, updating the current player status
        OnGround = controller.isGrounded /*Physics.CheckSphere(groundCheck.position, groundCheck.radius, groundMask, QueryTriggerInteraction.Ignore)*/ ;
        ceilingAbove = Physics.CheckSphere(ceilingCheck.position, ceilingCheck.radius, groundMask, QueryTriggerInteraction.Ignore);
        #endregion

        #region Resetting velocity, timeInAir and gather new movement input.
        // Gather input and make sure that the diagonal movement is functioning correctly.
        Vector3 input = new Vector3(Controls.GetAxis(InputAxis.Horizontal), 0, Controls.GetAxis(InputAxis.Vertical));
        if (input.magnitude > 1) input /= input.magnitude;

        // Resets the player's velocity, and gathers the new one based on input.
        if (OnGround)
        {
            // Fixes collision issues with objects that have collision.
            controller.stepOffset = 0.4f;
            controller.slopeLimit = 45f;

            // If on the ground, set the velocity to 0, as it will be modified by input (if any).
            velocity.x = 0f;
            velocity.z = 0f;
            // It is set to smaller than 0
            // only to make sure that the player stays firmly onto the ground.
            if (velocity.y < 0) velocity.y = -2f;

            headAnimator.ResetTrigger("Fell");
            headAnimator.ResetTrigger("WallFell");
            if (timeInAir != 0f)
            {
                ResetTimeInAir();
                if (IsWallRunning || wallFell)
                {
                    wallFell = false;
                    headAnimator.SetTrigger("WallFell");
                }
                else
                {
                    headAnimator.SetTrigger("Fell");
                }
            }

            if (isSliding)
            {
                timeSliding += Time.deltaTime;
            }

            // Calculating the velocity
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
        else
        {
            controller.slopeLimit = 0f;
            controller.stepOffset = 0f;
        }
        #endregion

        #region Check for action input
        // Check if we wanna wall run, basic jump or wall jump.
        if (Controls.GetActionDown(UserAction.Jump) && !ceilingAbove && !IsCrouching)
        {
            // Are we in state from which we can wall run or do a basic jump?
            if (!IsWallRunning && OnGround)
            {
                // If we're sprinting, then let's see if we can wall run.
                if (IsSprinting)
                {
                    // Check if there is any direction in which we can wall run.
                    foreach (var wallDirection in wallRunningDirectionalPossibilities)
                        if (CanWallRun(wallDirection))
                        {
                            // We found a direction in which we can and want to wall run, 
                            // so let's do it and break out of this (so we won't do wall run in multiple directions)!
                            wallRunningDirection = wallDirection;
                            velocity.y = Mathf.Sqrt(jumpHeight * 2 * currentGravity);
                            ignoreOnGroundTime = .5f;
                            IsWallRunning = true;
                            break;
                        }
                }
                // If we're not sprinting and / or we are not wall running, we'll do a basic jump.
                if (!IsWallRunning)
                {
                    velocity.y = Mathf.Sqrt(jumpHeight * 2 * currentGravity);
                    headAnimator.SetTrigger("Jumped");
                }
            }
            // If we're wall running and we've left the ground for a while now, then let's wall jump!
            else if (IsWallRunning && ignoreOnGroundTime <= 0f)
            {
                // Jumping in the opposite direction
                if (wallRunningDirection == Vector3.left || wallRunningDirection == Vector3.right)
                {
                    velocity.x += -wallRunningDirection.x * sprintingMultiplier * forwardSpeed;
                }
                else
                {
                    StartCoroutine(mouseLook.RotateHorizontal(Mathf.Pow(-1, Random.Range(0, 100)) * 180));
                    velocity.z -= -wallRunningDirection.z * sprintingMultiplier * forwardSpeed;
                }
                StopWallRunning();
                velocity.y = Mathf.Sqrt(jumpHeight * 2 * currentGravity);
                headAnimator.SetTrigger("WallJumped");
                ResetTimeInAir();
            }
        }

        // Make sure we stick to the wall if we're wall running
        if (IsWallRunning)
        {
            controller.Move(transform.TransformDirection(wallRunningDirection) * Time.deltaTime);
            currentGravity = gravityWhileWallRunning;
            ignoreOnGroundTime -= Time.deltaTime;
            // Maybe we are no longer on the wall or we've reached the ground and need to stop wall running.
            if ((OnGround && ignoreOnGroundTime <= 0f) || !CanWallRun(wallRunningDirection))
            {
                StopWallRunning();
                wallFell = true;
            }
        }

        // Check if currently holding down sprint
        if (Controls.GetAction(UserAction.Sprint) && !IsCrouching && (velocity.z >= forwardSpeed * 0.9f))
        {
            IsSprinting = true;
        }
        else
        {
            IsSprinting = false;
        }

        // Check for crouching
        if (Controls.GetActionDown(UserAction.Crouch))
        {
            controller.height = crouchHeight;
            IsCrouching = true;
            if (OnGround && !IsSprinting)
            {
                controller.Move(transform.up * crouchHeight * -1);
            }
        }
        else if (IsCrouching && !ceilingAbove && !Controls.GetAction(UserAction.Crouch))
        {
            controller.height = standUpHeight;
            IsCrouching = false;
            isSliding = false;
            timeSliding = 0f;
            headAnimator.SetBool("IsSliding", false);
        }

        // Check if we should slide or not
        if (IsCrouching && Mathf.Abs(currentMultiplier - sprintingMultiplier) <= 0.30f)
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
            if (Mathf.Abs(currentMultiplier - crouchingMultiplier) <= 0.15f)
            {
                isSliding = false;
                headAnimator.SetBool("IsSliding", false);
                timeSliding = 0f;
            }
        }
        else
        {
            timeInAir += Time.deltaTime;
            headAnimator.SetFloat("TimeInAir", timeInAir);
        }
        #endregion

        #region Moving and gravity application.
        // Apply the gravity onto the velocity.
        velocity.y -= currentGravity * Time.deltaTime;

        // Move the player based on the velocity.
        controller.Move((velocity.x * transform.right + velocity.y * transform.up + velocity.z * transform.forward) * Time.deltaTime);
        #endregion

        #region Updating animations
        // Update animations
        headAnimator.SetFloat("Multiplier", IsSprinting ? 2f : (IsWalking ? 1f : (IsCrouching ? 0.5f : 0f)));
        headAnimator.SetFloat("TimeSliding", timeSliding);
        headAnimator.SetBool("IsWallRunning", IsWallRunning);
        headAnimator.SetFloat("WallDirectionX", wallRunningDirection.x);
        headAnimator.SetFloat("WallDirectionZ", wallRunningDirection.z);
        #endregion

        #region Debugging
        string str = "";
        // // Wall running
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

        //print(str);
        #endregion
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(groundCheck.position, groundCheck.radius);
        Gizmos.DrawSphere(ceilingCheck.position, ceilingCheck.radius);
    }
    #endregion
}
