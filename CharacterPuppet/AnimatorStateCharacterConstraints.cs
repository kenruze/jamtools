using UnityEngine;
using System.Collections;
using System;
using JamTools;

public class AnimatorStateCharacterConstraints : StateMachineBehaviour
{
	public CharacterPuppet character;
    bool complainedOnce = false;
    public AnimationCurve movementThrottle;
    public AnimationCurve rotationThrottle;
    public bool useRootMotion;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
		character = animator.gameObject.GetComponentInParent<CharacterPuppet> ();
		if(character == null && !complainedOnce)
        {
            Debug.Log("no character set for animation restriction");
            complainedOnce = true;
            return;
        }
        if (useRootMotion)
            character.takingRootMotion = true;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if(character != null)
        {
			character.movementThrottle = movementThrottle.Evaluate(stateInfo.normalizedTime);
			character.rotationThrottle = rotationThrottle.Evaluate(stateInfo.normalizedTime);
        }
     }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		character.movementThrottle = 1;
		character.rotationThrottle = 1;
        if (useRootMotion)
            character.takingRootMotion = false;
        
    }

    // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}
}
