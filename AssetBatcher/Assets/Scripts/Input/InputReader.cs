using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "SO/InputReader", fileName = "InputReader")]
public class InputReader : ScriptableObject, GameInput.IUnitCursorModeActions
{
    private GameInput _gameInput;

    public UnityAction OnDeploymentMode = delegate {  };
    public UnityAction<Vector2> OnMouseLeftClickedAction = delegate {  };
    public UnityAction OnMouseLeftClickDownAction = delegate {  };
    public UnityAction OnMouseLeftClickUPAction = delegate {  };

    public UnityAction OnCtrlDownAction = delegate {  };
    public UnityAction OnCtrlUPAction = delegate {  };
    
    public Vector2 MousePos = new Vector2(0, 0);
    
    public UnityAction OnGrapPresseAction = delegate {  };
    public UnityAction OnScaleAction = delegate {  };
    public UnityAction OnRotationAction = delegate {  };
    
    public UnityAction OnXKeyAction = delegate {  };
    public UnityAction OnYKeyAction = delegate {  };
    public UnityAction OnZKeyAction = delegate {  };
    public UnityAction OnWKeyAction = delegate {  };
    
    public UnityAction<Vector2> OnMouseScrollAction = delegate(Vector2 arg0) {  };

    private InputMode _inputMode = InputMode.None;

    private void OnEnable()
    {
        if (_gameInput == null)
        {
            _gameInput = new GameInput();
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
            case InputMode.None:
                DeActivateInput();
                break;
            case InputMode.UnitCursor:
                DeActivateInput();
                OnUnitCursorModeStart();
                break;
        }
    }

    private void DeActivateInput()
    {
        _gameInput.UnitCursorMode.Disable();
    }


    public void OnUnitCursorModeStart()
    {
        _gameInput.UnitCursorMode.Enable();
    }

    public void OnMouse(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            Vector2 mousePoint = context.ReadValue<Vector2>();
            MousePos = mousePoint;
        }
    }

    public void OnMouseLeftClick(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            OnMouseLeftClickDownAction?.Invoke();
        }
        else if (context.phase == InputActionPhase.Performed)
        {
            OnMouseLeftClickedAction?.Invoke(MousePos);
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            OnMouseLeftClickUPAction?.Invoke();
        }
    }

    public void OnKeyboardCtrl(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            OnCtrlDownAction?.Invoke();
        }
        else if (context.phase == InputActionPhase.Performed)
        {
            
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            OnCtrlUPAction?.Invoke();
        }
    }

    public void OnGrap(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            OnGrapPresseAction?.Invoke();
        }
    }

    public void OnScale(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            OnScaleAction?.Invoke();
        }
    }

    public void OnRotation(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            OnRotationAction?.Invoke();
        }
    }

    public void OnKeyboardX(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            OnXKeyAction?.Invoke();
        }
    }

    public void OnKeyboardY(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            OnYKeyAction?.Invoke();
        }
    }

    public void OnKeyboardZ(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            OnZKeyAction?.Invoke();
        }
    }

    public void OnKeyboardW(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            OnWKeyAction?.Invoke();
        }
    }

    public void OnMouseScroll(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            Debug.Log("value : " + context.ReadValue<Vector2>());
            OnMouseScrollAction?.Invoke(context.ReadValue<Vector2>());
        }
    }

    public Vector2 GetMousePos()
    {
        return MousePos;
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
    None,
    UnitCursor,
}