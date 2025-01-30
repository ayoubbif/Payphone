using System.Collections;
using UnityEngine;

namespace KKL.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        private Camera _playerCamera;
        private CharacterController _characterController;
    
        private Vector3 _moveDirection;
        private float _rotationX;
        
        public bool CanMove { get; set; } = true;
        public bool CanLook { get; set; } = true;
        
        public bool CanHeadBob { get; set; } = true;

        [Header("Functional Options")] 
        [SerializeField] private bool canJump = true;
        [SerializeField] private bool canSprint = true;
        [SerializeField] private bool canCrouch = true;
        [SerializeField] private bool canHeadBob = true;
    
        [Header("Movement Parameters")] 
        [SerializeField] private float walkSpeed = 3.0f;
        [SerializeField] private float sprintSpeed = 4.5f;
        [SerializeField] private float crouchSpeed = 1f;
    
        [Header("Look Parameters")] 
        [SerializeField, Range(1, 50)] private float xSensitivity = 15.0f;
        [SerializeField, Range(1, 50)] private float ySensitivity = 15.0f;
        [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f;
        [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f;
    
        [Header("Jump Parameters")] 
        [SerializeField] private float jumpForce = 8.0f;
        [SerializeField] private float gravity = 30.0f;
    
        [Header("Crouch Parameters")] 
        [SerializeField] private float crouchHeight = 0.5f;
        [SerializeField] private float standingHeight = 1.75f;
        [SerializeField] private float timeToCrouch = 0.25f;
        [SerializeField] private Vector3 crouchingCenter = new(0, 0.2f, 0);
        [SerializeField] private Vector3 standingCenter = new(0, 0, 0);
        private bool _isCrouching;
        private bool _duringCrouchingAnimation;
    
        [Header("HeadBob Parameters")]
        [SerializeField] private float walkBobSpeed = 10f;
        [SerializeField] private float walkBobAmount = 0.03f;
        [SerializeField] private float sprintBobSpeed = 15f;
        [SerializeField] private float sprintBobAmount = 0.07f;
        [SerializeField] private float crouchBobSpeed = 5f;
        [SerializeField] private float crouchBobAmount = 0.01f;
        
        [Header("Stairs Parameters")] 
        [SerializeField] private float stairsSpeed = 1.7f;
        
        private float _defaultYPos;
        private float _timer;

        private bool _isSprinting;
        private const string StairsTag = "Stairs";

        private void Awake()
        {
            _playerCamera = GetComponentInChildren<Camera>();
            _characterController = GetComponent<CharacterController>();
            
            _defaultYPos = _playerCamera.transform.localPosition.y;
        
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if(canHeadBob)
                HandleHeadBob();
            
        }

        public void HandleMovement(Vector2 input)
        {
            if (!CanMove)
                return;
            
            var moveDirectionY = _moveDirection.y;
            var speed = GetSpeedModifier();

            _moveDirection = transform.TransformDirection(Vector3.forward) * (input.y * speed) +
                             transform.TransformDirection(Vector3.right) * (input.x * speed);
            _moveDirection.y = moveDirectionY;

            if (!_characterController.isGrounded)
                _moveDirection.y -= gravity * Time.deltaTime;

            _characterController.Move(_moveDirection * Time.deltaTime);
        }

        public void HandleCamera(Vector2 input)
        {
            if (!CanLook)
                return;
            
            SetCameraPitch(input.x);
            SetCameraYaw(input.y);
        }
    
        public void HandleJump()
        {
            if (!CanMove)
                return;
            
            if (_characterController.isGrounded && canJump)
                _moveDirection.y = jumpForce;
        }

        public void StartSprinting()
        {
            if (!CanMove)
                return;
            
            _isSprinting = canSprint;
        }

        public void StopSprinting()
        {
            _isSprinting = false;
        }

        public void HandleCrouch()
        {
            if(!_duringCrouchingAnimation && _characterController.isGrounded && canCrouch)
                StartCoroutine(CrouchStand());
        }
    
        private float GetSpeedModifier()
        {
            if (_isCrouching) return crouchSpeed;
            if (IsOnStairs()) return stairsSpeed;
            
            return _isSprinting ? sprintSpeed : walkSpeed;
        }
    
        private void SetCameraPitch(float mouseX)
        {
            transform.Rotate(Vector3.up * (mouseX * Time.deltaTime * xSensitivity));
        }

        private void SetCameraYaw(float mouseY)
        {
            _rotationX -= mouseY * Time.deltaTime * ySensitivity;
            _rotationX = Mathf.Clamp(_rotationX, -upperLookLimit, lowerLookLimit);
            _playerCamera.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
        }

        private IEnumerator CrouchStand()
        {
            if (_isCrouching && Physics.Raycast(_playerCamera.transform.position, Vector3.up, 1f))
            {
                yield break;
            }

            _duringCrouchingAnimation = true;

            float timeElapsed = 0;
        
            var targetHeight = _isCrouching ? standingHeight : crouchHeight;
            var currentHeight = _characterController.height;

            var targetCenter = _isCrouching ? standingCenter : crouchingCenter;
            var currentCenter = _characterController.center;

            while (timeElapsed < timeToCrouch)
            {
                _characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
                _characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
        
            _characterController.height = targetHeight;
            _characterController.center = targetCenter;

            _isCrouching = !_isCrouching;
        
            _duringCrouchingAnimation = false;
        }
    
        private void HandleHeadBob()
        {
            // Only allow head bobbing when not on stairs
            if(IsOnStairs()) return;
            
            if (!_characterController.isGrounded) return;

            if (!(Mathf.Abs(_moveDirection.x) > 0.1f) && !(Mathf.Abs(_moveDirection.z) > 0.1f)) return;

            if (!CanHeadBob) return;
            
            _timer += Time.deltaTime * (_isCrouching ? crouchBobSpeed : _isSprinting ? sprintBobSpeed : walkBobSpeed);
            _playerCamera.transform.localPosition = new Vector3(
                _playerCamera.transform.localPosition.x,
                _defaultYPos + Mathf.Sin(_timer) * (_isCrouching ? crouchBobAmount : _isSprinting ? sprintBobAmount : walkBobAmount),
                _playerCamera.transform.localPosition.z);
        }

        private bool IsOnStairs() => 
            Physics.Raycast(_playerCamera.transform.position,
                Vector3.down,
                out var hit,
                3) &&
            hit.collider.CompareTag(StairsTag);
    }
}