using System.Collections;
using UnityEngine;

namespace StateMachine.Player.Movement.Grounded
{
    public class DodgingState : State<PlayerMovement>
    {
        public DodgingState(PlayerMovement context) : base(context) { }

        private bool _isDodgeProcessed;

        public override void Enter()
        {
            Context.ControllerHeight = 1.8f;
            Context.StartCoroutine(ProcessDodge());
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
            if (_isDodgeProcessed)
            {
                SwitchState(new StandingState(Context));
            }
        }

        private IEnumerator ProcessDodge()
        {
            _isDodgeProcessed = false;

            if (Context.MoveInput.x < 0)
            {
                Context.Animator.CrossFade("Dodge Left", 0.05f);
            }
            else if (Mathf.Approximately(Context.MoveInput.x, 0))
            {
                Context.Animator.CrossFade("Dodge Backward", 0.15f);
            }
            else
            {
                Context.Animator.CrossFade("Dodge Right", 0.05f);
            }

            yield return new WaitForSeconds(Context.DodgeTime);
            _isDodgeProcessed = true;
        }
    }
}