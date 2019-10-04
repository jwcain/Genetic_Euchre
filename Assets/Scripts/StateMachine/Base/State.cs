using UnityEngine;
using System.Collections; 

namespace StateMachineSystem {

    /// <summary>
    /// An abstract class for representing a state within a state machine.
    /// </summary>
    public abstract class State : IStateUpdate {

        public StateMachine owner;

        public virtual void IStateUpdate() {
            //Do nothing by default
        }
        
        public virtual void IStateLateUpdate() {
            //Do nothing by default
        }
        
        public virtual void IStateFixedUpdate() {
            //Do nothing by default
        }

        /// <summary>
        /// Code to execute when entering this state
        /// </summary>
        public virtual IEnumerator Enter() {
            AddListeners();
            yield return null;
        }
        
        /// <summary>
        /// Code to execute when exiting this state
        /// </summary>
        public virtual IEnumerator Exit() {
            RemoveListeners();
            yield return null;
        }

		/// <summary>
		/// Undoes the actions of this state
		/// </summary>
        public void Reset() {
            Debug.Log("Reset called");
        }

        /// <summary>
        /// Add event listeners that are relevant to this state 
        /// </summary>
        protected virtual void AddListeners() {
			//Do nothing by default
        }
        
        /// <summary>
        /// Remove event listeners relevant to this state
        /// </summary>
        protected virtual void RemoveListeners() {
			//Do nothing by default
        }
    }
}