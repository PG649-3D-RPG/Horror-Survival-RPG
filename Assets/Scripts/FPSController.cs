using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputState))]
public class FPSController : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 4.0f;

    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 6.0f;

    [Tooltip("Rotation speed of the character")]
    public float RotationSpeed = 1.0f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    [Space(10)]
    [Tooltip("The height the player can jump")]
    public float JumpHeight = 1.2f;

    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.1f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Tooltip("How often the player can get hit before dying.")]
    public int Health = 3;

    [Tooltip("How long after a hit until the player can take damage again")]
    public float PostHitInvincibility = 3.0f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;

    [Tooltip("Useful for rough ground")] public float GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.5f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;

    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 90.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -90.0f;

    public List<GameObject> EquipmentPrefabs;
    private List<GameObject> Inventory = new();
    public GameObject EquipmentRoot;
    
    private IEquipment Equipment;
    // cinemachine
    private float _cinemachineTargetPitch;

    // player
    private float _speed;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;


    private PlayerInput _playerInput;
    private CharacterController _controller;
    private PlayerInputState _input;
    private GameObject _mainCamera;

    private const float Threshold = 0.01f;

    private bool IsCurrentDeviceMouse => _playerInput.currentControlScheme == "KeyboardMouse";
    
    // Invincibility
    private float _invincibilityTimeLeft = 0.0f;
    private bool _died = false;

    private void Awake()
    {
        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<PlayerInputState>();
        _playerInput = GetComponent<PlayerInput>();

        // reset our timeouts on start
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;

        foreach (var prefab in EquipmentPrefabs) {
            var equipment = Instantiate(prefab, EquipmentRoot.transform, false);
            equipment.SetActive(false);
            Inventory.Add(equipment);
        }

        if (Inventory.Count > 0)
        {
            Equip(Inventory[0]);
        }

        GameObject.Find("PlayerCapsule").GetComponent<PlayerCollision>().Controller = this;
    }

    private void Update()
    {
        JumpAndGravity();
        GroundedCheck();
        Move();
        UseEquipment();
        UpdateInvincibility();
        CheckForDeath();
    }
    
    private void LateUpdate()
    {
        CameraRotation();
    }

    // Called whenever the player collides with a GameObject with an collider and a bone
    public void OnDamage()
    {
        if (_invincibilityTimeLeft <= float.Epsilon)
        {
            _invincibilityTimeLeft = PostHitInvincibility;
            Health--;
            Debug.Log("Player Hit");
        }
    }

    private void UpdateInvincibility()
    {
        _invincibilityTimeLeft = Mathf.Max(0.0f, _invincibilityTimeLeft - Time.deltaTime);
    }

    private void CheckForDeath()
    {
        if (Health <= 0 && !_died)
        {
            _died = true;
            Debug.Log("Game ended");

            SceneTransition.ToGameOver();
        }
    }

    private void Equip(GameObject equipment)
    {
        var e = equipment.GetComponent<IEquipment>();
        if (e == null) return;
       
        e.OnEquip();
        Equipment?.OnUnequip();
        Equipment = e;
        equipment.transform.position = EquipmentRoot.transform.position;
    }

    private void UseEquipment()
    {
        if (_input.PrimaryAction)
        {
            Equipment.OnPrimary();
        }
        if (_input.SecondaryAction)
        {
            Equipment.OnSecondary();
        }

        if (Inventory.Count > 0 && Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            Equip(Inventory[0]);
        }
        
        if (Inventory.Count > 1 && Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            Equip(Inventory[1]);
        }
        
        // I have not found a way to make Unity do this automatically.
        // The Jump input is reset manually in the Jump and Gravity code as well
        _input.PrimaryAction = false;
        _input.SecondaryAction = false;
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        var position = transform.position;
        var spherePosition = new Vector3(position.x, position.y - GroundedOffset,
            position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
    }

    private void CameraRotation()
    {
        // if there is an input
        if (!(_input.Look.sqrMagnitude >= Threshold)) return;
        //Don't multiply mouse input by Time.deltaTime
        var deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

        _cinemachineTargetPitch += _input.Look.y * RotationSpeed * deltaTimeMultiplier;
        _rotationVelocity = _input.Look.x * RotationSpeed * deltaTimeMultiplier;

        // clamp our pitch rotation
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Update Cinemachine camera target pitch
        CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

        // rotate the player left and right
        transform.Rotate(Vector3.up * _rotationVelocity);
    }

    private void Move()
    {
        // set target speed based on move speed, sprint speed and if sprint is pressed
        var targetSpeed = _input.Sprint ? SprintSpeed : MoveSpeed;

        // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

        // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is no input, set the target speed to 0
        if (_input.Move == Vector2.zero) targetSpeed = 0.0f;

        // a reference to the players current horizontal velocity
        var currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        var speedOffset = 0.1f;
        var inputMagnitude = _input.AnalogMovement ? _input.Move.magnitude : 1f;

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

            // round speed to 3 decimal places
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        // normalise input direction
        var inputDirection = new Vector3(_input.Move.x, 0.0f, _input.Move.y).normalized;

        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is a move input rotate player when the player is moving
        if (_input.Move != Vector2.zero)
        {
            // move
            inputDirection = transform.right * _input.Move.x + transform.forward * _input.Move.y;
        }

        // move the player
        _controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) +
                         new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            if (_input.Jump && _jumpTimeoutDelta <= 0.0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
            }

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }

            // if we are not grounded, do not jump
            _input.Jump = false;
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnDrawGizmosSelected()
    {
        var transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        var transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        Gizmos.color = Grounded ? transparentGreen : transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
            GroundedRadius);
    }
}