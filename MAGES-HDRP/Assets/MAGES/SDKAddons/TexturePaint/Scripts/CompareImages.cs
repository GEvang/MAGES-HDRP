using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompareImages : MonoBehaviour {

    public Texture a;
    public Texture b;


    // Use this for initialization
    void Start () {
        int dif = 0;
        if (a.width == b.width && a.height == b.height)
        {
            RenderTexture rt = this.GetComponent<SkinnedMeshRenderer>().sharedMaterials[0].mainTexture as RenderTexture;
            a = rt;
            Color[] pixelsColorA = toTexture2D(SetupRenderTexture(a)).GetPixels();
            Color[] pixelsColorB = toTexture2D(SetupRenderTexture(b)).GetPixels();
            for (int c = 0; c < pixelsColorA.Length; c++)
            {

                if (pixelsColorA[c] != pixelsColorB[c])
                    dif++;
                //float difference = Vector3.Distance(
                //    new Vector3(pixelsColorA[c].r, pixelsColorA[c].g, pixelsColorA[c].b),
                //    new Vector3(pixelsColorB[c].r, pixelsColorB[c].g, pixelsColorB[c].b));
                //Debug.Log("Distance = " + difference + c);
            }
            Debug.Log("Found " + dif + "/" + pixelsColorA.Length) ;
        }
	}

    private Texture2D toTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.ARGB32, false);
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0f, 0f, (float)rTex.width, (float)rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }

    private RenderTexture SetupRenderTexture(Texture baseTex)
    {
        RenderTexture rt = new RenderTexture(baseTex.width, baseTex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        rt.filterMode = baseTex.filterMode;
        rt.wrapMode = baseTex.wrapMode;

        Graphics.Blit(baseTex, rt);
        return rt;
    }

    // Update is called once per frame
    void Update () {
		
	}
}
