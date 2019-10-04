namespace StateMachineSystem {
    /// <summary>
    /// An interface for forwarding Unity Updates through a State Machine
    /// </summary>
    public interface IStateUpdate {

        /// <summary>
        /// Forwards Unity Update method through the state machine
        /// </summary>
        void IStateUpdate();
        
        /// <summary>
        /// Forwards Unity LateUpdate method through the state machine
        /// </summary>
        void IStateLateUpdate();
        
        /// <summary>
        /// Forwards Unity LateUpdate method through the state machine
        /// </summary>
        void IStateFixedUpdate();
        
    }
}