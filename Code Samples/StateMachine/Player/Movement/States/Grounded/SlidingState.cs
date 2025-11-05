using StateMachine.Player.Movement.Grounded.Sliding;

namespace StateMachine.Player.Movement.Grounded
{
    public class SlidingState : State<PlayerMovement>
    {
        public SlidingState(PlayerMovement context) : base(context)
        {
        }

        public override void Enter()
        {
            Context.ControllerHeight = 0.7f;
            InitializeSubState();
        }

        public override void Execute()
        {

        }

        public override void Exit()
        {
        }

        public override void CheckSwitchState()
        {
            if (!Context.IsSliding)
            {
                if (!Context.IsCrouching)
                {
                    SwitchState(new StandingState(Context));
                }
                else
                {
                    SwitchState(new CrouchingState(Context));
                }
            }
        }

        public override void InitializeSubState()
        {
            SetSubState(new IdleState(Context));
        }
    }
}