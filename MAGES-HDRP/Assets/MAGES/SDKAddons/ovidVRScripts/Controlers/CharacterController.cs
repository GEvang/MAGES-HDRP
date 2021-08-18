using System.Collections;
using UnityEngine;

namespace ovidVR.CharacterController
{
    public class CharacterController : MonoBehaviour
    {
        public GameObject body;
        public GameObject head;
        //public GameObject nasalCavity;

        private static CharacterController controller;
        public static CharacterController instance
        {
            get
            {
                if (!controller)
                {
                    controller = FindObjectOfType(typeof(CharacterController)) as CharacterController;
                    if (!controller)
                    {
                        controller = new CharacterController();
                    }
                }
                return controller;
            }
        }

        private GameObject tissueBox;

        private Animator animator;

        private bool updateSkinnedCollider = false;

        private AudioSource audioSource;
        #if FINAL_IK
        public LookAtIKTargetController lookAtIKTargetController;
        #endif
        // Use this for initialization
        private void Start()
        {
            #if FINAL_IK
            lookAtIKTargetController = GetComponent<LookAtIKTargetController>();
            #endif
            controller = new CharacterController();
            //colliderHead = GameObject.Find("female_head_skin");

            audioSource = GetComponent<AudioSource>();

            UpdateCharacterCollision();

            animator = this.gameObject.GetComponent<Animator>();
            if (!animator)
            {
                Debug.Log("CharacterController.cs No animator found");
            }

            tissueBox = GameObject.Find("tissue_box");

        }

        bool blend = false;
        bool forwardBlend;
        float blendValue;
        float interpollator = 0;
        private void Update()
        {
            if (!GetAnimatorIsPlaying())
            {
                // If animation stopped after transition, update skinned collider
                if (updateSkinnedCollider)
                {
                    UpdateCharacterCollision();

                    updateSkinnedCollider = false;

                }
            }
            else if (GetAnimatorIsPlaying())
            {
                updateSkinnedCollider = true;
            }

            if (blend)
            {
                blendValue = Mathf.Lerp(0, 1, interpollator);
                animator.SetLayerWeight(2, blendValue);

                if (forwardBlend)
                {
                    interpollator += 0.5f * Time.deltaTime;
                }
                else
                {
                    interpollator -= 0.5f * Time.deltaTime;
                    if(blendValue < 0.01)
                    {
                        blend = false;
                        blendValue = 0;
                    }
                }
            }
        }

        public void PlayBlowNose()
        {
            SetTissueBox(true);

            animator.CrossFade("BlowNose", 0.0f, 0, 0.0f);
            animator.SetBool("BlowNose", true);

            StartCoroutine(PlayBlowSound());
        }

        IEnumerator PlayBlowSound()
        {
            yield return new WaitForSeconds(6);
            audioSource.Play();
            yield return new WaitForSeconds(6);

            SetTissueBox(false);

        }

        public void ResetBlowNose()
        {
            animator.SetBool("BlowNose", false);
            animator.CrossFade("Idle", 0.0f, 0, 1.0f);

            SetTissueBox(false);

            StopAllCoroutines();
        }

        public void PlayNeckBack()
        {
            animator.CrossFade("NeckBack", 0.0f, 0, 1.0f);

            animator.SetBool("NeckBack", true);
            StopAllCoroutines();
            SetTissueBox(false);
            StartCoroutine(DelayUpdateColliders());

        }

        public void ResetNeckBack()
        {
            animator.SetBool("NeckBack", false);
            animator.CrossFade("BlowNose", 0.0f, 0, 1.0f);
            StartCoroutine(DelayUpdateColliders());
        }

        public void UndoToNeck()
        {
            animator.SetBool("NeckReset", false);
            animator.CrossFade("NeckBack", 0.0f, 0, 1.0f);
            animator.SetBool("NeckBack", true);

            StartCoroutine(DelayUpdateColliders());
        }

        public void Test()
        {
            animator.CrossFade("Idle", 0.0f, 0, 1.0f);
            animator.CrossFade("NeckBack", 0.0f, 0, 1.0f);
        }

        IEnumerator DelayUpdateColliders()
        {
            yield return new WaitForSeconds(2);
            UpdateCharacterCollision();
        }

        public void PlayNeckAnimationCustom(float value)
        {
            animator.Play("NeckBack", 0, value);
        }

        public void PlayIrritation()
        {
            blend = true;
            forwardBlend = true;

            animator.SetBool("Irritation", true);
        }

        public void StopIrritation()
        {
            forwardBlend = false;
            //animator.SetBool("Irritation", false);
        }

        public void PlayNeckReset()
        {
            animator.CrossFade("NeckReset", 0.0f, 0, 0.0f);
            animator.CrossFade("IdleIrritation", 0.0f, 2, 0.0f);

            animator.SetBool("NeckReset", true);
            animator.SetBool("NeckBack", false);
            animator.SetBool("BlowNose", false);        
            animator.SetBool("Irritation", false);        
        }

        public void CloseEyes()
        {
            animator.SetBool("CloseEyesON", true);
            animator.SetBool("CloseEyesOFF", false);
            //animator.CrossFade("CloseEyes", 0.0f, 0, 1.0f);
        }

        public void OpenEyes()
        {
            animator.SetBool("CloseEyesOFF", true);
            animator.SetBool("CloseEyesON", false);
        }

        public void EnableAnimator()
        {
            animator.enabled = true;
        }
        public void DisableAnimator()
        {
            animator.enabled = false;
        }
        public bool GetAnimatorIsInTransition()
        {
            return animator.IsInTransition(0);
        }

        public bool GetAnimatorIsPlaying()
        {
            return animator.GetCurrentAnimatorStateInfo(0).length >
                   animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        }

        public void UpdateCharacterCollision()
        {
            body.GetComponent<SkinnedCollisionHelper>().UpdateCollisionMesh();
            head.GetComponent<SkinnedCollisionHelper>().UpdateCollisionMesh();
        }

        private void SetTissueBox(bool set)
        {
            tissueBox.transform.GetChild(0).gameObject.SetActive(set);
            tissueBox.transform.GetChild(1).gameObject.SetActive(set);
            tissueBox.transform.GetChild(2).gameObject.SetActive(set);

        }

        public Collider GetHeadCollider()
        {
            return head.GetComponent<MeshCollider>();
        }
    }
}