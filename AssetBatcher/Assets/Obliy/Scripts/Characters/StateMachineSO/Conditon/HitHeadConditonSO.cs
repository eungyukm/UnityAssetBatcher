using UnityEngine;
using Obliy.StateMachine;
using Obliy.StateMachine.ScriptableObjects;

[CreateAssetMenu(fileName = "HitHeadConditon", menuName = "Obliy State Machines/Conditions/Hit Head Conditon")]
public class HitHeadConditonSO : StateConditionSO<HitHeadConditon>
{

}

public class HitHeadConditon : Condition
{
	private MainPlayer _playerScript;
	private CharacterController _characterController;
	private Transform _transform;

	public override void Awake(StateMachine stateMachine)
	{
		_transform = stateMachine.GetComponent<Transform>();
		_playerScript = stateMachine.GetComponent<MainPlayer>();
		_characterController = stateMachine.GetComponent<CharacterController>();
	}

	protected override bool Statement()
	{
		bool isMovingUpwards = _playerScript.movementVector.y > 0f;
		if (isMovingUpwards)
		{
			float permittedDistance = _characterController.radius / 2f;
			float topPositionY = _transform.position.y + _characterController.height;
			float distance = Mathf.Abs(_playerScript.lastHit.point.y - topPositionY);
			if (distance <= permittedDistance)
			{
				_playerScript.jumpInput = false;
				_playerScript.movementVector.y = 0f;

				return true;
			}
		}

		return false;
	}
}
