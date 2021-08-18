using System;
using System.Collections.Generic;
using UnityEngine;

public class PaintTextureMeshDataContainer : MonoBehaviour
{

    [Serializable]
    public class CollisionInfo
    {
        public string colliderTag;
        public string colliderName;
    };

    public CollisionInfo collisionInformation;

    [SerializeField]
    bool enableFluids = false;

    [SerializeField]
    private int maxFluidCapacity = 10;

    private Color blendColor = Color.white;
    private LayerMask physIgnoreLayer;
    #region ShaderPropertyID

    private int paintUVPropertyID;
    private int brushTexturePropertyID;
    private int brushScalePropertyID;
    private int brushRotatePropertyID;
    private int brushColorPropertyID;
    private int brushNormalTexturePropertyID;
    private int brushNormalBlendPropertyID;
    private int brushHeightTexturePropertyID;
    private int brushHeightBlendPropertyID;
    private int brushHeightColorPropertyID;

    #endregion ShaderPropertyID

    [SerializeField]
    private Queue<RenderTexture> resetMainTextureData;
    [SerializeField]
    private Queue<RenderTexture> resetNormalTextureData;
    [SerializeField]
    private Queue<RenderTexture> resetMetalicTextureData;

    private void Start()
    {
        physIgnoreLayer = LayerMask.NameToLayer("ToolsOFF");

        InitPropertyID();       

        resetMainTextureData = new Queue<RenderTexture>();
        resetNormalTextureData = new Queue<RenderTexture>();
        resetMetalicTextureData = new Queue<RenderTexture>();

        foreach (Material m in this.GetComponent<Renderer>().materials)
        {

            if(m.mainTexture)
            {
                var mainTex = SetupRenderTexture(m.mainTexture);
                m.mainTexture = mainTex;
                resetMainTextureData.Enqueue(SetupRenderTexture(m.mainTexture));
            }
            if(m.GetTexture("_BumpMap"))
            {
                var normalTex = SetupRenderTexture(m.GetTexture("_BumpMap"));
                m.SetTexture("_BumpMap", normalTex);
                resetNormalTextureData.Enqueue(SetupRenderTexture(m.GetTexture("_BumpMap")));
            }
            if(m.GetTexture("_MetallicGlossMap"))
            {
                var metalicTex = SetupRenderTexture(m.GetTexture("_MetallicGlossMap"));
                m.SetTexture("_MetallicGlossMap", metalicTex);
                resetMetalicTextureData.Enqueue(SetupRenderTexture(m.GetTexture("_MetallicGlossMap")));
            }
            //

        }       
    }

    public void ResetAllTextures()
    {
        foreach (Material m in this.GetComponent<Renderer>().materials)
        {
            m.mainTexture = resetMainTextureData.Dequeue();
            m.SetTexture("_BumpMap", resetNormalTextureData.Dequeue());
            m.SetTexture("_MetallicGlossMap", resetMetalicTextureData.Dequeue());
        }
        Start();
    }

