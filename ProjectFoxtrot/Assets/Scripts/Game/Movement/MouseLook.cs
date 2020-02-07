using UnityEngine;

public class MouseLook : MonoBehaviour 
{
    [Header("Components")]
    [SerializeField] private Transform playerTransform = null;
    [SerializeField] private PlayerMovement playerMovement = null;

    [Header("Mouse")]
    [SerializeField] private float mouseSensitivity = 100f;

    [Header("Restrictions")]
    [SerializeField] [Range(0f, 90f)] private float xAngleRotationLimit = 90f;
    [SerializeField] [Range(0f, 180f)] private float yAngleRotationLimit = 30f;
    private float xRotation = 0f;
    private float yRotation = 0f;
    
	
	void Update ()
    {
        Vector2 mouse = new Vector2(Controls.GetAxis(InputAxis.MouseX), Controls.GetAxis(InputAxis.MouseY)) * mouseSensitivity * Time.deltaTime;

        // Calculate the vertical rotation (around the X axis) of the camera
        xRotation -= mouse.y; // += will flip the rotation // Could be used for inverted look settings 
        xRotation = Mathf.Clamp(xRotation, -xAngleRotationLimit, xAngleRotationLimit); // Will prevent the player from over-rotating and looking behind himself
        

        if(playerMovement.OnGround)
        {
            // Rotates the player if on the ground.
            playerTransform.Rotate(Vector3.up * mouse.x);

            // Rotates the player if just finished falling and rotated the camera.
            playerTransform.Rotate(Vector3.up, yRotation);
            yRotation = 0f;
        }
        else
        {
            // Calculates the horizontal rotation (around the Y axis) of the camera if in the air.
            yRotation += mouse.x;
            yRotation = Mathf.Clamp(yRotation, -yAngleRotationLimit, yAngleRotationLimit);
        }

        // Applies rotation to the camera.
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
}
