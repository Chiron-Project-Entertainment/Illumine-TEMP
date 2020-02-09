///////////////////////////////////////////////////////////////
///                                                         ///
///             Script coded by Hakohn (Robert).            ///
///                                                         ///
///////////////////////////////////////////////////////////////

using System.Collections;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform playerTransform = null;
    [SerializeField] private PlayerMovement playerMovement = null;

    [Header("Mouse")]
    [SerializeField] private int mouseSensitivity = 100;

    [Header("Restrictions")]
    [SerializeField] [Range(0, 90)] private int xAngleRotationLimit = 80;
    [SerializeField] [Range(0, 180)] private int yAngleRotationLimit = 30;
    private float xRotation = 0f;
    private float yRotation = 0f;


    private bool manualRotationInProgress = false;

    public IEnumerator RotateHorizontal(float degrees)
    {
        manualRotationInProgress = true;
        float speed = 6f;

        yield return new WaitUntil(() =>
        {
            yRotation = Mathf.Lerp(yRotation, degrees, speed * Time.deltaTime);
            return Mathf.Abs(yRotation - degrees) <= speed;
        });
        playerTransform.Rotate(Vector3.up, yRotation);
        yRotation = 0f;
        manualRotationInProgress = false;
        yield return new WaitForEndOfFrame();
    }

    void Update()
    {
        Vector2 mouse = new Vector2(Controls.GetAxis(InputAxis.MouseX), Controls.GetAxis(InputAxis.MouseY)) * mouseSensitivity * Time.deltaTime;

        // Calculate the vertical rotation (around the X axis) of the camera
        xRotation -= mouse.y; // += will flip the rotation // Could be used for inverted look settings 
        xRotation = Mathf.Clamp(xRotation, -xAngleRotationLimit, xAngleRotationLimit); // Will prevent the player from over-rotating and looking behind himself


        
        // The camera movement while on ground, which allows freedom in any direction.
        if (playerMovement.OnGround)
        {
            // Rotates the player if on the ground.
            playerTransform.Rotate(Vector3.up * mouse.x);


            // Rotates the player if just finished falling and rotated the camera.
            playerTransform.Rotate(Vector3.up, yRotation);
            yRotation = 0f;
        }
        // The  camera movement while falling or in the air, which restricts the horizontal movement.
        else if(!manualRotationInProgress)
        {
            // Calculates the horizontal rotation (around the Y axis) of the camera if in the air.
            yRotation += mouse.x;
            yRotation = Mathf.Clamp(yRotation, -yAngleRotationLimit, yAngleRotationLimit);
        }

        // Applies rotation to the camera.
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
}
