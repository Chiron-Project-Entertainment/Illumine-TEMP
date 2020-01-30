using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    public CharacterController controller;

    public float speed = 12f;
    public float gravity = -9.81f;

    Vector3 velocity;

	// Update is called once per frame
	void Update () {

        float x = Controls.GetAxisRaw(InputAxis.Horizontal);
        float z = Controls.GetAxisRaw(InputAxis.Vertical);

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
	}
}
