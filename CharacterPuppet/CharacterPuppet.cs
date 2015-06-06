using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace JamTools
{
	public class CharacterPuppet : MonoBehaviour
	{
		
		[Header ("Init")]
		[ContextMenuItem ("init", "Init")]
		public Animator animator;
		public string walkingAnimatorPropName = "walking";
		public string jumpAnimatorStateName = "jump";
		public string landAnimatorStateName = "land";
		public Rigidbody body;
		public CapsuleCollider capsule;

		[Header ("Movement Parameters")]
		public LayerMask groundCollisionMask = ~0;
		public float movementSpeed = 10;
		public float rotationSpeed = 720;
		public float gravity = 9.8f;
		public float jumpStrength = 4;
		public float stepHeight = 0.02f;

		[Header ("Exposed")]
		public bool grounded;
		public bool jumped;
		public Vector3 velocity;
		public Vector3 movementInput;
		public Vector3? walkTarget;

		public Quaternion rotationTarget;
		public float targetStopDistance = 0.5f;
		public List<Vector3> waypoints = new List<Vector3> ();
		public float movementThrottle = 1;
		public float rotationThrottle = 1;

		public void Init ()
		{
			//movement init
			if (animator == null) {
				animator = GetComponentInChildren<Animator> ();
			}
			if (body == null) {
				body = GetComponentInChildren<Rigidbody> ();
			}
			if (capsule == null) {
				capsule = GetComponentInChildren<CapsuleCollider> ();
			}

		}

		void Start ()
		{
			rotationTarget = transform.rotation;
		}

		void Update ()
		{
		}

		//██▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀██
		//██Character Movement                         ██

		#region character movement

		void FixedUpdate ()
		{
			MovementFixedUpdate ();
		}

		void MovementFixedUpdate ()
		{
			if (walkTarget != null) {
				movementInput = walkTarget.Value - transform.position;
				movementInput.y = 0;

				if (movementInput.magnitude > targetStopDistance) {
					movementInput.Normalize ();

				} else {
					movementInput = Vector2.zero;
					walkTarget = null;
				}
			} else {
				if (waypoints.Count > 0) {
					walkTarget = waypoints [0];
					movementInput = walkTarget.Value - transform.position;
					movementInput.y = 0;
					movementInput.Normalize ();
				}
			}

			bool wasgrounded = grounded;
			grounded = false;
			//update motion
			float mag = movementInput.magnitude;
			if (mag >= 0.01f) {
				if (mag > 1) {
					movementInput.Normalize ();
					mag = 1;
				}

				Vector3 moveStep = movementInput * movementSpeed * movementThrottle;
				moveStep.y = velocity.y;
				velocity = moveStep;
				rotationTarget = Quaternion.LookRotation (movementInput);
			} else if (wasgrounded) {
				velocity *= Mathf.Clamp01 (1 - 16 * Time.deltaTime);
			}

			if (animator != null && walkingAnimatorPropName != "") {
				animator.SetFloat (walkingAnimatorPropName, mag);
			}

			float castRadius = capsule.radius - 0.1f;
			RaycastHit[] hits = Physics.SphereCastAll (transform.position + Vector3.up * capsule.height / 2, castRadius, Vector3.down, capsule.height / 2 - castRadius + 0.02f, groundCollisionMask);
			for (int i = 0; i < hits.Length; i++) {
				if (hits [i].collider == capsule)
					continue;
				if (hits [i].collider.isTrigger)
					continue;

				grounded = true;
				if (jumped) {
					jumped = false;
					SendMessage ("Land");
				}
				float disparity = capsule.height / 2 - castRadius - hits [i].distance;
				if (disparity > 0 - stepHeight) {
					//velocity.y += (disparity + stepHeight);
					float tinyAmount = 0.001f;
					transform.position += Vector3.up * (disparity + stepHeight - tinyAmount);
					break;
				}
			}

			//█physics
			if (!grounded && !wasgrounded) {
				velocity.y -= gravity * Time.deltaTime;
			} else {
				velocity.y *= 0.5f;
			}

			body.transform.rotation = Quaternion.RotateTowards (body.transform.rotation, rotationTarget, rotationSpeed * Time.deltaTime);
			body.velocity = velocity;
		}

		public void Land ()
		{
			if (animator != null && landAnimatorStateName != "") {
				animator.Play (landAnimatorStateName, 0, 0);
			}
		}

		public void SetWalkTarget (Vector3 position)
		{
			walkTarget = position;
		}

		public void StopWalking ()
		{
			walkTarget = null;
		}

		public void Jump (float jumpDelay =0)
		{
			StartCoroutine (JumpAction (jumpDelay));
		}

		IEnumerator JumpAction (float jumpDelay)
		{
			if (animator != null && jumpAnimatorStateName != "") {
				animator.Play (jumpAnimatorStateName, 0, 0);
				yield return new WaitForSeconds (jumpDelay);
			}
			transform.position += Vector3.up * (stepHeight);
			velocity.y = jumpStrength;
			body.velocity = velocity;
			grounded = false;
			jumped = true;
		}

		public void PlayAnimation (string animationName, float crossFadeTime = 0.25f)
		{
			animator.CrossFade (animationName, crossFadeTime, 0, 0);
		}

		#endregion

		//██Character Movement                         ██
		//██▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄██
	}
}