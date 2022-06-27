using System;
using UnityEngine;
using UnityEngine.Serialization;

public class MainPlayer : MonoBehaviour
{
	[SerializeField] private GameInputReader inputReader = default;
	public Transform gameplayCamera;

	private Vector2 _previousMovementInput;

	[HideInInspector] public bool jumpInput;
	[HideInInspector] public Vector3 movementInput;
	[HideInInspector] public Vector3 movementVector;
	[HideInInspector] public ControllerColliderHit lastHit;

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		lastHit = hit;
	}

	private void OnEnable()
	{
		inputReader.jumpEvent += OnJumpInitiated;
		inputReader.jumpCanceledEvent += OnJumpCanceled;
		inputReader.moveEvent += OnMove;
	}

	private void OnDisable()
	{
		inputReader.jumpEvent -= OnJumpInitiated;
		inputReader.jumpCanceledEvent -= OnJumpCanceled;
		inputReader.moveEvent -= OnMove;
	}

	// Update is called once per frame
    void Update()
    {
	    RecalculateMovement();
    }

    private void RecalculateMovement()
    {
	    Vector3 cameraForward = gameplayCamera.forward;
	    cameraForward.y = 0f;
	    Vector3 cameraRight = gameplayCamera.right;
	    cameraRight.y = 0f;

	    Vector3 adjustedMovement = cameraRight.normalized * _previousMovementInput.x +
	                               cameraForward.normalized * _previousMovementInput.y;

	    movementInput = Vector3.ClampMagnitude(adjustedMovement, 1f);
    }

    private void OnMove(Vector2 movement)
    {
	    _previousMovementInput = movement;
    }

    private void OnJumpInitiated()
    {
	    jumpInput = true;
    }

    private void OnJumpCanceled()
    {
	    jumpInput = false;
    }
}
