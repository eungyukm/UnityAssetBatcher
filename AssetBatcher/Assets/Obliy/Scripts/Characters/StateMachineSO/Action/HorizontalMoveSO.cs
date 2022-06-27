using UnityEngine;
using Obliy.StateMachine;
using Obliy.StateMachine.ScriptableObjects;

[CreateAssetMenu(fileName = "HorizontalMove", menuName = "Obliy State Machines/Actions/Horizontal Move")]
public class HorizontalMoveSO : StateActionSO<HorizontalMove>
{
	[Tooltip("Horizontal XZ plane speed multiplier")]
	public float speed = 8f;
}

public class HorizontalMove : StateAction
{
	private MainPlayer _playerScript;
	private HorizontalMoveSO _originSO => (HorizontalMoveSO)base.OriginSO;
	public override void Awake(StateMachine stateMachine)
	{
		_playerScript = stateMachine.GetComponent<MainPlayer>();
	}

	public override void OnUpdate()
	{
		_playerScript.movementVector.x = _playerScript.movementInput.x * _originSO.speed;
		_playerScript.movementVector.z = _playerScript.movementInput.z * _originSO.speed;
	}
}
