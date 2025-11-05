using UnityEngine;

namespace StateMachine.Player.Movement.Grounded.Standing
{
    public class RunState : State<PlayerMovement>
    {
        public RunState(PlayerMovement context) : base(context)
        {
        }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            Context.TargetSpeed = Context.RunSpeed;
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
            else if (!Context.IsRunning)
            {
                SwitchState(new WalkState(Context));
            }
        }
    }
}