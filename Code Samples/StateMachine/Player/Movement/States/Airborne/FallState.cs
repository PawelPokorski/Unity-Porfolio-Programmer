using System.Collections;
using UnityEngine;

namespace StateMachine.Player.Movement.Airborne
{
    public class FallState : State<PlayerMovement>
    {
        public FallState(PlayerMovement context) : base(context) { }

        public override void Enter()
        {
            Context.ControllerHeight = 1.55f;
            Context.HasControl = true;
            InitializeSubState();
        }

        public override void Execute()
        {
            HandleGravity();
        }

        public override void Exit()
        {
            Context.Animator.CrossFade("Jump Land", 0.15f);
            Context.StartCoroutine(Land());
        }

        public override void CheckSwitchState() { }

        private void HandleGravity()
        {
            float lastVerticalVelocity = Context.CurrentVerticalVelocity;
            Context.CurrentVerticalVelocity += Context.Gravity * Context.FallMultiplier * Time.deltaTime;
            Context.AppliedVerticalVelocity = Mathf.Max((Context.CurrentVerticalVelocity + lastVerticalVelocity) * 0.5f, -20f);
        }

        private IEnumerator Land()
        {
            Context.HasControl = false;
            Context.TargetSpeed = 0f;
            yield return new WaitForSeconds(Context.LandTime);
            Context.HasControl = true;
        }
    }
}