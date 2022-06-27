using Obliy.StateMachine.ScriptableObjects;

namespace Obliy.StateMachine
{
	/// <summary>
	/// 오브젝트의 Action을 나타냅니다.
	/// </summary>
	public abstract class StateAction : IStateComponent
	{
		internal StateActionSO _originSO;
		protected StateActionSO OriginSO => _originSO;

		/// <summary>
		/// Sate Machine에서 매 프레임 호출됩니다.
		/// </summary>
		public abstract void OnUpdate();

		/// <summary>
		/// 인스턴스를 만들 때 호출됩니다.
		/// </summary>
		public virtual void Awake(StateMachine stateMachine) { }

		public virtual void OnStateEnter() { }
		public virtual void OnStateExit() { }

		/// <summary>
		/// State의 Enter Exit Update를 나타냅니다.
		/// </summary>
		public enum SpecificMoment
		{
			OnStateEnter, OnStateExit, OnUpdate,
		}
	}
}
