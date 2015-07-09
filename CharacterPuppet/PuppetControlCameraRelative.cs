﻿//#define JamToolsUseInControl


using UnityEngine;
using System.Collections;
using System.Collections.Generic;


#if JamToolsUseInControl
//if you have an error here, and you don't use InControl for gamepad input, comment out the define at the top of this file
using InControl;
#endif

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
            InControlTouchAnalogue,
            Any
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
        //moved to camera zone setting
        //public CameraTrackConstrainZ constrainZWithCameraTracks;
        public bool trackMagnetWhenConstrained = true;
        internal float verticalDisplacement;

        public TouchButtonPad touchButtonPad;

        #if JamToolsUseInControl
        InputDevice inputDevice;
        #endif

        //touch only
        float tapReleaseJumpTimer;

        void Start()
        {
            if (puppetAbilities == null)
                puppetAbilities = GetComponentInChildren<CharacterPuppet>() as CharacterAbilitiesBehaviour;
            if (cam == null)
                cam = Camera.main;

            #if JamToolsUseInControl
            switch (inputMode)
            {
                case InputMode.InControl:            
                    inputDevice = InputManager.Devices[playerNumber];
                    break;
                case InputMode.InControlTouchAnalogue:            
                    inputDevice = TouchManager.Device;
                    break;
            }

            touchButtonPad.touchButtonPadActions[(int)TouchButtonPad.TouchButtonPadAction.SwipeUp] = puppetAbilities.Jump;
            touchButtonPad.touchButtonPadActions[(int)TouchButtonPad.TouchButtonPadAction.Tap] = AttackAction;
            #else
            Debug.Log("Using input without InControl");
            #endif
        }

        public void AttackAction()
        {
            print("Attack");
            puppetAbilities.GetComponent<CharacterPuppet>().PlayAnimation("slash", 0);
        }

        void Update()
        {
            Vector3 displacement = Vector3.zero;

            #if JamToolsUseInControl
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
                    break;
                case InputMode.Any:
                    bool jumpedForAnother = false;
                    displacement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
                    inputDevice = InputManager.Devices[playerNumber];
                    if (displacement.magnitude < 0.1f)
                    {
                        displacement = new Vector3(inputDevice.Direction.X, 0, inputDevice.Direction.Y);
                    }
                    if (inputDevice.Action1.WasPressed && !jumpedForAnother)
                    {
                        puppetAbilities.Jump();  
                        jumpedForAnother = true;
                    }
                    
                    inputDevice = TouchManager.Device;
                    if (displacement.magnitude < 0.1f)
                    {
                        displacement = new Vector3(inputDevice.Direction.X, 0, inputDevice.Direction.Y);
                    }
                    
                    if (Input.GetButtonDown("Jump"))
                        puppetAbilities.Jump();

                    tapReleaseJumpTimer -= Time.deltaTime;
                                                                       
                    break;
            }

            if (inputMode == InputMode.InControlTouchAnalogue || inputMode == InputMode.Any)
            {
                touchButtonPad.UpdateTouchButtonPad();
            }
            #else
            displacement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            if (Input.GetButtonDown("Jump"))
                puppetAbilities.Jump();
            #endif

            float mag = Mathf.Min(1, displacement.magnitude);
            if (mag >= 0.01f)
            {
                //rotate stick input to camera orientation
                Vector3 flatcamforward = cam.transform.forward;
                flatcamforward.y = 0;
                flatcamforward.Normalize();
                Quaternion camToCharacterSpace = Quaternion.FromToRotation(Vector3.forward, flatcamforward);
                if (CameraTrack.s_currentCamera != null && CameraTrack.s_currentCamera.constrainZ != CameraTrackConstrainZ.Off)
                {
                    switch (CameraTrack.s_currentCamera.constrainZ)
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
                        case CameraTrackConstrainZ.Map:
                            verticalDisplacement = displacement.z;
                            displacement.z = 0;
                            float mgnitude = displacement.magnitude;
                            camToCharacterSpace = Quaternion.FromToRotation(Vector3.forward, CameraTrack.s_currentCamera.m_targetTrackingBox.transform.forward);
                            displacement = (camToCharacterSpace * displacement);
                            displacement = Vector3.ProjectOnPlane(displacement, CameraTrack.s_currentCamera.m_targetTrackingBox.transform.forward);
                            displacement = displacement.normalized * mgnitude;
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
            }
            else
            {
                displacement = Vector3.zero;
            }
            puppetAbilities.MoveInput(displacement);
        }
    }

    [System.Serializable]
    public class TouchButtonPad
    {
        public enum TouchButtonPadAction
        {
            Tap,
            SwipeUp,
            SwipeDown,
            SwipeLeft,
            SwipeRight,
        }

        internal System.Action[] touchButtonPadActions = new System.Action[System.Enum.GetValues(typeof(TouchButtonPadAction)).Length];

        #if JamToolsUseInControl
        public TouchControl.AnalogTarget stick;
        public InputControlType button;
        #endif

        //seconds
        public float tapReleaseWindow = 0.2f;
        //stick magnitude
        public float dragDistance = 0.75f;

        [Header("Exposed")]
        public float tapReleasedTimer;
        public bool fired;

        public void UpdateTouchButtonPad()
        {
            #if JamToolsUseInControl

            var device = TouchManager.Device;

            if (device != null)
            {
                InputControl button = device.GetControl(this.button);
                TwoAxisInputControl stick = device.RightStick;
                if (this.stick == TouchControl.AnalogTarget.LeftStick)
                    stick = device.LeftStick;
                Vector2 stickValue = stick.Value;

                if (button.Target == this.button)
                {
                    if (button.WasPressed)
                    {
                        //Debug.Log("press");
                        tapReleasedTimer = tapReleaseWindow;
                        fired = false;
                    }
                    else if (button.WasReleased)
                    {
                        if (tapReleasedTimer > 0 && !fired)
                        {
                            //Debug.Log("release jump");
                            if (touchButtonPadActions[(int)TouchButtonPadAction.Tap] != null)
                            {
                                touchButtonPadActions[(int)TouchButtonPadAction.Tap].Invoke();
                                fired = true;
                            }
                        }
                    }
                    else if (button.IsPressed)
                    {
                        if (tapReleasedTimer > 0)
                        {
                            if (!fired && touchButtonPadActions[(int)TouchButtonPadAction.SwipeUp] != null && stickValue.y > dragDistance)
                            {
                                touchButtonPadActions[(int)TouchButtonPadAction.SwipeUp].Invoke();
                                fired = true;
                            }
                            if (!fired && touchButtonPadActions[(int)TouchButtonPadAction.SwipeDown] != null && stickValue.y < -dragDistance)
                            {
                                touchButtonPadActions[(int)TouchButtonPadAction.SwipeDown].Invoke();
                                fired = true;
                            }
                            if (!fired && touchButtonPadActions[(int)TouchButtonPadAction.SwipeLeft] != null && stickValue.x < -dragDistance)
                            {
                                touchButtonPadActions[(int)TouchButtonPadAction.SwipeLeft].Invoke();
                                fired = true;
                            }
                            if (!fired && touchButtonPadActions[(int)TouchButtonPadAction.SwipeRight] != null && stickValue.x > dragDistance)
                            {
                                touchButtonPadActions[(int)TouchButtonPadAction.SwipeRight].Invoke();
                                fired = true;
                            }

                            tapReleasedTimer -= Time.deltaTime;
                            if (tapReleasedTimer <= 0 && !fired)
                            {
                                //Debug.Log("timeout jump");
                                if (touchButtonPadActions[(int)TouchButtonPadAction.Tap] != null)
                                {
                                    touchButtonPadActions[(int)TouchButtonPadAction.Tap].Invoke();
                                    fired = true;
                                }
                            }
                        }
                    }
                }
            }
            #endif
        }
    }
}