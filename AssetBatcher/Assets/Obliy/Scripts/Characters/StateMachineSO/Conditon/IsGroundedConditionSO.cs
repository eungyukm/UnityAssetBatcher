using UnityEngine;
using Obliy.StateMachine;
using Obliy.StateMachine.ScriptableObjects;

[CreateAssetMenu(fileName = "IsGroundedCondition", menuName = "Obliy State Machines/Conditions/Is Grounded Condition")]
public class IsGroundedConditionSO : StateConditionSO<IsGroundedCondition>
{
	public float groundCheckDistance = 0.1f;
	public LayerMask fieldLayer;
}

public class IsGroundedCondition : Condition
{
	private CharacterController _characterController;
	private IsGroundedConditionSO _originSO => (IsGroundedConditionSO)base.OriginSO;
	public override void Awake(StateMachine stateMachine)
	{
		_characterController = stateMachine.GetComponent<CharacterController>();
	}

	protected override bool Statement()
	{
		// if (_characterController.isGrounded)
		// {
		// 	return true;
		// }

		// 발사하는 광선의 초기 위치와 방향
		// 약간 신체에 박혀 있는 위치로부터 발사하지 않으면 제대로 판정할 수 없을 때가 있다.
		var ray = new Ray(_characterController.transform.position + Vector3.up * 0.1f, Vector3.down);
		// 탐색 거리
		var maxDistance = 0.3f;
		// 광선 디버그 용도
		Debug.DrawRay(_characterController.transform.position + Vector3.up * 0.1f, Vector3.down * maxDistance, Color.red);
		// Raycast의 hit 여부로 판정
		// 지상에만 충돌로 레이어를 지정
		// return Physics.Raycast(ray, maxDistance, _fieldLayer);
		return Physics.Raycast(ray, _originSO.groundCheckDistance);
	}
}
