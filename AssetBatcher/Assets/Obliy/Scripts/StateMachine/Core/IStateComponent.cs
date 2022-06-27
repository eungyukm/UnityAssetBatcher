namespace Obliy.StateMachine
{
	interface IStateComponent
	{
		/// <summary>
		/// State가 시작될 때
		/// </summary>
		void OnStateEnter();

		/// <summary>
		/// State가 끝날 때
		/// </summary>
		void OnStateExit();
	}
}