    private RenderTexture SetupRenderTexture(Texture baseTex)
    {
        var rt = new RenderTexture(baseTex.width, baseTex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        rt.filterMode = baseTex.filterMode;
        rt.wrapMode = baseTex.wrapMode;

        Graphics.Blit(baseTex, rt);
        return rt;
    }

    private void InitPropertyID()
    {
        paintUVPropertyID = Shader.PropertyToID("_PaintUV");
        brushTexturePropertyID = Shader.PropertyToID("_Brush");
        brushScalePropertyID = Shader.PropertyToID("_BrushScale");
        brushRotatePropertyID = Shader.PropertyToID("_BrushRotate");
        brushColorPropertyID = Shader.PropertyToID("_ControlColor");
        brushNormalTexturePropertyID = Shader.PropertyToID("_BrushNormal");
        brushNormalBlendPropertyID = Shader.PropertyToID("_NormalBlend");
        brushHeightTexturePropertyID = Shader.PropertyToID("_BrushHeight");
        brushHeightBlendPropertyID = Shader.PropertyToID("_HeightBlend");
        brushHeightColorPropertyID = Shader.PropertyToID("_Color");
    }

    public void AddFuildData(ref RenderTexture texMain, ref RenderTexture texNormal, ref RenderTexture texMetalic,
        ref Texture _sourcePaintMain, ref Texture _sourcePaintNormal, ref Texture _sourcePaintMetalic,
        ref Material shaderToPaint,
        float _startSize,Vector2 _pixelUV,float _addRandomFluid,float _fluidTime)
    {
        paintFluidData pfd = new paintFluidData(ref texMain, ref texNormal, ref texMetalic,
             ref _sourcePaintMain, ref _sourcePaintNormal, ref _sourcePaintMetalic, 
             shaderToPaint,
            _startSize, _pixelUV, _addRandomFluid);
        pfd.fluidTime = _fluidTime;
        paintFluidDataList.Add(pfd);
    }

    public void paintDataToTexture(RenderTexture tex, Material m)
    {
        if (tex == null)
            return;

        var mainPaintTextureBuffer = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        Graphics.Blit(tex, mainPaintTextureBuffer, m);
        Graphics.Blit(mainPaintTextureBuffer, tex);
        RenderTexture.ReleaseTemporary(mainPaintTextureBuffer);
        RenderTexture.active = null;
    }

    private class paintFluidData
    {

        public Texture sourcePaintMain = null;
        public Texture sourcePaintNormal = null;
        public Texture sourcePaintMetalic = null;

        public RenderTexture paintTex;
        public RenderTexture paintNormalTex;
        public RenderTexture paintMetalicTex;
        public Material shaderToPaint;


        public Vector2 UV;
        public float paintedTime = 0.0f;
        public float currScale;
        public float fluidTime = 1.5f;
        public float rotation = 0.0f;
        public paintFluidData(ref RenderTexture _paintTex, ref RenderTexture _paintNormalTex, ref RenderTexture _paintMetalicTex,
                                  ref Texture _sourcePaintMain, ref Texture _sourcePaintNormal, ref Texture _sourcePaintMetalic,
                                  Material mat_to_paint,
                                  float _startSize, Vector2 _UV, float _rotation)
        {
            paintTex = _paintTex;
            paintNormalTex = _paintNormalTex;
            paintMetalicTex = _paintMetalicTex;

            sourcePaintMain = _sourcePaintMain;
            sourcePaintNormal = _sourcePaintNormal;
            sourcePaintMetalic = _sourcePaintMetalic;

            shaderToPaint = mat_to_paint;

            currScale = _startSize;
            UV = _UV;
            paintedTime = 0.0f;
            rotation = _rotation;
        }
    }
    List<paintFluidData> paintFluidDataList = new List<paintFluidData>();
    List<paintFluidData> toRemove = new List<paintFluidData>();

    private void Update()
    {
        if (enableFluids == false)
            return;
        int fluidCounter = 0;
        foreach (paintFluidData pfd in paintFluidDataList)
        {
            ++fluidCounter;
            if (fluidCounter > maxFluidCapacity)
            {
                break;
            }
            pfd.paintedTime += Time.deltaTime;
            pfd.currScale *= 1.0072f;
            if (pfd.paintedTime >= pfd.fluidTime)
            {
                toRemove.Add(pfd);
            }
            if (pfd.sourcePaintMain)
            {
                SetPaintMainData(pfd.UV, pfd.rotation, pfd.currScale,ref pfd.sourcePaintMain, false,ref pfd.shaderToPaint);
                paintDataToTexture(pfd.paintTex, pfd.shaderToPaint);
            }
            if (pfd.sourcePaintNormal)
            {
                SetPaintMainData(pfd.UV, pfd.rotation, pfd.currScale,ref pfd.sourcePaintNormal, true, ref pfd.shaderToPaint);
                paintDataToTexture(pfd.paintNormalTex, pfd.shaderToPaint);
            }
            if (pfd.sourcePaintMetalic)
            {
                SetPaintMainData(pfd.UV, pfd.rotation, pfd.currScale,ref pfd.sourcePaintMetalic, true, ref pfd.shaderToPaint);
                paintDataToTexture(pfd.paintMetalicTex, pfd.shaderToPaint);
            }

        }

        foreach (paintFluidData pfd in toRemove)
        {
            paintFluidDataList.Remove(pfd);
        }
        toRemove.Clear();
    }

    public void SetPaintMainData(Vector2 uv, float rotation, float scale,ref Texture texToPaint, bool blend, ref Material shaderToPaint)
    {
        shaderToPaint.SetVector("_PaintUV", new Vector4(uv.x, uv.y, 0.0f, 1.0f));
        shaderToPaint.SetTexture(brushTexturePropertyID, texToPaint);
        shaderToPaint.SetFloat(brushScalePropertyID, scale);
        shaderToPaint.SetFloat(brushRotatePropertyID, rotation);
        shaderToPaint.SetVector(brushColorPropertyID, blendColor);

        foreach (var key in shaderToPaint.shaderKeywords)
            shaderToPaint.DisableKeyword(key);

        if (blend)
            shaderToPaint.EnableKeyword("INK_PAINTER_COLOR_BLEND_USE_BRUSH");
        else
            shaderToPaint.EnableKeyword("INK_PAINTER_COLOR_BLEND_USE_BRUSH");

    }

    

    private Texture2D toTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.ARGB32, false);
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }

}
