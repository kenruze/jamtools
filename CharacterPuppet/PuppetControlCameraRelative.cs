using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InControl;

//using UnityEngine.UI;

namespace JamTools
{
    public class PuppetControlCameraRelative : MonoBehaviour
    {
        public CharacterAbilitiesBehaviour puppetAbilities;
        public Camera cam;

        public enum InputMode
        {
            DefaultUnityAxes,
            InControl,
            InControlTouchAnalogue
        }

        public InputMode inputMode;
        public int playerNumber = 0;


        public enum CameraTrackConstrainZ
        {
            Off,
            Project,
            Map,
            //MapFlipX
        }

        public CameraTrackConstrainZ constrainZWithCameraTracks;
        public bool trackMagnetWhenConstrained = true;

        InputDevice inputDevice;

        void Start()
        {
            if (puppetAbilities == null)
                puppetAbilities = GetComponentInChildren<CharacterPuppet>() as CharacterAbilitiesBehaviour;
            if (cam == null)
                cam = Camera.main;

            switch (inputMode)
            {
                case InputMode.InControl:            
                    inputDevice = InputManager.Devices[playerNumber];
                    break;
                case InputMode.InControlTouchAnalogue:            
                    inputDevice = TouchManager.Device;
                    break;
            }
        }

        void Update()
        {
            Vector3 displacement = Vector3.zero;
            float verticalDisplacement;
            switch (inputMode)
            {
                case InputMode.DefaultUnityAxes:
                    displacement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
                    if (Input.GetButtonDown("Jump"))
                        puppetAbilities.Jump();
                    break;
                case InputMode.InControl:
                    displacement = new Vector3(inputDevice.Direction.X, 0, inputDevice.Direction.Y);
                    if (inputDevice.Action1.WasPressed)
                        puppetAbilities.Jump();
                    break;
                case InputMode.InControlTouchAnalogue:
                    inputDevice = TouchManager.Device;
                    displacement = new Vector3(inputDevice.Direction.X, 0, inputDevice.Direction.Y);
                    if (inputDevice.Action1.WasPressed)
                        puppetAbilities.Jump();
                    break;
            }

            float mag = Mathf.Min(1, displacement.magnitude);
            if (mag >= 0.01f)
            {
                //rotate stick input to camera orientation
                Vector3 flatcamforward = cam.transform.forward;
                flatcamforward.y = 0;
                flatcamforward.Normalize();
                Quaternion camToCharacterSpace = Quaternion.FromToRotation(Vector3.forward, flatcamforward);
                if (constrainZWithCameraTracks != CameraTrackConstrainZ.Off && CameraTrack.s_currentCamera != null && CameraTrack.s_currentCamera.constrainZ)
                {
                    switch (constrainZWithCameraTracks)
                    {
                        case CameraTrackConstrainZ.Project:
                            displacement = (camToCharacterSpace * displacement);
                            displacement = Vector3.ProjectOnPlane(displacement, CameraTrack.s_currentCamera.m_targetTrackingBox.transform.forward);
                            verticalDisplacement = displacement.y;
                            if (trackMagnetWhenConstrained)
                            {
                                Vector3 disp = puppetAbilities.transform.position - CameraTrack.s_currentCamera.m_targetTrackingBox.transform.position;
                                disp -= Vector3.ProjectOnPlane(disp, CameraTrack.s_currentCamera.m_targetTrackingBox.transform.forward);
                                displacement -= disp;
                            }
                            break;
                    }
                }
                else
                {
                    displacement = (camToCharacterSpace * displacement);
                }
                displacement.y = 0;
                displacement.Normalize();
                displacement = displacement * mag;
            }
            else
            {
                displacement = Vector3.zero;
            }
            puppetAbilities.MoveInput(displacement);
        }
    }
}