using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonStateBehavior : StateMachineBehaviour
{

    /// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {

        string animName = animator.name;

        switch (animName)
        {
            case "ButtonImageMask":
                animator.SetBool("ButtonPress", false);
                break;
            case "Header":
                animator.SetBool("PopAnimReverse", false);
                break;
            case "UINotification":
                animator.SetBool("FadeOut", false);
                break;
        }
    }

    // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}
}
