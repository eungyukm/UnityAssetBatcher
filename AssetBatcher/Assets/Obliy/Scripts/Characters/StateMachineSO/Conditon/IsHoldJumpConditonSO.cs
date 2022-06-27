using UnityEngine;
using Obliy.StateMachine;
using Obliy.StateMachine.ScriptableObjects;

[CreateAssetMenu(fileName = "IsIdleJumpConditon", menuName = "Obliy State Machines/Conditions/Is Idle Jump Conditon")]
public class IsHoldJumpConditonSO : StateConditionSO<IsHoldJumpConditon>
{

}

public class IsHoldJumpConditon : Condition
{
	private MainPlayer _playerScript;
	public override void Awake(StateMachine stateMachine)
	{
		_playerScript = stateMachine.GetComponent<MainPlayer>();
	}

	protected override bool Statement() => _playerScript.jumpInput;
}
