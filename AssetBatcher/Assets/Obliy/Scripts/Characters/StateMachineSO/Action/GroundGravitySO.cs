using UnityEngine;
using Obliy.StateMachine;
using Obliy.StateMachine.ScriptableObjects;

[CreateAssetMenu(fileName = "GroundGravity", menuName = "Obliy State Machines/Actions/Ground Gravity")]
public class GroundGravitySO : StateActionSO<GroundGravity>
{
	public float verticalPull = -5f;
}

public class GroundGravity : StateAction
{
	private MainPlayer _playerScript;
	private GroundGravitySO _originSO => (GroundGravitySO)base.OriginSO;
	public override void Awake(StateMachine stateMachine)
	{
		_playerScript = stateMachine.GetComponent<MainPlayer>();
	}

	public override void OnUpdate()
	{
		_playerScript.movementVector.y = _originSO.verticalPull;
	}
}
