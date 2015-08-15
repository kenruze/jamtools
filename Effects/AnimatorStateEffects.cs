using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace JamTools
{
	public class AnimatorStateEffects : StateMachineBehaviour
	{

		public List<EffectQueue> queues = new List<EffectQueue> ();
		public bool allowLooping;

		float wrappedTime;
		int loops;
		// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
		override public void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			for (int i = 0; i < queues.Count; i++)
			{
				queues [i].fired = false;
			}
			loops = 0;
			wrappedTime = stateInfo.normalizedTime;
		}

		// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
		override public void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (allowLooping)
			{
				bool loopReset = false;
				wrappedTime = stateInfo.normalizedTime - loops;
				if (wrappedTime < 0)
				{
					loops = (int)stateInfo.normalizedTime;
					wrappedTime = stateInfo.normalizedTime - loops;
					loopReset = true;
				}
				if (wrappedTime > 1)
				{
					loopReset = true;
					while (wrappedTime > 1)
					{
						loops++;
						wrappedTime = stateInfo.normalizedTime - loops;
					}
				}
				if (loopReset)
				{
					for (int i = 0; i < queues.Count; i++)
					{
						queues [i].fired = false;
					}
				}
			}

			for (int i = 0; i < queues.Count; i++)
			{
				if (!queues [i].fired && wrappedTime > queues [i].queue)
				{
					if (EffectOutlet.Get != null)
					{
						var pos = animator.transform.position + animator.transform.rotation * queues [i].localPosition;
						var parent = queues [i].passParent ? animator.transform : null;
						EffectOutlet.Get.PlayEffect (queues [i].effect, pos, animator.transform.rotation, parent);
						Debug.Log ("fired effect");
					}
					queues [i].fired = true;
				}
			}

		}

		// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
		//override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		//
		//}

		// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
		//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		//
		//}

		// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
		//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		//
		//}

		[System.Serializable]
		public class EffectQueue
		{
			public EffectType effect;
			[Range (0, 1)]
			public float queue;
			public Vector3 localPosition;
			public bool passParent;
			internal bool fired;
		}
	}

}