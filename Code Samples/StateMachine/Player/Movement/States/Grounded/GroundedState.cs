using StateMachine.Player.Movement.Grounded;
using UnityEngine;

namespace StateMachine.Player.Movement
{
    public class GroundedState : State<PlayerMovement>
    {
        public GroundedState(PlayerMovement context) : base(context)
        {
            IsRootState = true;
        }

        public override void Enter()
        {
            InitializeSubState();
        }

        public override void Execute()
        {
            HandleGravity();
        }

        public override void Exit()
        {

        }

        public override void CheckSwitchState()
        {
            if (!Context.IsGrounded)
            {
                SwitchState(new AirborneState(Context));
            }
        }

        public override void InitializeSubState()
        {
            if (!Context.IsSliding)
            {
                if (!Context.IsCrouching)
                {
                    SetSubState(new StandingState(Context));
                }
                else
                {
                    SetSubState(new CrouchingState(Context));
                }
            }
            else
            {
                SetSubState(new SlidingState(Context));
            }

            base.InitializeSubState();
        }

        private void HandleGravity()
        {
            Context.CurrentVerticalVelocity = Context.GroundedGravity;
            Context.AppliedVerticalVelocity = Context.GroundedGravity;
        }
    }
}