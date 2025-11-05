using StateMachine.Player.Movement.Grounded.Standing;

namespace StateMachine.Player.Movement.Grounded
{
    public class StandingState : State<PlayerMovement>
    {
        public StandingState(PlayerMovement context) : base(context)
        {
        }

        public override void Enter()
        {
            Context.ControllerHeight = 1.8f;
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
            if (!Context.IsSliding && Context.IsCrouching)
            {
                SwitchState(new CrouchingState(Context));
            }
            else if (Context.IsSliding)
            {
                SwitchState(new SlidingState(Context));
            }
            //else if (Context.IsDodging)
            //{
            //    SwitchState(new DodgingState(Context));
            //}
        }

        public override void InitializeSubState()
        {
            if (Context.IsMoving)
            {
                if (Context.IsRunning)
                {
                    SetSubState(new RunState(Context));
                }
                else
                {
                    SetSubState(new WalkState(Context));
                }
            }
            else
            {
                SetSubState(new IdleState(Context));
            }

            base.InitializeSubState();
        }
    }
}