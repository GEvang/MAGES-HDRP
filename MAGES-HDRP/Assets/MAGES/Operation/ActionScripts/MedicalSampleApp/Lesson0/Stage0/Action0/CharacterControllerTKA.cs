/**
 * Controller for the characters animation
 * Plays the initial cut and rise/lower leg.
 * 
 */

using System.Collections;
using UnityEngine;

namespace ovidVR.CharacterController
{
    public class CharacterControllerTKA : MonoBehaviour
    {

        private static CharacterControllerTKA controller;
        public static CharacterControllerTKA instance
        {
            get
            {
                if (!controller)
                {
                    controller = FindObjectOfType(typeof(CharacterControllerTKA)) as CharacterControllerTKA;
                    if (!controller)
                    {
                        controller = new CharacterControllerTKA();
                    }
                }
                return controller;
            }
        }

        private Animator animator;

        private bool updateSkinnedCollider = false;

        // Use this for initialization
        void Start()
        {
            controller = new CharacterControllerTKA();
            animator = this.gameObject.GetComponent<Animator>();
            if (!animator)
            {
                Debug.LogError("No animator found");
            }
        }

        private void Update()
        {
            if (!GetAnimatorIsPlaying())
            {
                // If animation stopped after transition, update skinned collider
                if (updateSkinnedCollider)
                {
                    transform.GetChild(4).GetChild(0).GetComponent<SkinnedCollisionHelper>().UpdateCollisionMesh();

                    updateSkinnedCollider = false;
                }
            }
            else if (GetAnimatorIsPlaying())
            {
                updateSkinnedCollider = true;
            }
        }

        public void PlayCut1Animation()
        {
            animator.SetBool("Cut1", true);

        }
        public void ResetCutAnimation()
        {
            animator.SetBool("Cut1", false);
            animator.SetBool("Cut2", false);
            animator.SetBool("Cut3", false);
            animator.SetBool("Cut4", false);

            animator.CrossFade("Idle", 0.0f, 0, 0.0f);
        }
        public void PlayCut2Animation()
        {
            animator.SetBool("Cut2", true);

        }
        public void PlayCut3Animation()
        {
            animator.SetBool("Cut3", true);
        }
        public void PlayCut4Animation()
        {
            animator.SetBool("Cut4", true);
        }
        public void PlayOpenSkinFull()
        {
            StopAllCoroutines();
            animator.SetBool("OpenSkin", true);
            StartCoroutine(DelayCollider(3.5f));
        }
        public void ResetOpenSkinFull()
        {
            animator.SetBool("OpenSkin", false);

            animator.CrossFade("cut4", 0.0f, 0, 1.0f);
        }
        public void PlayRiseLeg()
        {
            StopAllCoroutines();
            animator.SetBool("LowerLeg", false);
            animator.SetBool("RiseLeg", true);
            StartCoroutine(DelayCollider(8));
        }

        IEnumerator DelayCollider(float value)
        {
            yield return new WaitForSeconds(value);
            transform.GetChild(4).GetChild(0).GetComponent<SkinnedCollisionHelper>().UpdateCollisionMesh();
        }

        public void ResetRiseLeg()
        {
            animator.SetBool("LowerLeg", false);
            animator.SetBool("RiseLeg", false);

            animator.CrossFade("lowerLeg", 0.0f, 0, 0.0f);
        }
        public void PlayMovePattela()
        {
            animator.SetBool("MovePattelaToSide", true);
        }
        public void ResetMovePattela()
        {
            animator.SetBool("MovePattelaToSide", false);

            animator.CrossFade("openSkinFull", 0.0f, 0, 1.0f);
        }
        public void PlayLowerLeg()
        {
            animator.SetBool("RiseLeg", false);
            animator.SetBool("LowerLeg", true);
        }
        public void ResetLowerLeg()
        {
            animator.SetBool("RiseLeg", false);
            animator.SetBool("LowerLeg", false);
            animator.CrossFade("riseLeg", 0.0f, 0, 0.0f);
        }
        public void PlayBonePush()
        {
            animator.SetBool("BonePush", true);
        }
        public void ResetPlayBonePush()
        {
            animator.SetBool("BonePush", false);            
        }        


        public void PlayCloseSkin()
        {
            animator.SetBool("CloseSkin", true);
        }

        public void PlayClosePattela()
        {
            animator.SetBool("ClosePattela", true);
        }
        public void PlayCloseCut1()
        {
            animator.SetBool("CloseCut1", true);
        }
        public void PlayCloseCut2()
        {
            animator.SetBool("CloseCut2", true);
        }
        public void PlayCloseCut3()
        {
            animator.SetBool("CloseCut3", true);
        }
        public void PlayCloseCut4()
        {
            animator.SetBool("CloseCut4", true);
        }




        public void EnableAnimator()
        {
            animator.enabled = true;
        }

        public void DisableAnimator()
        {
            animator.enabled = false;

        }


        public void ResetSewing()
        {
            animator.SetBool("CloseCut1", false);
            animator.SetBool("CloseCut2", false);
            animator.SetBool("CloseCut3", false);
            animator.SetBool("CloseCut4", false);
            animator.SetBool("CloseSkin", false);
            animator.SetBool("ClosePattela", false);
            animator.SetBool("LowerLeg", false);
            animator.CrossFade("riseLeg", 0.0f, 0, 1.0f);
        }

        public bool GetAnimatorIsPlaying()
        {
            return animator.GetCurrentAnimatorStateInfo(0).length >
                   animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        }

        public bool GetIsAnimationLoaded(string _anim)
        {
            return animator.GetCurrentAnimatorStateInfo(0).IsName(_anim);
        }

        public void UpdateCharacterCollision()
        {
            transform.GetChild(4).GetChild(0).GetComponent<SkinnedCollisionHelper>().UpdateCollisionMesh();
        }
    }
}