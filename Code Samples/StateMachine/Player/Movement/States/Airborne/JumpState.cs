using UnityEngine;
namespace StateMachine.Player.Movement.Airborne
{
    public class JumpState : State<PlayerMovement>
    {
        public JumpState(PlayerMovement context) : base(context)
        {
        }

        public override void Enter()
        {
            Context.TimeSinceLastGrounded = 0f;
            Context.ControllerHeight = 1.65f;
            Context.Animator.CrossFade("Jump Start", 0.05f);
            HandleJump();
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
            if (Context.IsFalling)
            {
                SwitchState(new FallState(Context));
            }
        }

        private void HandleJump()
        {
            float jumpVelocity = Mathf.Sqrt(-2f * Context.Gravity * Context.JumpHeight);

            Context.CurrentVerticalVelocity = jumpVelocity;
            Context.AppliedVerticalVelocity = jumpVelocity;
        }

        private void HandleGravity()
        {
            float lastVerticalVelocity = Context.CurrentVerticalVelocity;

            Context.CurrentVerticalVelocity += Context.Gravity * Time.deltaTime;
            Context.AppliedVerticalVelocity = (lastVerticalVelocity + Context.CurrentVerticalVelocity) * 0.5f;
        }
    }
}