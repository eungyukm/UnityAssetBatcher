using UnityEngine;
using Obliy.StateMachine;
using Obliy.StateMachine.ScriptableObjects;

[CreateAssetMenu(fileName = "Descend", menuName = "Obliy State Machines/Actions/Descend")]
public class DescendSO : StateActionSO<Descend>
{

}

public class Descend : StateAction
{
	private MainPlayer _playerScript;

	private float _verticalMovement;
	private const float GRAVITY_MULTIPLIER = 5f;
	private const float MAX_FALL_SPEED = -50f;
	private const float MAX_RISE_SPEED = 100f;

	public override void Awake(StateMachine stateMachine)
	{
		_playerScript = stateMachine.GetComponent<MainPlayer>();
	}

	public override void OnStateEnter()
	{
		_verticalMovement = _playerScript.movementVector.y;

		_playerScript.jumpInput = false;
	}

	public override void OnUpdate()
	{
		_verticalMovement += Physics.gravity.y * GRAVITY_MULTIPLIER * Time.deltaTime;

		_verticalMovement = Mathf.Clamp(_verticalMovement, MAX_FALL_SPEED, MAX_RISE_SPEED);

		_playerScript.movementVector.y = _verticalMovement;
	}
}
