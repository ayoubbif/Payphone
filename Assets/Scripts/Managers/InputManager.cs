using KKL.Player;
using KKL.Utils;
using UnityEngine;

namespace KKL.Managers
{
    public class InputManager : Singleton<InputManager>
    {
        
        private FirstPersonController _controller;
        private PlayerInput _playerInput;
        private PlayerInput.PlayerActions _playerActions;
        
        protected override void Awake()
        {
            InitializeComponents();
            SetupInputActions();
        }

        private void OnEnable()
        {
            _playerActions.Enable();
        }

        private void FixedUpdate()
        {
            _controller.HandleMovement(_playerActions.Move.ReadValue<Vector2>());
        }
        
        private void LateUpdate()
        {
            _controller.HandleCamera(_playerActions.Look.ReadValue<Vector2>());
        }

        private void OnDisable()
        {
            _playerActions.Disable();
        }
        
        private void InitializeComponents()
        {
            _playerInput = new PlayerInput();
            _playerActions = _playerInput.Player;
            if (!_controller)  _controller = GetComponent<FirstPersonController>();
        }
        
        private void SetupInputActions()
        {
            _playerActions.Enable();
            
            // Jump
            _playerActions.Jump.performed += _ =>
            {
                _controller.HandleJump();
            };
            
            // Sprint
            _playerActions.Sprint.performed += _ =>
            {
                _controller.StartSprinting();
            };
            
            
            // Crouch
            _playerActions.Crouch.performed += _ =>
            {
                _controller.HandleCrouch();
            };
        }
    }
}