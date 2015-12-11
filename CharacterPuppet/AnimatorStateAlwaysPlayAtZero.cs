using UnityEngine;
using System.Collections;
using System;
using JamTools;

public class AnimatorStateAlwaysPlayAtZero : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetTime(0);
    }
}
