using System;
using System.Collections;
using UnityEngine;

public class HeaderAnimation : MonoBehaviour
{

    [HideInInspector]
    public float spawnButtonsDur = 0.8f;


    // Header Icon Animator 
    private Animator headerIconAnim;
    private float animationDuration;
    private float animationSpeed;
    private float waitTime;
    private Transform contentTransform;

    private int wordCounter = 0;

    public float durationPerWord = 0.5f;

    [HideInInspector]
    public float textHeaderTime;

    // Start is called before the first frame update
    void Start()
    {
        if (!transform.Find("InterfaceSpawn") || !transform.Find("InterfaceContent"))
        { Destroy(this); return; }

        headerIconAnim = transform.Find("InterfaceSpawn/CenterInterfaceHeader").GetComponent<Animator>();

        
        RuntimeAnimatorController ac = headerIconAnim.runtimeAnimatorController;    //Get Animator controller


        animationDuration = ac.animationClips[0].length;
        animationSpeed = headerIconAnim.speed;

        contentTransform = transform.Find("InterfaceContent");

        

        StartCoroutine(PlayHeaderIconAnimation());

        string source = GetComponent<HighlightKeyword>().textToHighlight.text;
        wordCounter = source.Split(new Char[] { ' ', ',', '.', ':', '\t' }).Length -1;
        Debug.Log(wordCounter);
        textHeaderTime = wordCounter * durationPerWord;
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }


    private IEnumerator PlayHeaderIconAnimation()
    {
        contentTransform.localScale = new Vector3();

        headerIconAnim.Play(0);
        waitTime = animationDuration * (1 / animationSpeed);
        //Wait for Total Icon Animation duration
        yield return new WaitForSeconds(waitTime);

        contentTransform.localPosition += new Vector3(0f, 0f, 0.2f);

        float timer = 0f;
        float duration = spawnButtonsDur;
        contentTransform.localScale = new Vector3(1f, 1f, 1f);

        while (timer <= duration)
        {
            
            contentTransform.localPosition = new Vector3(0f, 0f,
                                                         Mathf.SmoothStep(0.1f, 0f, timer / duration));

            contentTransform.localScale = new Vector3(1,
                                                      1,
                                                      Mathf.SmoothStep(0f, 1f, timer / duration));

            
            timer += Time.deltaTime;
            yield return null;
        }

        
    }
}
