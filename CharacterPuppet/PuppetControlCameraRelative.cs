using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//using UnityEngine.UI;

namespace JamTools
{
    public class PuppetControlCameraRelative : MonoBehaviour
    {
        public CharacterPuppet characterPuppet;
        public Camera cam;

        public float notGroundedJumpWindow = 0.2f;

        [HeaderAttribute("Exposed")]
        public float sinceGrounded = 1;
        public float sinceAirJump = 1;

        void Start()
        {
            if (characterPuppet == null)
                characterPuppet = GetComponentInChildren<CharacterPuppet>();
            if (cam == null)
                cam = Camera.main;
        }

        void Update()
        {
            Vector3 displacement = Vector3.zero;

            displacement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            //update motion
            float mag = displacement.magnitude;
            if (mag >= 0.01f)
            {
                if (mag > 1)
                {
                    displacement.Normalize();
                    mag = 1;
                }
                //rotate stick input to camera orientation
                Vector3 flatcamforward = cam.transform.forward;
                flatcamforward.y = 0;
                flatcamforward.Normalize();
                Quaternion camToCharacterSpace = Quaternion.FromToRotation(Vector3.forward, flatcamforward);
                displacement = (camToCharacterSpace * displacement);
                displacement.y = 0;
                displacement.Normalize();
                displacement = displacement * mag;

                characterPuppet.movementInput = displacement;
            }

            if (characterPuppet.grounded)
            {
                if (sinceAirJump < notGroundedJumpWindow)
                {
                    characterPuppet.Jump();
                    sinceAirJump = notGroundedJumpWindow * 2f;
                }
                sinceGrounded = 0;
            }
            else
            {
                sinceGrounded += Time.deltaTime;
            }

            if (Input.GetButtonDown("Jump"))
            {
                if (characterPuppet.grounded)
                {
                    Jump();
                }
                else if (sinceGrounded < notGroundedJumpWindow)
                {
                    Jump();
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
        }

        void Jump()
        {
            characterPuppet.Jump();
            sinceGrounded = notGroundedJumpWindow * 2f;
        }
    }
}