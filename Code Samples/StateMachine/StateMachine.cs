using UnityEngine;

namespace StateMachine
{
    public class StateMachine<T> : MonoBehaviour where T : StateMachine<T>
    {
        public State<T> CurrentState { get; set; }

        public void InitializeStateMachine(State<T> initialState)
        {
            CurrentState = initialState;
            initialState.Enter();
        }

        public void UpdateStateMachine()
        {
            CurrentState?.Update();
            CurrentState?.UpdateSwitchCheck();
        }
    }
}