using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour {

    // Varaibles

    public float mouseSensitivity = 100f;

    public Transform playerBody; // Reference from camera to the player body so it can be rotated // DON'T FORGET TO LINK THE PLAYER BODY!

    float xRotation = 0f;
	
	void Update ()
    {
        float mouseX = Controls.GetAxis(InputAxis.MouseX) * mouseSensitivity * Time.deltaTime; // Gather input and multiply it by mouse sensitivity
        float mouseY = Controls.GetAxis(InputAxis.MouseY) * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY; // += will flip the rotation // Could be used for inverted look settings 
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Will prevent the player from overrotating and looking behind himself

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f); // Unity uses Quaternion to handle rotations
        playerBody.Rotate(Vector3.up * mouseX);

        
    }
}
