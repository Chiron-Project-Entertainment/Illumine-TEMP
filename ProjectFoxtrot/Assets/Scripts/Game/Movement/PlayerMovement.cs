///////////////////////////////////////////////////////////////
///                                                         ///
///             Script coded by Hakohn (Robert).            ///
///                                                         ///
///////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The main script used for all the player movement and free-running. It deals with simulating the rigidbody physics,
/// with handling the movement animations, with the movement interaction with the world, collisions, and so on.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    #region Variables
    [Header("Components")]
    /// <summary> The mask we're gonna check objects in for all our movement. </summary>
    [SerializeField] private LayerMask groundMask = 512;
    /// <summary> All the first-person camera animations. </summary>
    [SerializeField] private Animator headAnimator = null;
    /// <summary> The method we're using for rotating our camera. Given the fact that this script is already referenced in
    /// the script having the headRotationFunc, it's implemented from there. </summary>
    [HideInInspector] public Func<float, IEnumerator> headRotationFunc = null;
    /// <summary> The owner of all the movement and collision. Poor boi. </summary>
    private CharacterController controller = null;

    /// <summary> Collision checks, "beautified". </summary>
    private struct SphereCheck
    {
        public Vector3 position;
        public float radius;
        public SphereCheck(Vector3 position, float radius) { this.position = position; this.radius = radius; }
    }
    /// <summary> The sphere used for ground checking. </summary>
    private SphereCheck groundCheck { get { return new SphereCheck(transform.TransformPoint(controller.center) - transform.up * (controller.height / 2), 0.4f); } }
    /// <summary> The sphere used for ceiling checking. </summary>
    private SphereCheck ceilingCheck { get { return new SphereCheck(transform.TransformPoint(controller.center) + transform.up * crouchHeight, 0.4f); } }
    /// <summary> A HashSet containing all the possible wall running directions. Easier than referencing them one by one, isn't it? </summary>
    private HashSet<Vector3> wallRunningDirectionalPossibilities = null;

    [Header("General")]
    /// <summary> Our wall running is pretty much a simple jump with lower gravity. Compared to the physics gravity, what scale is it? </summary>
    [SerializeField] private float gravityScaleWhileWallRunning = 0.4f;
    /// <summary> Our current gravity. </summary>
    private Vector3 currentGravity = Vector3.zero;
    /// <summary> The beautifully implemented artificial velocity. </summary>
    [HideInInspector] public Vector3 velocity = Vector3.zero;
    /// <summary> Do we have a ceiling above us? </summary>
    private bool ceilingAbove = false;
    /// <summary> For how longer should the ground checking be ignored? </summary>
    private float ignoreOnGroundTime = 0f;

    [Header("Multipliers")]
    /// <summary> The transition speed between multipliers. </summary>
    private float generalTransitionSpeed = 4.0f;
    /// <summary> The specific transition speed for sliding -> our general multiplier. </summary>
    private float slidingTransitionSpeed = 0.75f;
    /// <summary> Defines how quicker is the movement speed when sprinting compared to when simply walking. </summary>
    [SerializeField][Range(1, 5)] private float sprintingMultiplier = 2.0f;
    /// <summary> Defines how slower is the movement speed when crouching compared to when simply walking. </summary>
    [SerializeField][Range(0, 1)] private float crouchingMultiplier = 0.5f;
    private readonly float baseMultiplier = 1f;
    /// <summary> The current multiplier applied to the movement speed, with the transition speed being applied to it every frame. </summary>
    private float currentMultiplier = 1f;
    /// <summary> The multiplier the currentMultiplier is aiming for. </summary>
    private float targetMultiplier = 1f;

    [Header("Movement")]
    /// <summary> The movement speed when simply walking forward. </summary>
    [SerializeField][Range(0, 15)] private float forwardSpeed = 4.0f;
    /// <summary> The movement speed when walking in any other direction than forward. </summary>
    [SerializeField][Range(0, 15)] private float lateralSpeed = 2.0f;
    /// <summary> Are we sprinting? </summary>
    public bool IsSprinting { get; private set; }
    /// <summary> Are we just walking? </summary>
    public bool IsWalking { get { return !isSliding && !IsSprinting && OnGround && (Mathf.Abs(velocity.x) > 0.35f || Mathf.Abs(velocity.z) > 0.35f); } }
    /// <summary> Are we wall running? </summary>
    public bool IsWallRunning { get; private set; }
    /// <summary> Local wall running direction used for proper movement and calculations of wall sticking and running. </summary>
    private Vector3 wallRunningDirection = Vector3.zero;

    [Header("Jumping")]
    /// <summary> The height of the basic jump. </summary>
    [SerializeField][Range(0, 6)] public float jumpHeight = 0.9f;
    /// <summary> Are we currently touching the ground? </summary>
    public bool OnGround { get; private set; }
    /// <summary> For how much time have we been in the air? Equals zero if we are currently on the ground. Theoretically. </summary>
    private float timeInAir = 0f;
    /// <summary> Have we just fallen of a wall? Used mainly for triggering the wall fall animation. </summary>
    private bool wallFell = false;
    /// <summary> The direction of the wall we have just jumped from. Equals zero if none ever since the last time the ground was touched. </summary>
    private Vector3 previousWallJumpDirection = Vector3.zero;

    [Header("Crouching & sliding")]
    /// <summary> The height of the player when standing straight. </summary>
    [SerializeField] private float standUpHeight = 1.8f;
    /// <summary> The height of the player when crouching or sliding. </summary>
    [SerializeField] private float crouchHeight = 1f;
    /// <summary> The time the player has currently been sliding for. </summary>
    private float timeSliding = 0f;
    /// <summary> Are we currently crouching? </summary>
    public bool IsCrouching { get; private set; }
    /// <summary> Are we currently sliding? </summary>
    public bool isSliding { get; private set; }
    #endregion

    #region Methods
    private void Awake()
    {
        // Setting up the components
        controller = GetComponent<CharacterController>();
        controller.height = standUpHeight;

        // Setting up the general variables
        currentGravity = Physics.gravity;

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
        currentGravity = Physics.gravity;
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

            previousWallJumpDirection = Vector3.zero;
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
            if ((!IsWallRunning && OnGround) || previousWallJumpDirection != Vector3.zero)
            {
                // If we're sprinting, then let's see if we can wall run.
                if (IsSprinting)
                {
                    // Check if there is any direction in which we can wall run.
                    foreach (var wallDirection in wallRunningDirectionalPossibilities)
                        if (wallDirection != previousWallJumpDirection && wallRunningDirection != wallDirection && CanWallRun(wallDirection))
                        {
                            // We found a direction in which we can and want to wall run, 
                            // so let's do it and break out of this (so we won't do wall run in multiple directions)!
                            wallRunningDirection = wallDirection;
                            previousWallJumpDirection = Vector3.zero;
                            velocity.y = Mathf.Sqrt(jumpHeight * 2 * -currentGravity.y);
                            ignoreOnGroundTime = .5f;
                            IsWallRunning = true;
                            break;
                        }
                }
                // If we're not sprinting and / or we are not wall running, we'll do a basic jump.
                if (!IsWallRunning && OnGround)
                {
                    velocity.y = Mathf.Sqrt(jumpHeight * 2 * -currentGravity.y);
                    headAnimator.SetTrigger("Jumped");
                }
            }
            // If we're wall running and we've left the ground for a while now, then let's wall jump!
            else if (IsWallRunning && ignoreOnGroundTime <= 0f)
            {
                // Jumping in the opposite direction.
                // The changes required for jumping on the X axis.
                if (wallRunningDirection == Vector3.left || wallRunningDirection == Vector3.right)
                {
                    velocity.x += -wallRunningDirection.x * sprintingMultiplier * forwardSpeed;
                }
                // The changes required for jumping on the Z axis.
                else
                {
                    StartCoroutine(headRotationFunc(Mathf.Pow(-1, UnityEngine.Random.Range(0, 100)) * 180));
                    velocity.z -= -wallRunningDirection.z * sprintingMultiplier * forwardSpeed;
                }
                // Now, we're ready for the proper wall jumping.
                previousWallJumpDirection = wallRunningDirection;
                StopWallRunning();
                velocity.y = Mathf.Sqrt(jumpHeight * 2 * -currentGravity.y);
                headAnimator.SetTrigger("WallJumped");
                ResetTimeInAir();
            }
        }

        // Make sure we stick to the wall if we're wall running
        if (IsWallRunning)
        {
            controller.Move(transform.TransformDirection(wallRunningDirection) * Time.deltaTime);
            currentGravity = Physics.gravity * gravityScaleWhileWallRunning;
            ignoreOnGroundTime -= Time.deltaTime;
            // Maybe we are no longer on the wall or we've reached the ground and need to stop wall running.
            if ((OnGround && ignoreOnGroundTime <= 0f) || !CanWallRun(wallRunningDirection))
            {
                StopWallRunning();
                wallFell = true;
            }
        }

        // Check if we should be sprinting now or not.
        if ((Controls.instance.AutoSprinting || Controls.GetAction(UserAction.Sprint)) && !IsCrouching && velocity.z >= forwardSpeed * 0.9f)
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
        velocity.y += currentGravity.y * Time.deltaTime;

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
        // Wall running
        str += "Is " + (IsWallRunning ? "" : "not ") + "wall running " + DirectionToString(wallRunningDirection);
        str += previousWallJumpDirection != Vector3.zero ? ("\t|\t Last wall jump was from the " + DirectionToString(previousWallJumpDirection)) : "";
        str += "\t|\t";
        // Status
        List<string> currentStatuses = new List<string>();
        if (isSliding) currentStatuses.Add("sliding");
        if (IsCrouching) currentStatuses.Add("crouching");
        if (IsWalking) currentStatuses.Add("walking");
        if (IsSprinting) currentStatuses.Add("sprinting");
        if (IsWallRunning) currentStatuses.Add("wall running");
        if (OnGround) currentStatuses.Add("on ground");
        else currentStatuses.Add("in air for " + timeInAir + " seconds");
        currentStatuses.Add("current gravity: " + currentGravity);
        str += "Current status: ";
        currentStatuses.ForEach(e => str += e + ", ");
        str = str.Remove(str.Length - 2, 2);

        print(str);
        #endregion
    }

    string DirectionToString(Vector3 direction)
    {
        return direction == Vector3.left ? "left" : (direction == Vector3.forward ? "front" : (direction == Vector3.right ? "right" : (direction == Vector3.zero ? "" : "Unknown")));
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(groundCheck.position, groundCheck.radius);
        Gizmos.DrawSphere(ceilingCheck.position, ceilingCheck.radius);
    }
    #endregion
}
