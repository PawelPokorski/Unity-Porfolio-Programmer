namespace StateMachine
{
    public abstract class State<T> where T : StateMachine<T>
    {
        private State<T> _currentSubState;
        private State<T> _currentSuperState;

        protected bool IsRootState { get; set; }
        protected T Context { get; }

        protected State(T context)
        {
            Context = context;
        }

        public abstract void Enter();
        public abstract void Execute();
        public abstract void Exit();
        public abstract void CheckSwitchState();

        public virtual void InitializeSubState()
        {
            _currentSubState?.Enter();
        }

        public void Update()
        {
            Execute();
            _currentSubState?.Update();
        }

        public void UpdateSwitchCheck()
        {
            CheckSwitchState();
            _currentSubState?.UpdateSwitchCheck();
        }

        public void SwitchState(State<T> newState)
        {
            Exit();
            newState.Enter();

            if (IsRootState)
            {
                Context.CurrentState = newState;
                _currentSubState?.Exit();
            }
            else
            {
                _currentSuperState?.SetSubState(newState);
            }

            newState.SetSuperState(_currentSuperState);
        }

        protected void SetSuperState(State<T> superState)
        {
            _currentSuperState = superState;
        }

        protected void SetSubState(State<T> subState)
        {
            _currentSubState = subState;
            subState.SetSuperState(this);
        }
    }
}