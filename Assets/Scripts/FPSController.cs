using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{   
    [SerializeField] private AudioSource footStepSound;
    [SerializeField] private AudioClip AsphaltfootStepSound;
    [SerializeField] private AudioClip GrassfootStepSound;
    [SerializeField] private AudioClip GravelfootStepSound;
    public Camera playerCamera;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    private float speedBuffer = 0f;
    public float jumpPower = 7f;
    public float gravity = 10f;

    private string terrainType = "Asphalt";

    public float lookSpeed = 2f;
    public float lookXLimit = 45f;

    private float tick;


    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    public bool canMove = true;

    CharacterController characterController;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {   
        
        if (hit.gameObject.tag == "Grass")
        {
            terrainType = "Grass";
        }
        else if (hit.gameObject.tag == "Gravel")
        {
            terrainType = "Gravel";
        }
        else
        {
            terrainType = "Asphalt";
        }
    }

    void Awake()
    {
        footStepSound.playOnAwake = false;
    }
    void Start()
    {   
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
    #region Handles Movment
    Vector3 forward = transform.TransformDirection(Vector3.forward);
    Vector3 right = transform.TransformDirection(Vector3.right);

    // Press Left Shift to run
    bool isRunning = Input.GetKey(KeyCode.LeftShift);

    if (isRunning && speedBuffer < runSpeed)
    {
        speedBuffer += 0.10f;
    }
    else if ((Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0) && speedBuffer < walkSpeed)
    {
        speedBuffer += 0.10f;
    }
    else if (speedBuffer > 0)
    {
        speedBuffer -= 0.30f;
        if (speedBuffer < 0)
        {
            speedBuffer = 0;
        }
    }

    float curSpeedX = canMove ? speedBuffer * Input.GetAxis("Vertical") : 0;
    float curSpeedY = canMove ? speedBuffer * Input.GetAxis("Horizontal") : 0;
    float movementDirectionY = moveDirection.y;
    moveDirection = (forward * curSpeedX) + (right * curSpeedY);

    if (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0)
    {
        tick += Time.deltaTime;
    }

    switch(terrainType) 
    {
        case "Grass":
            footStepSound.clip = GrassfootStepSound;
            break;
        case "Asphalt":
            footStepSound.clip = AsphaltfootStepSound;
            break;
        case "Gravel":
            footStepSound.clip = GravelfootStepSound;
            break;
        default:
            footStepSound.clip = AsphaltfootStepSound;
            break;
    }
    
    if (tick > runSpeed / speedBuffer / 3 && speedBuffer > 0)
    {
        AudioSource footStepSoundClone = Instantiate(footStepSound);
        footStepSoundClone.transform.SetParent(transform);
        float randomPitch = Random.Range(0.9f, 1.1f);
        footStepSoundClone.pitch = randomPitch;
        footStepSoundClone.Play();
        Destroy(footStepSoundClone.gameObject, footStepSoundClone.clip.length);
        tick = 0;
    }

    #endregion

    #region Handles Jumping
    if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
    {
    moveDirection.y = jumpPower;
    }
    else
    {
    moveDirection.y = movementDirectionY;
    }

    if (!characterController.isGrounded)
    {
    moveDirection.y -= gravity * Time.deltaTime;
    }

    #endregion

    #region Handles Rotation
    characterController.Move(moveDirection * Time.deltaTime);

    if (canMove)
    {
    rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
    rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
    playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
    transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
    }

    #endregion
    }
}