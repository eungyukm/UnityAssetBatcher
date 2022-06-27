using Obliy.StateMachine.ScriptableObjects;

namespace Obliy.StateMachine
{
	/// <summary>
	/// State Machine의 Condition을 나타냅니다.
	/// </summary>
	public abstract class Condition : IStateComponent
	{
		private bool _isCached = false;
		private bool _cachedStatement = default;
		internal StateConditionSO _originSO;
		protected StateConditionSO OriginSO => _originSO;

		protected abstract bool Statement();

		internal bool GetStatement()
		{
			if (!_originSO.cacheResult)
				return Statement();

			if (!_isCached)
			{
				_isCached = true;
				_cachedStatement = Statement();
			}

			return _cachedStatement;
		}

		internal void ClearStatementCache()
		{
			_isCached = false;
		}
		public virtual void Awake(StateMachine stateMachine) { }
		public virtual void OnStateEnter() { }
		public virtual void OnStateExit() { }
	}

	public readonly struct StateCondition
	{
		internal readonly StateMachine _stateMachine;
		internal readonly Condition _condition;
		internal readonly bool _expectedResult;

		public StateCondition(StateMachine stateMachine, Condition condition, bool expectedResult)
		{
			_stateMachine = stateMachine;
			_condition = condition;
			_expectedResult = expectedResult;
		}

		public bool IsMet()
		{
			bool statement = _condition.GetStatement();
			bool isMet = statement == _expectedResult;

#if UNITY_EDITOR
			_stateMachine._debugger.TransitionConditionResult(_condition._originSO.name, statement, isMet);
#endif
			return isMet;
		}
	}
}
