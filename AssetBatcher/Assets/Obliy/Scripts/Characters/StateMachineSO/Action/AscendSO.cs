using UnityEngine;
using Obliy.StateMachine;
using Obliy.StateMachine.ScriptableObjects;

[CreateAssetMenu(fileName = "Ascend", menuName = "Obliy State Machines/Actions/Ascend")]
public class AscendSO : StateActionSO<Ascend>
{
	public float initialJumpForce = 6f;
}

public class Ascend : StateAction
{
	private MainPlayer _playerScript;

	private float _verticalMovement;
	private float _gravityContributionMultiplier;
	private const float GRAVITY_COMEBACK_MULTIPLIER = .03f;
	private const float GRAVITY_DIVIDER = .6f;
	private const float GRAVITY_MULTIPLIER = 5f;
	private AscendSO _originSO => (AscendSO)base.OriginSO;

	public override void Awake(StateMachine stateMachine)
	{
		_playerScript = stateMachine.GetComponent<MainPlayer>();
	}

	public override void OnStateEnter()
	{
		_verticalMovement = _originSO.initialJumpForce;
	}

	public override void OnUpdate()
	{
		_gravityContributionMultiplier += GRAVITY_COMEBACK_MULTIPLIER;
		_gravityContributionMultiplier *= GRAVITY_DIVIDER; //Reduce the gravity effect
		_verticalMovement += Physics.gravity.y * GRAVITY_MULTIPLIER * Time.deltaTime * _gravityContributionMultiplier;

		_playerScript.movementVector.y = _verticalMovement;
	}
}
