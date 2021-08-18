using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class HighlightKeyword : MonoBehaviour
{

    [System.Serializable]
    public class KeyWord
    {
        public float displayTime;
        public string keyWord;


        public KeyWord(float displaytime, string keyword)
        {
            displayTime = displaytime;
            keyWord = keyword;
        }
    }

    [Header("Select keywords for highlighting")]
    public List<KeyWord> keywords;

    
    private string textToSplit = null;
    private string[] splitText = null;

    //public List<string> keyWord;
    public Text textToHighlight;

    public Color keywordColor;

    private List<String> keys;

    [HideInInspector]
    public int wordCounter = 0;

    // Use this for initialization
    void Start()
    {


        string pattern = @"([\s.,!?;:]+)";

        if (textToHighlight != null)
        {
            
            textToSplit = textToHighlight.text;

            if (textToSplit != null)
            {

                splitText = Regex.Split(textToSplit, pattern);


            }
        }

        string hexColor = ColorUtility.ToHtmlStringRGB(keywordColor);
        hexColor = "#" + hexColor;

                

        keys = new List<string>();

        foreach (KeyWord key in keywords)
        {
            keys.Add(key.keyWord);
        }

        
        if(keywords.Count != 0)
        {
            textToHighlight.text = "";
            Text txt = textToHighlight;
            StartCoroutine(SmoothChangeKeywordColor(textToHighlight));
        }
        
    }

    
    void OnDisable()
    {
        StopAllCoroutines();
    }





    public IEnumerator SmoothChangeKeywordColor(Text text)
    {

        float timer = 0f;
        float duration = 1f;
        float initialKeyWordColorR = text.color.r;
        float initialKeyWordColorB = text.color.b;
        float initialKeyWordColorG = text.color.g;
        float initialKeyWordColorA = text.color.a;

        float finalKeyWordColorR = keywordColor.r;
        float finalKeyWordColorB = keywordColor.b;
        float finalKeyWordColorG = keywordColor.g;
        float finalKeyWordColorA = keywordColor.a;

        Color newKeywordColor;

        string[] oldText = splitText;


        string tmpText = "";


        for (int i = 0; i < oldText.Length; i++)
        {
            wordCounter++;

            foreach (string key in keys)
            {
                timer = 0f;
                if (splitText[i] != null && oldText[i] == key)
                {

                    while (timer <= duration)
                    {

                        float newAlpha = Mathf.SmoothStep(initialKeyWordColorA, finalKeyWordColorA, timer / duration);
                        float newR = Mathf.SmoothStep(initialKeyWordColorR, finalKeyWordColorR, timer / duration);
                        float newG = Mathf.SmoothStep(initialKeyWordColorG, finalKeyWordColorG, timer / duration);
                        float newB = Mathf.SmoothStep(initialKeyWordColorB, finalKeyWordColorB, timer / duration);


                        newKeywordColor.a = newAlpha;
                        newKeywordColor.r = newR;
                        newKeywordColor.g = newG;
                        newKeywordColor.b = newB;

                        Color finalColor = newKeywordColor;


                        string hexColor = ColorUtility.ToHtmlStringRGB(finalColor);

                        hexColor = "#" + hexColor;
                        splitText[i] = "<color=" + hexColor + ">" + key + "</color>";

                        for (int j = 0; j < splitText.Length; j++)
                        {
                            tmpText += splitText[j];
                        }

                        text.text = tmpText;
                        tmpText = "";
                        splitText[i] = oldText[i];

                        timer += Time.deltaTime;
                        yield return null;
                    }


                }


            }
        }

        yield return null;
    }

    
}

