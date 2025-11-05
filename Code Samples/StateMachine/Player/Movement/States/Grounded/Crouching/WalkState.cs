using UnityEngine;

namespace StateMachine.Player.Movement.Grounded.Crouching
{
    public class WalkState : State<PlayerMovement>
    {
        public WalkState(PlayerMovement context) : base(context) { }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            Context.TargetSpeed = Context.CrouchSpeed;
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
        }
    }
}