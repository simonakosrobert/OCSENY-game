using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{   
    [SerializeField] private AudioSource footStepSound;
    [SerializeField] private AudioClip AsphaltfootStepSound;
    [SerializeField] private AudioClip GrassfootStepSound;
    [SerializeField] private AudioClip GravelfootStepSound;
    [SerializeField] private AudioSource tiredSoundEffect;
    [SerializeField] private GameObject Stamina;
    [SerializeField] private GameObject Battery;
    [SerializeField] private GameObject Flashlight;
    [SerializeField] private AudioSource FlashlightSound;
    [SerializeField] private Slider StaminaBar;
    [SerializeField] private Slider BatteryBar;
    [SerializeField] private Image StaminaBarFill;
    public Camera playerCamera;
    public Vector3 playerCameraOrigPos;
    public float walkSpeed = 6f;
    private float walkSpeedOrig;
    public float runSpeed = 12f;
    private float speedBuffer = 0f;
    public float jumpPower = 7f;
    public float crouchLength = 1f;
    public float crouchTime;
    public float crouchTick = 0;
    public float gravity = 10f;

    private string terrainType = "Asphalt";

    public float lookSpeed = 2f;
    public float lookXLimit = 45f;

    private float tick;
    private float sprintTick;
    private bool isTired = false;
    private bool canRun = true;
    private byte StaminaBarColorChange = 255;

    private float currentBattery = 100;
    private float batteryTick = 0f;

    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    public bool canMove = true;

    CharacterController characterController;
    private float characterControllerOrigH;

    public static bool circuitBreakerOn = true;

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
        characterControllerOrigH = characterController.height;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Stamina.SetActive(true);
        Battery.SetActive(true);
        Flashlight.SetActive(false);
        playerCameraOrigPos = playerCamera.transform.position;
        walkSpeedOrig = walkSpeed;
    }

    void Update()
    {

    //RenderSettings.skybox.SetFloat("_Rotation", Time.time * 1f);

    if(Physics.Raycast(transform.position, transform.forward, out var hit, 2f))
    {
        var obj = hit.collider.gameObject;

        if (obj.name == "UtilityPole Main Street Coop" && Input.GetKeyDown(KeyCode.E))
        {
            var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "Utility Pole Spot Light");

            foreach (var _object in objects)
            {   
                if (_object.GetComponent<Light>().isActiveAndEnabled) 
                {
                    circuitBreakerOn = false;
                    _object.GetComponent<Light>().enabled = false;
                }
                else 
                {
                    _object.GetComponent<Light>().enabled = true;
                    circuitBreakerOn = true;
                }
            }
        }
    }

    if (Input.GetKeyUp(KeyCode.Mouse0) && Flashlight.activeSelf == false)
    {
        if (currentBattery > 0)
        {
            Flashlight.SetActive(true);
        }
        AudioSource flashLightSoundClone = Instantiate(FlashlightSound);
        flashLightSoundClone.transform.SetParent(transform);
        flashLightSoundClone.Play();
        Destroy(flashLightSoundClone.gameObject, flashLightSoundClone.clip.length);
    }
    else if (Input.GetKeyUp(KeyCode.Mouse0) && Flashlight.activeSelf == true)
    {   
        if (currentBattery > 0)
        {
            Flashlight.SetActive(false);
        }
        AudioSource flashLightSoundClone = Instantiate(FlashlightSound);
        flashLightSoundClone.transform.SetParent(transform);
        flashLightSoundClone.Play();
        Destroy(flashLightSoundClone.gameObject, flashLightSoundClone.clip.length);
    }

    if (currentBattery > 0 && Flashlight.activeSelf == true)
    {   
        batteryTick += Time.deltaTime;
        if (batteryTick >= 10f)
        {
            currentBattery -= 1;
            batteryTick = 0;
            BatteryBar.value = currentBattery / 100f;
        }
    }
    else if (currentBattery == 0)
    {
        Flashlight.SetActive(false);
    }

    if (currentBattery < 10)
    {
        Flashlight.GetComponent<Light>().intensity = 0.2f;
    }
    else if (Flashlight.GetComponent<Light>().intensity != 0.5f)
    {
        Flashlight.GetComponent<Light>().intensity = 0.5f;
    }

    #region Handles Movment
    Vector3 forward = transform.TransformDirection(Vector3.forward);
    Vector3 right = transform.TransformDirection(Vector3.right);

    // Press Left Shift to run
    
    bool isRunning = Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.S);

    if (StaminaBar.value == 0f && !isTired)
    {
        isTired = true;
        tiredSoundEffect.Play();
    }
    else if (StaminaBar.value >= 0.2f)
    {
        isTired = false;
    }

    if (isRunning && StaminaBar.value != 0f && !isTired && canRun)
    {
        StaminaBar.value -= 0.002f;
        sprintTick = 0f;
        StaminaBarColorChange = (byte)Mathf.Floor(StaminaBar.value * 255f);
        StaminaBarFill.color = new Color32(255, StaminaBarColorChange, StaminaBarColorChange, 255);
    }

    sprintTick += Time.deltaTime;

    if (sprintTick >= 2f)
    {
        StaminaBar.value += 0.001f;
        StaminaBarColorChange = (byte)Mathf.Floor(StaminaBar.value * 255f);
        StaminaBarFill.color = new Color32(255, StaminaBarColorChange, StaminaBarColorChange, 255);
    }

    if (isRunning && speedBuffer < runSpeed && StaminaBar.value > 0f && !isTired && canRun)
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
    

    if (tick > runSpeed / speedBuffer / 3 && speedBuffer > 0 && characterController.isGrounded)
    {   
        
        AudioSource footStepSoundClone = Instantiate(footStepSound);
        footStepSoundClone.transform.SetParent(transform);
        float randomPitch = Random.Range(0.9f, 1.1f);
        footStepSoundClone.pitch = randomPitch;
        if (canRun)
        {
            footStepSoundClone.volume = 1.0f;
        }
        else
        {
            footStepSoundClone.volume = 0.25f;
        }
        footStepSoundClone.Play();
        Destroy(footStepSoundClone.gameObject, footStepSoundClone.clip.length);
        tick = 0;
    }

    #endregion

    #region Crouching

    if (Input.GetKeyDown(KeyCode.LeftControl))
    {
        crouchTick = 0;
    }
    else if (Input.GetKeyUp(KeyCode.LeftControl))
    {
        crouchTick = 0;
    }

    if (Input.GetKey(KeyCode.LeftControl) && canMove && characterController.height > characterControllerOrigH / 2)
    {   
        crouchTick += crouchTime * Time.deltaTime;
        characterController.height = Mathf.Lerp(characterController.height, characterControllerOrigH / 2, crouchTick);
        walkSpeed = Mathf.Lerp(walkSpeed, walkSpeedOrig / 2, crouchTime);
        canRun = false;
    }
    else if (!Input.GetKey(KeyCode.LeftControl) && characterController.height < characterControllerOrigH)
    {
        crouchTick += crouchTime * Time.deltaTime;
        characterController.height = Mathf.Lerp(characterController.height, characterControllerOrigH, crouchTick);
        walkSpeed = Mathf.Lerp(walkSpeed, walkSpeedOrig, crouchTime);
        canRun = true;
    }

    #endregion

    #region Handles Jumping
    if (Input.GetButton("Jump") && canMove && characterController.isGrounded && !isTired)
    {
    moveDirection.y = jumpPower;

    StaminaBar.value -= 0.05f;
    sprintTick = 0f;
    StaminaBarColorChange = (byte)Mathf.Floor(StaminaBar.value * 255f);
    StaminaBarFill.color = new Color32(255, StaminaBarColorChange, StaminaBarColorChange, 255);
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

    if (Input.GetKeyUp(KeyCode.Escape))
    {
        Application.Quit();
    }
    }
}