using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControllerr : MonoBehaviour
{
    [Header("Player")]
    public float MoveSpeed = 4.0f;
    public float SprintSpeed = 6.0f;
    public float RotationSeed = 1.0f;
    public float SpeedChangeRate= 10.0f;

    [Space(10)]
    public float JumpHeight = 1.2f;
    public float Gravity = -15.0f;

    [Space(10)]
    public float JumpTimeout = 0.1f;
    public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    public bool Grounded = true;
    public float GroundedOffset = -0.14f;
    public float GroundedRadius = 0.5f;
    public LayerMask GroundLayers;

    [Header("Cinemachine")]
    public GameObject CinemachineCameraTarget;
    public float TopClamp = 90.0f;
    public float BottonClamp = -90.0f;

    private bool jump_Input;
    private Vector2 move_Input;
    private Vector2 look_Input;

    //cineachine
    private float _cinemachineTargetPitch;

    //player
    private float _speed;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    //timmeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    private PlayerInput _playerInput;
    private CharacterController _controller;

    //private StarterAssetsInputs _input;

    private GameObject _mainCamera;

    private const float _threshold = 0.01f;

    private bool IsCurrentDeviceMouse
    {
        get { return _playerInput.currentControlScheme == "KeyboardMouse"; }
    }

    void Awake()
    {
        if (_mainCamera == null)
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        
    }

    // Start is called before the first frame update
    void Start()
    {
        _controller = GetComponent<CharacterController>();
        //_input = GetComponent<StarterAssetsInputs>();

        _playerInput = GetComponent<PlayerInput>();

        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;

        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        JumpAndGravity();
        GroundedCheck();
        Move();

    }

    void LateUpdate()
    {
        CameraRotation();
    }

    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

    }

    private void CameraRotation()
    {
        look_Input = _playerInput.actions["Look"].ReadValue<Vector2>();

        if (look_Input.sqrMagnitude >= _threshold)
        {
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetPitch += look_Input.y * RotationSeed * deltaTimeMultiplier;
            _rotationVelocity = look_Input.x * RotationSeed * deltaTimeMultiplier;

            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottonClamp, TopClamp);

            CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

            transform.Rotate(Vector3.up * _rotationVelocity);
        }
    }

    private void Move()
    {
        move_Input = _playerInput.actions["Move"].ReadValue<Vector2>();

        //float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
        float targetSpeed = MoveSpeed;

        if (move_Input == Vector2.zero) targetSpeed = 0.0f;

        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        // float inputMagnitude = _input.analogMovement ? _input.movr.magnitude : 1f;
        float inputMagnitude = 1f;

        if(currentHorizontalSpeed <targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed, Time.deltaTime * SpeedChangeRate);

            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        Vector3 inputDirection = new Vector3(move_Input.x, 0.0f, move_Input.y).normalized;

        if(move_Input != Vector2.zero)
            inputDirection = transform.right * move_Input.x + transform.forward * move_Input.y;

        _controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            _fallTimeoutDelta = FallTimeout;

            if(_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            if(jump_Input && _jumpTimeoutDelta <= 0.0f)
            {
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
            }

            if(_jumpTimeoutDelta <= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            _jumpTimeoutDelta = JumpTimeout;

            if(_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }

            jump_Input = false;
        }

        if(_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    public void Jump(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
            jump_Input = true;
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if(lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (Grounded) Gizmos.color = transparentGreen; 
        else Gizmos.color = transparentRed;

        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
    }
}
