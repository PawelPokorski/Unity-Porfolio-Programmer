using UnityEngine;

namespace StateMachine.Player.Movement.Grounded.Sliding
{
    public class IdleState : State<PlayerMovement>
    {
        public IdleState(PlayerMovement context) : base(context) { }

        public override void Enter()
        {
        }

        public override void Execute()
        {

        }

        public override void Exit()
        {

        }

        public override void CheckSwitchState()
        {

        }
    }
}