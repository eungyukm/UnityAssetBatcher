using UnityEngine;
using Obliy.StateMachine;
using Obliy.StateMachine.ScriptableObjects;

[CreateAssetMenu(fileName = "ApplyMovement", menuName = "Obliy State Machines/Actions/Apply Movement")]
public class ApplyMovementSO : StateActionSO<ApplyMovement>
{
}

public class ApplyMovement : StateAction
{
	private MainPlayer _playerScript;
	private CharacterController _characterController;
	public override void Awake(StateMachine stateMachine)
	{
		_playerScript = stateMachine.GetComponent<MainPlayer>();
		_characterController = stateMachine.GetComponent<CharacterController>();
	}

	public override void OnUpdate()
	{
		_characterController.Move(_playerScript.movementVector* Time.deltaTime);
	}
}
