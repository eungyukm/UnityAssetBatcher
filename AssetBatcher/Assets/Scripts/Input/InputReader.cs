using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Ping = UnityEngine.Ping;

[CreateAssetMenu(menuName = "SO/InputReader", fileName = "InputReader")]
public class InputReader : ScriptableObject, GameInput.IDeploymentActions, GameInput.IUnitCursorModeActions
{
    private GameInput _gameInput;

    public UnityAction OnDeploymentMode = delegate {  };
    public UnityAction<Vector2> OnMouseLeftClickedAction = delegate {  };
    public UnityAction<Vector2> OnMouseCursorClickAction = delegate {  };

    public Vector2 MousePos = new Vector2(0, 0);

    private InputMode _inputMode = InputMode.Deploy;

    private void OnEnable()
    {
        if (_gameInput == null)
        {
            _gameInput = new GameInput();
            _gameInput.Deployment.SetCallbacks(this);
            _gameInput.UnitCursorMode.SetCallbacks(this);
        }
        _gameInput.Enable();
    }
    
    private void OnDisable()
    {
        _gameInput.Disable();
    }
    
    public void ModeSwitch(InputMode inputMode)
    {
        Debug.Log("Mode : " + inputMode);
        switch (inputMode)
        {
            case InputMode.Deploy:
                DeActivateInput();
                OnDeploymentModeStart();
                break;
            case InputMode.UnitCursor:
                DeActivateInput();
                OnUnitCursorModeStart();
                break;
        }
    }

    private void DeActivateInput()
    {
        _gameInput.Deployment.Disable();
        _gameInput.UnitCursorMode.Disable();
    }
    
    public void OnDeploymentModeStart()
    {
        _gameInput.Deployment.Enable();
    }


    public void OnUnitCursorModeStart()
    {
        _gameInput.UnitCursorMode.Enable();
    }

    public void OnMouse(InputAction.CallbackContext context)
    {
        // Debug.Log("Call!!");
        if (context.phase == InputActionPhase.Performed)
        {
            Vector2 mousePoint = context.ReadValue<Vector2>();
            MousePos = mousePoint;
            // Debug.Log("movement x : " + MousePos.x);
        }
    }

    public void OnMouseLeftClick(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            // Debug.Log("[MLC] mouse x : " + MousePos.x + " mouse y : " + MousePos.y);
            OnMouseLeftClickedAction?.Invoke(MousePos);
        }
    }

    public Vector2 GetMousePos()
    {
        return MousePos;
    }

    public void OnMouseCursorLeftClick(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            OnMouseCursorClickAction?.Invoke(MousePos);
        }
    }

    public void OnMousPoint(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            Vector2 mousePoint = context.ReadValue<Vector2>();
            MousePos = mousePoint;
            // Debug.Log("movement x : " + MousePos.x);
        }
    }
}
public enum InputMode
{
    Deploy,
    UnitCursor,
}