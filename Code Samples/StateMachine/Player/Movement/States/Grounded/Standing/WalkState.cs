using UnityEngine;

namespace StateMachine.Player.Movement.Grounded.Standing
{
    public class WalkState : State<PlayerMovement>
    {
        public WalkState(PlayerMovement context) : base(context)
        {
        }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            if (Context.MoveInput.y < 0)
            {
                Context.TargetSpeed = Context.WalkSpeed * 0.5f;
            }
            else if (Context.MoveInput.y == 0)
            {
                Context.TargetSpeed = Context.WalkSpeed * 0.75f;
            }
            else
            {
                Context.TargetSpeed = Context.WalkSpeed;
            }
        }

        public override void Exit()
        {
        }

        public override void CheckSwitchState()
        {
            if (!Context.IsMoving)
            {
                SwitchState(new IdleState(Context));
            }
            else if (Context.IsRunning)
            {
                SwitchState(new RunState(Context));
            }
        }
    }
}