using UnityEngine;
using System.Collections;

namespace JamTools
{
    public class JumpTimingAbilities : CharacterAbilitiesBehaviour
    {
        public CharacterAbilitiesBehaviour puppetAbilities;

        public float notGroundedJumpWindow = 0.2f;

        [Header("Exposed")]
        public float sinceGrounded = 1;
        public float sinceAirJump = 1;
        public bool jumpInput;

        void Start()
        {
            if (puppetAbilities == null)
                puppetAbilities = GetComponent<CharacterPuppet>()as CharacterAbilitiesBehaviour;
        }

        void Update()
        {
            bool triggeredJump = false;

            if (puppetAbilities.CanJump())
            {
                if (sinceAirJump < notGroundedJumpWindow)
                {
                    puppetAbilities.Jump();
                    triggeredJump = true;
                    sinceAirJump = notGroundedJumpWindow * 2f;
                }
                sinceGrounded = 0;
            }
            else
            {
                sinceGrounded += Time.deltaTime;
            }

            if (jumpInput && !triggeredJump && !puppetAbilities.WaitingOnJumpDelay())
            {
                if (puppetAbilities.IsGrounded())
                {
                    SendJump();
                }
                else if (sinceGrounded < notGroundedJumpWindow)
                {
                    SendJump();
                }
                else
                {
                    sinceAirJump = 0;
                }
            }
            else
            {
                sinceAirJump += Time.deltaTime;
            }

            jumpInput = false;
        }

        public void SendJump()
        {
            puppetAbilities.Jump();
            sinceGrounded = notGroundedJumpWindow * 2f;
        }

        #region CharacterAbilities implementation
        public override void Jump()
        {
            jumpInput = true;
        }
        public override void MoveInput(Vector3 direction)
        {
            puppetAbilities.MoveInput(direction);
        }
        public override void MoveTarget(Vector3? position)
        {
            puppetAbilities.MoveTarget(position);
        }
        public override bool CanJump()
        {
            return puppetAbilities.CanJump();
        }
        public override bool WaitingOnJumpDelay()
        {
            return puppetAbilities.WaitingOnJumpDelay();
        }
        public override bool IsGrounded()
        {
            return puppetAbilities.IsGrounded();
        }
        #endregion
    }
}