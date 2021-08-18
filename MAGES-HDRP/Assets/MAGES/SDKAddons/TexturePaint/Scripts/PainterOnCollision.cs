using System;
using UnityEngine;

public class PainterOnCollision : MonoBehaviour
{
    [Serializable]
    public class CollisionInfo
    {
        public string colliderTag;
        public string colliderName;
    }

    public CollisionInfo collisionInformation;

    [SerializeField]
    private bool enableFluids = false;

    [SerializeField]
    private float maxFluidTime = 2f;

    [SerializeField]
    private float startSize = 0.003f;

    public Texture sourcePaintMain = null;

    public Texture sourcePaintNormal = null;

    public Texture sourcePaintMetalic = null;

    public Material shaderToPaint;

    private void Start()
    {
        if (this.sourcePaintNormal)
        {
            this.converNormalToMainTextureAlpha();
        }
        if (this.sourcePaintMetalic)
        {
            this.converMetalicToMainTextureAlpha();
        }
    }

    private void converNormalToMainTextureAlpha()
    {
        RenderTexture tex3 = this.SetupRenderTexture(this.sourcePaintNormal);
        RenderTexture tex2 = this.SetupRenderTexture(this.sourcePaintMain);
        Texture2D texture3 = this.toTexture2D(tex3);
        Texture2D texture2 = this.toTexture2D(tex2);
        Color[] pixelsColor = texture3.GetPixels();
        Color[] pixelsAlpha = texture2.GetPixels();
        Color[] resultingPixels = new Color[pixelsColor.Length];
        for (int c = 0; c < pixelsColor.Length; c++)
        {
            if (pixelsAlpha[c].a > 0.8f)
            {
                resultingPixels[c] = new Color(pixelsColor[c].r, pixelsColor[c].g, pixelsColor[c].b, 0.5f);
            }
            else
            {
                resultingPixels[c] = new Color(pixelsColor[c].r, pixelsColor[c].g, pixelsColor[c].b, pixelsAlpha[c].a);
            }
        }
        Texture2D result = new Texture2D(texture3.width, texture3.height);
        result.SetPixels(resultingPixels);
        result.Apply();
        this.sourcePaintNormal = result;
    }

    private void converMetalicToMainTextureAlpha()
    {
        RenderTexture tex3 = this.SetupRenderTexture(this.sourcePaintMetalic);
        RenderTexture tex2 = this.SetupRenderTexture(this.sourcePaintMain);
        Texture2D texture3 = this.toTexture2D(tex3);
        Texture2D texture2 = this.toTexture2D(tex2);
        Color[] pixelsColor = texture3.GetPixels();
        Color[] pixelsAlpha = texture2.GetPixels();
        Color[] resultingPixels = new Color[pixelsColor.Length];
        for (int c = 0; c < pixelsColor.Length; c++)
        {
            if (pixelsAlpha[c].a > 0.8f)
            {
                resultingPixels[c] = new Color(pixelsColor[c].r, pixelsColor[c].g, pixelsColor[c].b, 0.5f);
            }
            else
            {
                resultingPixels[c] = new Color(pixelsColor[c].r, pixelsColor[c].g, pixelsColor[c].b, pixelsAlpha[c].a);
            }
        }
        Texture2D result = new Texture2D(texture3.width, texture3.height);
        result.SetPixels(resultingPixels);
        result.Apply();
        this.sourcePaintMetalic = result;
    }

    private RenderTexture SetupRenderTexture(Texture baseTex)
    {
        RenderTexture rt = new RenderTexture(baseTex.width, baseTex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        rt.filterMode = baseTex.filterMode;
        rt.wrapMode = baseTex.wrapMode;

        Graphics.Blit(baseTex, rt);
        return rt;
    }

    private Texture2D toTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.ARGB32, false);
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0f, 0f, (float)rTex.width, (float)rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }

    private void OnCollisionStay(Collision collisionInfo)
    {
        Debug.Log("Found Collistion");
        int hitpointsCount = 0;
        ContactPoint[] contacts = collisionInfo.contacts;
        for (int k = 0; k < contacts.Length; k++)
        {
            ContactPoint cp = contacts[k];
            Debug.Log("Colliding with " + cp.otherCollider.name);
            PaintTextureMeshDataContainer ptoc = cp.otherCollider.gameObject.GetComponent<PaintTextureMeshDataContainer>();
            if (ptoc == null)
            {
                break;
            }
            Debug.Log("Painting");
            hitpointsCount++;
            if (hitpointsCount > 2)
            {
                break;
            }
            float currentfluidPoints2 = UnityEngine.Random.Range(this.maxFluidTime / 2f, this.maxFluidTime);
            float rayLength = 0.002f;
            Ray ray = new Ray(cp.point + cp.normal * rayLength * 0.5f, -cp.normal);
            Debug.DrawLine(cp.point + cp.normal, cp.point - cp.normal * rayLength, Color.red, 5f);
            RaycastHit[] hits;
            if ((hits = Physics.RaycastAll(ray, rayLength)) != null)
            {
                MeshCollider meshCollider = null;
                Vector2 pixelUV = Vector2.zero;

                for(int j = 0;j< hits.Length; j++)
                {
                    if ((cp.otherCollider == hits[j].collider))
                    {
                        meshCollider = (hits[j].collider as MeshCollider);
                        pixelUV = hits[j].textureCoord;
                        break;
                    }                    
                }
               
                Renderer rend = ptoc.GetComponent<Renderer>();
                if (rend != null && !(rend.sharedMaterial == null) && !(rend.sharedMaterial.mainTexture == null) && !(meshCollider == null))
                {
                    int addRandomFluid = UnityEngine.Random.Range(0, 100);
                    Material[] materials = rend.materials;
                    foreach (Material i in materials)
                    {
                        RenderTexture texMain = i.mainTexture as RenderTexture;
                        if (this.sourcePaintMain)
                        {
                            ptoc.SetPaintMainData(pixelUV, (float)addRandomFluid, this.startSize, ref this.sourcePaintMain, false,ref shaderToPaint);
                            ptoc.paintDataToTexture(texMain, this.shaderToPaint);
                        }
                        RenderTexture texNormal = i.GetTexture("_BumpMap") as RenderTexture;
                        if (this.sourcePaintNormal)
                        {
                            ptoc.SetPaintMainData(pixelUV, (float)addRandomFluid, this.startSize, ref this.sourcePaintNormal, true, ref shaderToPaint);
                            ptoc.paintDataToTexture(texNormal, this.shaderToPaint);
                        }
                        RenderTexture texMetalic = i.GetTexture("_MetallicGlossMap") as RenderTexture;
                        if (this.sourcePaintMetalic)
                        {
                            ptoc.SetPaintMainData(pixelUV, (float)addRandomFluid, this.startSize, ref this.sourcePaintMetalic, true, ref shaderToPaint);
                            ptoc.paintDataToTexture(texMetalic, this.shaderToPaint);
                        }
                        if (this.enableFluids && addRandomFluid > 93)
                        {
                            currentfluidPoints2 = Mathf.Min(currentfluidPoints2, this.maxFluidTime);
                            currentfluidPoints2 *= 0.8f;
                            ptoc.AddFuildData(ref texMain, ref texNormal, ref texMetalic, ref this.sourcePaintMain, ref this.sourcePaintNormal, ref this.sourcePaintMetalic,
                                ref shaderToPaint,this.startSize, pixelUV, (float)addRandomFluid, currentfluidPoints2);
                        }
                    }
                    continue;
                }
                break;
            }
        }
    }
}
