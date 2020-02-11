///////////////////////////////////////////////////////////////
///                                                         ///
///               Script coded by Veynam (Tom).             ///
///            Slightly tweaked by Hakohn (Robert).         ///
///                                                         ///
///////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interact : MonoBehaviour
{

    ///<summary> This script allows the player to: Pick up and cary around objects using Left Mouse Button, 
    ///as well as throw currently held object by clicking Right Mouse Button. </summary>

    Vector3 objectPos;

    public Transform player;

    public Transform playerCam;

    public float throwForce = 10f;

    bool canHold = false;

    bool beingCarried = false;

    private bool touched = false; //checks if the player is carrying an object and if the player tuches a wall it will fall fown

    private GameObject propHeld;

    private Rigidbody propRb;

    public bool isThrowing;

    void Start()
    {
        canHold = true;
    }

    void Update()
    {
        RaycastHit hitInfo;
        var rayCollision = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, (Screen.height / 2)));

        if (Controls.GetActionUp(UserAction.Pickup))
        {
            canHold = true;
        }

        if (Physics.Raycast(rayCollision, out hitInfo, 2.5f))
        {
            var selectedProp = hitInfo.transform;
            if (canHold && selectedProp.CompareTag("Prop") && Controls.GetAction(UserAction.Pickup))
            {
                propHeld = hitInfo.transform.gameObject;
                propRb = propHeld.GetComponent<Rigidbody>();
                propRb.isKinematic = true;

                propHeld.transform.parent = playerCam;
                beingCarried = true;
            }
            else
            {
                objectPos = transform.position;
                propHeld = hitInfo.transform.gameObject;
                propRb = propHeld.GetComponent<Rigidbody>();
                propRb.isKinematic = false;

                propHeld.transform.parent = null;
                beingCarried = false;
            }
            if (beingCarried && Controls.GetActionDown(UserAction.Throw))
            {
                canHold = false;
                propRb.isKinematic = false;
                propHeld.transform.parent = null; //unparents the object the player is carrying from the camera.
                beingCarried = false;
                propRb.AddForce(playerCam.transform.forward * throwForce, ForceMode.Impulse); //takes the forward direction of theplayers position and instantly throws the box. 
                isThrowing = true;
            }
        }

        // TO DO: 1) Disallow the player to carry multiple boxes at once; - Check
        //        2) Reset the boxes velocity when it is being carried;
        //        3) Reset the boxes rotation to face the player when he starts carrying it
        //        4) Disallow the player to immidiatly grab the box after throwing it; Add a canCarry cooldown
    }
}
