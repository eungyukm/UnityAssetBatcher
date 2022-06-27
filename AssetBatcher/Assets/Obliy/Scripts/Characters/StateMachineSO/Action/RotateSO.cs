using UnityEngine;
using Obliy.StateMachine;
using Obliy.StateMachine.ScriptableObjects;

[CreateAssetMenu(fileName = "Rotate", menuName = "Obliy State Machines/Actions/Rotate")]
public class RotateSO : StateActionSO<Rotate>
{
	public float turnSmoothTime = 0.2f;
}

public class Rotate : StateAction
{
	private MainPlayer _playerScript;
	private Transform _transform;

	private float _turnSmoothSpeed;
	private const float ROTATION_TRESHOLD = .02f;

	private RotateSO _originSO => (RotateSO)base.OriginSO;

	public override void Awake(StateMachine stateMachine)
	{
		_playerScript = stateMachine.GetComponent<MainPlayer>();
		_transform = stateMachine.GetComponent<Transform>();
	}

	public override void OnUpdate()
	{
		Vector3 horizontalMovement = _playerScript.movementVector;
		horizontalMovement.y = 0f;

		if (horizontalMovement.sqrMagnitude >= ROTATION_TRESHOLD)
		{
			float targetRotation = Mathf.Atan2(_playerScript.movementVector.x, _playerScript.movementVector.z) * Mathf.Rad2Deg;
			_transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(
				_transform.eulerAngles.y,
				targetRotation,
				ref _turnSmoothSpeed,
				_originSO.turnSmoothTime);
		}
	}
}
