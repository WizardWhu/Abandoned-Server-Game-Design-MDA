using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CCPlayer : MonoBehaviour
{

    [Header("MovementControls")]
    [SerializeField] private float walkSpeed = 5;
    [SerializeField] private float runSpeed = 9;
    [SerializeField] private float jumpHeight = 5;


    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float lookSensitivity;

    [Header("Physics")]
    private CharacterController cc;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float verticalVelocity;//Current Upward/downward speed
    [SerializeField] private float gravity;//Constant downward acceleration

    private bool isSprinting;
    private bool isJumping;
    private bool isInteracting;

    private float pitch; //Up and Down
    private float yaw; //Side to side


    [Header("Reticle Controls")]
    [SerializeField] private Color reticleColor;
    [SerializeField] private float interactDistance;
    [SerializeField] private Color interactableColor;


    //Interaction variables
    [SerializeField] private Image reticleImage;
    private bool interactPressed;

    //this is our event that the other scripts will be listening for
    private Interactable currentInteractable;




    [Header("Camera Control")]
    [SerializeField] private float lookAtTime;

    //Locking bools
    private bool cameraLocked = false;
    private bool movementLocked = false;
    private bool jumpLocked = false;
    private bool interactLocked = false;
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        //If we are actually hitting the key isJumping equals true
        if (context.performed) isJumping = true;
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        isSprinting = context.ReadValueAsButton();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.action.WasPerformedThisFrame())
        {
            interactPressed = true;
        }
        else
        {
            interactPressed = false;
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        cc = GetComponent<CharacterController>();

        //Optional Cursor locking
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;


    }

    // Update is called once per frame
    void Update()
    {
        if (!cameraLocked) HandleLook();
        if (!movementLocked) HandleMovement();
        if (!interactLocked)
        {
            CheckInteract();
            HandleInteract();
        }

    }

    void CheckInteract()
    {
        //reset reticle image to normal color first
        if (reticleImage != null) reticleImage.color = reticleColor;
        //make a ray that goes straight out of the camera(center of screen)
        //players eyesight
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        //RaycastHit hit;
        //asking unity if it hit something within 3 units
        //hit stores what we hit like the collider
        //bool didHit = Physics.Raycast(ray, out hit, interactDistance);

        //if (!didHit) return;//if we didn't hit anything start here
        //if we hit something tagged interactable
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            Debug.Log("Pressed");

            currentInteractable = hit.collider.GetComponentInParent<Interactable>();
            if (currentInteractable != null && reticleImage != null)
            {
                reticleImage.color = interactableColor;
                Debug.DrawRay(cameraTransform.position, cameraTransform.forward * 3, Color.red);

            }
            else
            {
                Debug.DrawRay(cameraTransform.position, cameraTransform.forward * 3, Color.blue);
            }
        }

    }

    void HandleInteract()
    {
        //if the player did not press interact this frame do nothing
        if (!interactPressed) return;
        //consume the input so one click only triggers one interactions
        //this changes next frame
        interactPressed = false;

        if (currentInteractable == null) return;

        currentInteractable.Interact(this);
        currentInteractable = null;

    }

    private void HandleLook()
    {
        //Horizontal mouse movement rotates player
        float yaw = lookInput.x * lookSensitivity;
        //Vertical mouse movement rotates camera
        float pitchDelta = lookInput.y * lookSensitivity;
        transform.Rotate(Vector3.up * yaw);

        //accumulate vertical rotation
        pitch -= pitchDelta;
        pitch = Mathf.Clamp(pitch, -90, 90);

        cameraTransform.localRotation = Quaternion.Euler(pitch, 0, 0);
    }
    private void HandleMovement()
    {
        //Updating our bool to be true or false if the player is grounded
        bool grounded = cc.isGrounded;

        //this keeps the cc snapped to the ground
        if (grounded && verticalVelocity <= 0)
        {
            verticalVelocity = -2f;
        }

        float currentSpeed = walkSpeed;

        if (isSprinting)
        {
            currentSpeed = runSpeed;
        }

        Vector3 move = transform.right * moveInput.x * currentSpeed + transform.forward * moveInput.y * currentSpeed;

        if (!jumpLocked)
        {
            if (isJumping && grounded)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            else
            {
                isJumping = false;
            }
        }


        //apply gravity to every frame
        verticalVelocity += gravity * Time.deltaTime;

        //Convert vertical velocity into movement Vector
        Vector3 velocity = Vector3.up * verticalVelocity;

        //Now we are FINALLY MOVING OUR PLAYER
        cc.Move((move + velocity) * Time.deltaTime);
    }


    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Debug.Log("CC collided with:" + hit.gameObject.name);
    }

    IEnumerator LookAt(Transform target)
    {

        Quaternion FinalRotation = Quaternion.LookRotation(target.position - transform.position);
        Quaternion StartRotation = cameraTransform.rotation;
        float counter = 0;
        while (true)
        {
            cameraTransform.rotation = Quaternion.Lerp(StartRotation, FinalRotation, counter);

            counter += 1f / (lookAtTime * 60f);
            yield return new WaitForFixedUpdate();
            if (counter >= 1f) break;
        }
    }

    public void LockCameraRotation() { cameraLocked = true; }
    public void UnlockCameraRotation() { cameraLocked = false; }


    public void LockMovement() { movementLocked = true; }
    public void UnlockMovement() { movementLocked = false; }


    public void LockInteraction() { interactLocked = true; }
    public void UnlockInteraction() { interactLocked = false; }


    public void LockJump() { jumpLocked = true; }
    public void UnlockJump() { jumpLocked = false; }
}
