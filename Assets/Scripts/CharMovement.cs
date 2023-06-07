/* 
=   This file contains general player movement.
=       1. W A S D      Player Control
=       2. SHIFT        Sprint
=       3. C            Crouch
=       4. SHIFT + C    Slide
=       5. SHIFT + F    Dive
=   Additionally, Physics based Jumping Capabilities 
=   
*/

using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public class CharMovement : MonoBehaviour
{
    // General Variables ----
    [Header("General Vars:")]
    bool allowControl;
    public float defCharDrag;
    public int jumpMultiplier = 2, fallMultiplier;
    private int speed;
    public int defSpeed, sprintSpeed, crouchSpeed;
    public bool isGrounded, inSprint;
    private bool jumpPressed, inJump;
    private Rigidbody rigidBodyComponent;
    public Camera MainCamera;
    public GameObject GunObject;
    public GameObject ClamberText;
    public bool clamberFlag, mountedFlag;
    public Transform clamberedObject;
    public RaycastHit climableObject;
    
    [Header("Crouch Variables:")]
    public float crouchScaleY;
    public bool isCrouched, crouchClicked;

    // Slide Mechanics ----
    [Header("Slide Variables:")]
    public int slideForce;
    public float maxSlideTime;
    float slideTimer;
    Vector3 characterSize, characterRot, slideDirection;
    bool inSlide;
    public float slideScaleY;

    // Dive Mechanics ----
    [Header("Dive Variables:")]
    public bool isDiving;
    bool standing;
    public float maxDiveTimer, floorDrag;
    public int diveForceVert, diveForceHoriz;
    float diveTimer;
    Vector3 charRotDive, charSizeDive, diveDirection;

    public int JumpMultiplier
    {
        get { return jumpMultiplier; }
        set { jumpMultiplier = value; }
    }
    
    public int Speed
    {
        get { return speed; }
        set { speed = value; }
    }
    
    private void Start()
    {
        // Initial character properties (scale, angle, etc.)
        characterSize = gameObject.transform.localScale;
        characterRot = gameObject.transform.localRotation.eulerAngles;

        allowControl = true;
        speed = defSpeed;
        slideTimer = 3;
        MainCamera = Camera.main;
        rigidBodyComponent = GetComponent<Rigidbody>();

        // Initial character drag properties
        gameObject.GetComponent<Rigidbody>().drag = defCharDrag;
        gameObject.GetComponent<Rigidbody>().angularDrag = defCharDrag;
    }

    // Update is called once per frame
    void Update()
    {
        // Engages jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space key pressed!");
            jumpPressed = true;
        }

        // Engages sprint
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isCrouched)
        {
            Sprint();
        }

        // Stops sprint
        if (Input.GetKeyUp(KeyCode.LeftShift) && !isCrouched)
        {
            StopSprint();
        }

        // Handles crouch
        if (Input.GetKeyDown(KeyCode.C) && isGrounded && !inSprint && !inSlide && !isCrouched)
        {
            isCrouched = true;
            crouchClicked = true;
        } 

        // Stands up character from crouch position
        if (Input.GetKeyUp(KeyCode.C) && isGrounded && !inSprint && !inSlide && isCrouched)
        {
            isCrouched = false;
            StopCrouch();
        }

        // Engages slide
        if (Input.GetKeyDown(KeyCode.C) && isGrounded && inSprint)
        {
            StartSlide();
        }

        // Stops the slide
        if (Input.GetKeyUp(KeyCode.C) && inSprint)
            StopSlide();

        // Engages dive
        if (Input.GetKeyDown(KeyCode.F) && isGrounded && inSprint)
            StartDive();
    }

    private void FixedUpdate()
    {
        // Check if grounded
        IsGrounded();

        // Check for Wall mounting
        if (Physics.Raycast(transform.position, MainCamera.transform.forward, out climableObject, 1.5f))
        {
            if (climableObject.transform.gameObject.tag == "Mountable")
            {
                if (Mathf.Abs(climableObject.transform.position.y) + climableObject.transform.localScale.y > (Mathf.Abs(transform.position.y) + transform.localScale.y))
                {
                    clamberedObject = climableObject.transform.gameObject.transform;
                    PromptWallMount();

                }
            }
        }       
        else 
        {
            ClamberText.SetActive(false);
            clamberedObject = null;
            clamberFlag = false;
        }
       
        // Checks if player is on the ground to enable another jump
        if (jumpPressed && isGrounded && !clamberFlag)
        {
            rigidBodyComponent.AddForce(Vector3.up * jumpMultiplier, ForceMode.VelocityChange);
            // inJump = true;
            jumpPressed = false;
        } else if (jumpPressed && clamberFlag) 
        {
            
            WallMount();
            mountedFlag = false;
            jumpPressed = false;
        } else
            jumpPressed = false;

      

        // Checks if player is in slide/crouch/is diving/standing and engages corresponding mechanisms
        if (inSlide)
        {
            SlideMechanic();
        }

        if (isCrouched)
            StartCrouch();

        if (isDiving)
            DiveMechanic();

        if (standing)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, characterSize, .1f);
            if (transform.localScale == characterSize)
                standing = false;
        }


        // Performs Character movement in relation to camera angle
        if (allowControl)
            this.transform.position = transform.position + (Camera.main.transform.forward * Input.GetAxis("Vertical") * speed * Time.deltaTime)
                                                        + (Camera.main.transform.right * Input.GetAxis("Horizontal") * speed * Time.deltaTime);
    }

    public void IsGrounded() 
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, (transform.localScale.y + .1f));
    }

    private void Sprint()
    {
        speed = sprintSpeed;
        inSprint = true;
        GunObject.SetActive(false);
    }

    private void StopSprint()
    {
        speed = defSpeed;
        inSprint = false;
        GunObject.SetActive(true);
    }

    private void PromptWallMount()
    {
        ClamberText.SetActive(true);
        // Set flag to avoid jumping instead of clambering
        clamberFlag = true;
    }

    private void WallMount()
    {
        Debug.Log("Wall mounting!");
        gameObject.transform.position = new Vector3(clamberedObject.position.x, clamberedObject.position.y + gameObject.transform.localScale.y * 2, clamberedObject.position.z);     
    }

    private void StartCrouch()
    {
        isCrouched = true;
        speed = crouchSpeed;
        gameObject.transform.localScale = new Vector3 (transform.localScale.x, crouchScaleY, transform.localScale.z);
        gameObject.GetComponent<Rigidbody>().AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }

    private void StopCrouch()
    {
        Debug.Log("Stopping Crouch");
        speed = defSpeed;
        isCrouched = false;
        standing = true;
    }

    private void StartSlide()
    {
        // Get direction of slide
        inSlide = true;
        slideDirection = gameObject.transform.forward;

        // Shrink character
        gameObject.transform.localScale = new Vector3(transform.localScale.x, slideScaleY, transform.localScale.z);
        
        // Add force to character to engage slide
        gameObject.GetComponent<Rigidbody>().AddForce(Vector3.down * 5f, ForceMode.Impulse);

        // Set the slide timer
        slideTimer = maxSlideTime;
    }

    private void SlideMechanic()
    {
        // Add force in the direction of slide
        gameObject.GetComponent<Rigidbody>().AddForce(slideDirection.normalized * slideForce, ForceMode.Impulse);
        gameObject.transform.localScale = new Vector3(transform.localScale.x, slideScaleY, transform.localScale.z);
        
        // Start/decrement timer
        slideTimer -= Time.deltaTime;

        // Run end slide mechanism once timer hits 0
        if (slideTimer <= 0)
        {
            StopSlide();
        }
    }

    private void StopSlide()
    {
        inSlide = false;
        standing = true;
    }

    private void StartDive()
    {
        // Set the player in diving state
        isDiving = true;

        // Disable player control
        allowControl = false;

        // Get initial values
        charRotDive = MainCamera.transform.forward;
        diveDirection = transform.forward;
        charSizeDive = transform.localScale;
        diveTimer = maxDiveTimer;

        // Engage the dive
        gameObject.GetComponent<Rigidbody>().AddForce(transform.up * diveForceVert, ForceMode.Impulse);
        gameObject.GetComponent<Rigidbody>().AddForce(diveDirection.normalized * diveForceHoriz, ForceMode.Impulse);
    }

    private void DiveMechanic()
    {
        // Decrement dive timer
        diveTimer -= Time.deltaTime;

        // Shrink character
        transform.localScale = new Vector3(charSizeDive.x, charSizeDive.y * .5f, charSizeDive.z);

        // Stop the dive once we are grounded and the timer is up
        if (isGrounded && diveTimer <= 0)
        {
            isDiving = false;
            gameObject.GetComponent<Rigidbody>().drag = 0;
            StopDive();
        } 

    }

    private void StopDive()
    {
        standing = true;
        allowControl = true;
    }

    void OnCollisionEnter(Collision other) 
    {
        // Give the player drag force to slow the player down
        if (isDiving)
        {
            gameObject.GetComponent<Rigidbody>().drag = floorDrag;
            gameObject.GetComponent<Rigidbody>().angularDrag = floorDrag;
        }

    }

}
