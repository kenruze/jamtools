using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace JamTools
{
    public class CharacterPuppet : CharacterAbilitiesBehaviour
    {
        public Animator animator;
        [Tooltip("leave this string blank if your animator doesn't use it")]
        public string walkingAnimatorPropName = "walking";
        [Tooltip("leave this string blank if your animator doesn't use it")]
        public string lateralAnimatorPropName = "lateral";
        [Tooltip("leave this string blank if your animator doesn't use it")]
        public string jumpAnimatorStateName = "jump";
        [Tooltip("leave this string blank if your animator doesn't use it")]
        public string landAnimatorStateName = "land";
        public Rigidbody body;
        public CapsuleCollider capsule;

        public BuiltInInputMode builtInInputMode = BuiltInInputMode.Off;

        [Header("Movement Parameters")]
        public LayerMask groundCollisionMask = ~0;
        public float movementSpeed = 10;
        public float rotationSpeed = 720;
        public float gravity = 9.8f;
        public float terminalVelocity = 100;
        // units per second
        public float jumpStrength = 4;
        public float jumpDelay = 0;
        public float stepHeight = 0.02f;
        [Range(0, 1)]
        public float groundedSlopeThreshold = 0.75f;
        [Range(0, 1)]
        public float lateralMovementPenalty;

        [Header("Exposed")]
        public bool grounded;
        public bool jumped;
        public Vector3 velocity;
        public Vector3 movementInput;
        public Vector3? walkTarget;

        public Quaternion rotationTarget;
        public float targetStopDistance = 0.5f;
        public List<Vector3> waypoints = new List<Vector3>();
        public float movementThrottle = 1;
        public float rotationThrottle = 1;
        public bool waitingOnJumpDelay;

        void Start()
        {
            rotationTarget = transform.rotation;
        }

        void Update()
        {
            if (builtInInputMode != BuiltInInputMode.Off)
            {
                BuiltInPuppetInputUpdate(builtInInputMode);
            }
        }

        //██▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀██
        //██Character Movement                         ██

        #region character movement

        void FixedUpdate()
        {
            MovementFixedUpdate();
        }

        void MovementFixedUpdate()
        {
            if (walkTarget != null)
            {
                movementInput = walkTarget.Value - transform.position;
                movementInput.y = 0;

                if (movementInput.magnitude > targetStopDistance)
                {
                    movementInput.Normalize();
                }
                else
                {
                    movementInput = Vector2.zero;
                    walkTarget = null;
                }
            }
            else
            {
                if (waypoints.Count > 0)
                {
                    walkTarget = waypoints[0];
                    movementInput = walkTarget.Value - transform.position;
                    movementInput.y = 0;
                    movementInput.Normalize();
                }
            }

            bool wasgrounded = grounded;
            grounded = false;
            //update motion
            float mag = movementInput.magnitude;
            if (mag >= 0.01f)
            {
                if (mag > 1)
                {
                    movementInput.Normalize();
                    mag = 1;
                }

                Vector3 moveStep = movementInput * movementSpeed * movementThrottle;
                moveStep.y = velocity.y;
                velocity = moveStep;
                rotationTarget = Quaternion.LookRotation(movementInput);
            }
            else if (wasgrounded)
            {
                velocity *= Mathf.Clamp01(1 - 16 * Time.deltaTime);
            }
            float lateralAngle = Geometry.AngleAroundAxis(rotationTarget * Vector3.forward, body.transform.rotation * Vector3.forward, Vector3.up);
            if (lateralMovementPenalty > 0)
            {
                float vy = velocity.y;
                velocity *= 1 - lateralMovementPenalty * (Mathf.Abs(lateralAngle) / 180f);
                velocity.y = vy;
            }

            if (animator != null && walkingAnimatorPropName != "")
            {
                animator.SetFloat(walkingAnimatorPropName, mag);
            }  
            if (animator != null && lateralAnimatorPropName != "")
            {
                const float angleFactor = 1f / 50f;
                animator.SetFloat(lateralAnimatorPropName, lateralAngle * angleFactor);
            }

            float castRadius = capsule.radius - 0.01f;
            RaycastHit[] hits = Physics.SphereCastAll(transform.position + Vector3.up * capsule.height / 2, castRadius, Vector3.down, capsule.height / 2 - castRadius + 0.02f, groundCollisionMask);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider == capsule)
                    continue;
                if (hits[i].collider.isTrigger)
                    continue;
                if ((1 - hits[i].normal.y) > groundedSlopeThreshold)
                    continue;//could develop sliding response
				
                grounded = true;
                if (jumped)
                {
                    jumped = false;
                    SendMessage("Land");
                }
                float disparity = capsule.height / 2 - castRadius - hits[i].distance;
                if (disparity > 0 - stepHeight)
                {
                    //velocity.y += (disparity + stepHeight);
                    float tinyAmount = 0.001f;
                    transform.position += Vector3.up * (disparity + stepHeight - tinyAmount);
                    break;
                }
            }

            //█physics
            if (!grounded && !wasgrounded)
            {
                velocity.y -= gravity * Time.deltaTime;
                if (velocity.y < -terminalVelocity)
                    velocity.y = -terminalVelocity;
            }
            else
            {
                velocity.y *= 0.5f;
            }

            body.transform.rotation = Quaternion.RotateTowards(body.transform.rotation, rotationTarget, rotationSpeed * Time.deltaTime);
            body.velocity = velocity;
        }

        public void Land()
        {
            if (animator != null && landAnimatorStateName != "")
            {
                animator.Play(landAnimatorStateName, 0, 0);
            }
        }

        public void StopWalking()
        {
            walkTarget = null;
        }


        IEnumerator JumpAction(float jumpDelay)
        {
            if (animator != null && jumpAnimatorStateName != "")
            {
                animator.Play(jumpAnimatorStateName, 0, 0);
            }
            yield return new WaitForSeconds(jumpDelay);
            transform.position += Vector3.up * (stepHeight);
            velocity.y = jumpStrength;
            body.velocity = velocity;
            grounded = false;
            jumped = true;
            yield return new WaitForEndOfFrame();
            waitingOnJumpDelay = false;
        }

        public void PlayAnimation(string animationName, float crossFadeTime = 0.25f)
        {
            animator.CrossFade(animationName, crossFadeTime, 0, 0);
        }

        #endregion

        //██Character Movement                         ██
        //██▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄██

        #region default CharacterAbilities

        public override void Jump()
        {
            if (waitingOnJumpDelay)
                return;
            waitingOnJumpDelay = true;
            StartCoroutine(JumpAction(jumpDelay));
        }

        public override void MoveInput(Vector3 direction)
        {
            walkTarget = null;
            this.movementInput = direction;
        }

        public override void MoveTarget(Vector3? position)
        {
            walkTarget = position;
        }

        public override bool CanJump()
        {
            return grounded && !waitingOnJumpDelay;
        }

        public override bool WaitingOnJumpDelay()
        {
            return waitingOnJumpDelay;
        }

        public override bool IsGrounded()
        {
            return grounded;
        }

        #endregion

        #region optional built-in input

        public enum BuiltInInputMode
        {
            Off,
            DefaultCameraRelative,
            ClickRaycastTarget,
        }

        void BuiltInPuppetInputUpdate(BuiltInInputMode inputMode)
        {
            switch (inputMode)
            {
                case BuiltInInputMode.DefaultCameraRelative:
                    Camera cam = Camera.main;
                    Vector3 displacement = Vector3.zero;
                    displacement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
                    float mag = Mathf.Min(1, displacement.magnitude);
                    if (mag >= 0.01f)
                    {
                        //rotate stick input to camera orientation
                        Vector3 flatcamforward = cam.transform.forward;
                        flatcamforward.y = 0;
                        flatcamforward.Normalize();
                        Quaternion camToCharacterSpace = Quaternion.FromToRotation(Vector3.forward, flatcamforward);
                        displacement = (camToCharacterSpace * displacement);
                        displacement.y = 0;
                        displacement.Normalize();
                        displacement = displacement * mag;
                    }
                    else
                    {
                        displacement = Vector3.zero;
                    }
                    movementInput = displacement;

                    if (Input.GetButtonDown("Jump") && CanJump())
                        Jump();
                    break;

                case BuiltInInputMode.ClickRaycastTarget:
                    if (Input.GetMouseButtonDown(0))
                    {
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit[] hits = Physics.RaycastAll(ray);
                        for (int i = 0; i < hits.Length; i++)
                        {
                            if (hits[i].collider == capsule)
                                continue;
                            if (hits[i].collider.isTrigger)
                                continue;
                            walkTarget = hits[i].point;
                        }
                    }
                    break;
            }
        }

        #endregion

        [ContextMenu("Initialize puppet components")]
        public void InitializeComponents()
        {
            animator = GetComponentInChildren<Animator>();
            body = GetComponentInChildren<Rigidbody>();
            if (body == null)
            {
                body = gameObject.AddComponent<Rigidbody>();
            }
            body.constraints = RigidbodyConstraints.FreezeRotation; 
            capsule = GetComponentInChildren<CapsuleCollider>();
            if (capsule == null)
            {
                capsule = gameObject.AddComponent<CapsuleCollider>();
            }
            capsule.height = 2;
            capsule.radius = 0.5f;
            capsule.center = Vector3.up;
        }
    }

    public interface CharacterAbilities
    {
        void Jump();

        void MoveInput(Vector3 direction);

        void MoveTarget(Vector3? position);

        bool CanJump();

        bool WaitingOnJumpDelay();

        bool IsGrounded();
    }
}