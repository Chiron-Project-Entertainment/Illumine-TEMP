using UnityEngine;

public class MouseLook : MonoBehaviour {

    public float mouseSensitivity = 100f;

    public Transform playerBody;

    float xRotation = 0f;
	// Use this for initialization
	void Start () {

        Cursor.lockState = CursorLockMode.Locked;

	}
	
	// Update is called once per frame
	void Update () {

        float mouseX = Controls.GetAxisRaw(InputAxis.MouseX) * mouseSensitivity * Time.deltaTime;
        float mouseY = Controls.GetAxisRaw(InputAxis.MouseY) * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
	}
}
