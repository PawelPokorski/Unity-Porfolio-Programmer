using UnityEngine;

namespace StateMachine.Player.Movement.Grounded.Standing
{
    public class IdleState : State<PlayerMovement>
    {
        public IdleState(PlayerMovement context) : base(context) { }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            Context.TargetSpeed = 0f;
        }

        public override void Exit()
        {
        }

        public override void CheckSwitchState()
        {
            if (Context.IsMoving)
            {
                if (Context.IsRunning)
                {
                    SwitchState(new RunState(Context));
                }
                else
                {
                    SwitchState(new WalkState(Context));
                }
            }
        }
    }
}