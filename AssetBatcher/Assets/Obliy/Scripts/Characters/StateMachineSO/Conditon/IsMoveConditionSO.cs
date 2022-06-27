using UnityEngine;
using Obliy.StateMachine;
using Obliy.StateMachine.ScriptableObjects;

[CreateAssetMenu(fileName = "IsMoveCondition", menuName = "Obliy State Machines/Conditions/Is Move Condition")]
public class IsMoveConditionSO : StateConditionSO<IsMoveCondition>
{
	public float treshold = 0.02f;
}

public class IsMoveCondition : Condition
{
	private MainPlayer _playerScript;
	private IsMoveConditionSO _originSO => (IsMoveConditionSO)base.OriginSO;
	public override void Awake(StateMachine stateMachine)
	{
		_playerScript = stateMachine.GetComponent<MainPlayer>();
	}

	protected override bool Statement()
	{
		Vector3 movementVector = _playerScript.movementInput;
		movementVector.y = 0f;
		return movementVector.sqrMagnitude > _originSO.treshold;
	}

	public override void OnStateExit()
	{
		_playerScript.movementVector = Vector3.zero;
	}
}
