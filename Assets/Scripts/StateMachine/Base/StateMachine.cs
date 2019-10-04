using UnityEngine;
using System.Collections;

namespace StateMachineSystem {
	/// <summary>
	/// Base state machine that allows for exlcusive operation of a single state with transitions
	/// </summary>
	public class StateMachine : IStateUpdate {

		public virtual void IStateUpdate() {
			if (CurrentState != null && _inTransition == false)
				CurrentState.IStateUpdate();
		}
		
		public virtual void IStateLateUpdate() {
			if (CurrentState != null && _inTransition == false)
				CurrentState.IStateLateUpdate();
		}
		
		public virtual void IStateFixedUpdate() {
			if (CurrentState != null && _inTransition == false)
				CurrentState.IStateFixedUpdate();
		}

		/// <summary>
		/// The current state of the machine
		/// </summary>
		/// <value></value>
		public virtual State CurrentState => _currentState;

        [SerializeField]
		protected State _currentState;
		protected volatile bool _inTransition;

		public void Transition<T>() where T : State, new() {
			GameManager.AccessInstance().StartCoroutine(TransitionTo<T>());
		}


		/// <summary>
		/// Transitions the state machine to a new state.
		/// </summary>
		/// <param name="value"></param>
		private IEnumerator TransitionTo<T>() where T : State, new() {
			//Yield while there is another tansition active
            while (_inTransition)
                yield return null;
			//Flag this as transitioning.
			_inTransition = true;
			//Debug.Log("Transition To:" + typeof(T).Name);

			//Exit the current state if it exists
			if (_currentState != null)
				yield return _currentState.Exit();
			
			//Create our new state
			_currentState = new T();

			_currentState.owner = this;
            
			//Enter it
            if (_currentState != null)
                yield return _currentState.Enter();

			//Mark us as not transitioning
			_inTransition = false;
		}

	}
}	