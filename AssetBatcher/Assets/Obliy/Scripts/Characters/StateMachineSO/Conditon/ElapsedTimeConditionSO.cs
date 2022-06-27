using UnityEngine;
using Obliy.StateMachine;
using Obliy.StateMachine.ScriptableObjects;

[CreateAssetMenu(fileName = "ElapsedTimeCondition", menuName = "Obliy State Machines/Conditions/Elapsed Time Condition")]
public class ElapsedTimeConditionSO : StateConditionSO<ElapsedTimeCondition>
{
	public float timerLength = .5f;
}

public class ElapsedTimeCondition : Condition
{
	private float _startTime;
	private ElapsedTimeConditionSO _originSO => (ElapsedTimeConditionSO)base.OriginSO; // The SO this Condition spawned from

	public override void OnStateEnter()
	{
		_startTime = Time.time;
	}

	protected override bool Statement() => Time.time >= _startTime + _originSO.timerLength;
}
