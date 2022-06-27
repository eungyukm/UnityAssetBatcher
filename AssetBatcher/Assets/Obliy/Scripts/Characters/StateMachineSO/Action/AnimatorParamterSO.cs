using UnityEngine;
using Obliy.StateMachine;
using Obliy.StateMachine.ScriptableObjects;
using Moment = Obliy.StateMachine.StateAction.SpecificMoment;

[CreateAssetMenu(fileName = "AnimatorParamter", menuName = "Obliy State Machines/Actions/Animator Paramter")]
public class AnimatorParamterSO : StateActionSO
{
	public ParameterType parameterType = default;
	public string parameterName = default;

	public bool boolValue = default;
	public int intValue = default;
	public float floatValue = default;

	public Moment whenToRun = default;

	protected override StateAction CreateAction() => new AnimatorParamter(Animator.StringToHash(parameterName));

	public enum ParameterType
	{
		Bool, Int, Float, Trigger
	}
}

public class AnimatorParamter : StateAction
{
	private Animator _animator;
	private AnimatorParamterSO _originSO => (AnimatorParamterSO)base.OriginSO; // The SO this StateAction spawned from
	private int _parameterHash;

	public AnimatorParamter(int parameterHash)
	{
		_parameterHash = parameterHash;
	}

	public override void Awake(StateMachine stateMachine)
	{
		_animator = stateMachine.GetComponent<Animator>();
	}

	public override void OnStateEnter()
	{
		if (_originSO.whenToRun == SpecificMoment.OnStateEnter)
			SetParameter();
	}

	public override void OnStateExit()
	{
		if (_originSO.whenToRun == SpecificMoment.OnStateExit)
			SetParameter();
	}

	private void SetParameter()
	{
		switch (_originSO.parameterType)
		{
			case AnimatorParamterSO.ParameterType.Bool:
				_animator.SetBool(_parameterHash, _originSO.boolValue);
				break;
			case AnimatorParamterSO.ParameterType.Int:
				_animator.SetInteger(_parameterHash, _originSO.intValue);
				break;
			case AnimatorParamterSO.ParameterType.Float:
				_animator.SetFloat(_parameterHash, _originSO.floatValue);
				break;
			case AnimatorParamterSO.ParameterType.Trigger:
				_animator.SetTrigger(_parameterHash);
				break;
		}
	}

	public override void OnUpdate() { }
}
