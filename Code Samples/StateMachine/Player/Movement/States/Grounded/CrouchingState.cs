using StateMachine.Player.Movement.Grounded.Crouching;

namespace StateMachine.Player.Movement.Grounded
{
    public class CrouchingState : State<PlayerMovement>
    {
        public CrouchingState(PlayerMovement context) : base(context)
        {
        }

        public override void Enter()
        {
            Context.ControllerHeight = 1.3f;
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
            if (!Context.IsCrouching || Context.IsRunning)
            {
                SwitchState(new StandingState(Context));
            }
            else if (Context.IsSliding)
            {
                SwitchState(new SlidingState(Context));
            }
        }

        public override void InitializeSubState()
        {
            if (Context.IsMoving)
            {
                SetSubState(new WalkState(Context));
            }
            else
            {
                SetSubState(new IdleState(Context));
            }

            base.InitializeSubState();
        }
    }
}