using StateMachine.Player.Movement.Airborne;
using UnityEngine;

namespace StateMachine.Player.Movement
{
    public class AirborneState : State<PlayerMovement>
    {
        public AirborneState(PlayerMovement context) : base(context)
        {
            IsRootState = true;
        }

        public override void Enter()
        {
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
            if (Context.IsGrounded)
            {
                SwitchState(new GroundedState(Context));
            }
        }

        public override void InitializeSubState()
        {
            if (Context.IsFalling)
            {
                SetSubState(new FallState(Context));
            }
            else if (Context.IsJumping)
            {
                SetSubState(new JumpState(Context));
            }

            base.InitializeSubState();
        }
    }
}