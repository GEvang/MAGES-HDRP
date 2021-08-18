using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// A class was created to represent scene objects
public class CutoutObject
{
    public GameObject gameobject; // The associated gameobject
    public bool cutoutEnabled;    // Whether or not the cutout effect for the object has been enabled
    public CutoutObject(GameObject go, bool state)
    {
        gameobject = go;
        cutoutEnabled = state;
    }
}

[System.Serializable]
public class ObjectToCutout
{
    public GameObject mainObject;
    public bool AffectChildObjects = false;

    public ObjectToCutout(GameObject mainObj, bool AffectChildren)
    {
        mainObject = mainObj;
        AffectChildObjects = AffectChildren;
    }
}

public class SphereCutout : MonoBehaviour
{
    private Vector3 upStep = new Vector3(0.003f, 0.003f, 0.003f);
    public float sphereScale = 1f;
    public float affectDistance = 15f;                                   // Affect distance
    public float sphereRadius;                                           // Sphere radius updated every frame
    public float borderRadius = 0.045f;                                  // Border radius updated every frame
    public float invertEffect = 0;                                       // Invert effect (either 0 or 1)
    public Shader theCutoutShader, theCutoutShaderSpecular, theCutoutShaderTransparent, theCutoutShaderUnlit, theCutoutShaderFade;   // A "place holder" for the cutout shader(s) in case it is missplaced
    public List<Material> CutoutMaterials = new List<Material>();
    List<CutoutObject> allObjects;
    [Tooltip("Add to this list all the gameobjects on which the cutout shader will operate.")]
    public List<ObjectToCutout> AffectedObjects;
    [Tooltip("Add to this list all the gameobjects, which the cutout shader will ignore.")]
    public List<GameObject> ExcludedObjects;


    public bool transparentShaderActive = false;

    void Awake()
    {
        GetCutoutShaders();
        //allObjects = GetSceneObjects();
    }

    private void Start()
    {
        //Enable();
    }

    void Update()
    {
        sphereRadius = GetSphereRadius();
        CheckObjectDistance();
        UpdateShaderValues();
    }

    public void Enable()
    {
        allObjects = GetSceneObjects();
        gameObject.transform.localScale = new Vector3(0, 0, 0);
        StartCoroutine(ScaleUp());
        
    }

    public void Disable()
    {
        StartCoroutine(ScaleDown());
    }

    IEnumerator ScaleUp()
    {
        while (gameObject.transform.localScale.x <= sphereScale)
        {
            gameObject.transform.localScale += upStep;
            yield return null;
        }
        yield return null;
    }

    IEnumerator ScaleDown()
    {
        while (gameObject.transform.localScale.x > 0.003f)
        {
            gameObject.transform.localScale -= upStep;
            yield return null;
        }
        yield return null;
    }

    // Finds and fills the shaders used for material replacements
    // In case a shader is not found, an error is logged.
    private void GetCutoutShaders()
    {
        if (theCutoutShader == null)
            if (Shader.Find("Custom/SphereCutout") != null)
                theCutoutShader = Shader.Find("Custom/SphereCutout");
            else
                Debug.LogError("Please add the SphereCutout.shader to script or add it under Assets/Resources/.");

        if (theCutoutShaderFade == null)
            if (Shader.Find("Custom/SphereCutoutFade") != null)
                theCutoutShaderFade = Shader.Find("Custom/SphereCutoutFade");
            else
                Debug.LogError("Please add the SphereCutoutFade.shader to script or add it under Assets/Resources/.");

        if (theCutoutShaderSpecular == null)
            if (Shader.Find("Custom/SphereCutoutSpecular") != null)
                theCutoutShaderSpecular = Shader.Find("Custom/SphereCutoutSpecular");
            else
                Debug.LogError("Please add the SphereCutoutSpecular.shader to script or add it under Assets/Resources/.");

        if (theCutoutShaderTransparent == null)
            if (Shader.Find("Custom/SphereCutoutTransparent") != null)
                theCutoutShaderTransparent = Shader.Find("Custom/SphereCutoutTransparent");
            else
                Debug.LogError("Please add the SphereCutoutTransparent.shader to script or add it under Assets/Resources/.");

        if (theCutoutShaderUnlit == null)
            if (Shader.Find("Custom/SphereCutoutUnlit") != null)
                theCutoutShaderUnlit = Shader.Find("Custom/SphereCutoutUnlit");
            else
                Debug.LogError("Please add the SphereCutout.shader to script or add it under Assets/Resources/.");
    }

    // Extension that allows the insertion of GameObjects during runtime (Giannis)
    public void AddRuntimeObject(GameObject mainObj, bool AffectChildren)
    {
        AffectedObjects.Add(new ObjectToCutout(mainObj, AffectChildren));
    }

    // Returns a list of all the objects in scene
    private List<CutoutObject> GetSceneObjects()
    {
        List<CutoutObject> objectsInScene = new List<CutoutObject>();
        //foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
        //    if (go.activeSelf == true && go != this.gameObject && go.tag!="gManager") //ignore the gamemanager
        //        objectsInScene.Add(new CutoutObject(go, false));

        // Extension that allows the insertion of GameObjects during runtime (Giannis)
      
        foreach (ObjectToCutout go in AffectedObjects)
        {
            if (go.mainObject != null)
            {
                if (!ExcludedObjects.Contains(go.mainObject))
                    objectsInScene.Add(new CutoutObject(go.mainObject, false));
                if (go.AffectChildObjects == true)
                {
                    if (go.mainObject.transform.childCount > 0)
                    {
                        foreach (Transform cgo in go.mainObject.GetComponentsInChildren<Transform>())
                        {
                            if (cgo != null)
                            {
                                if (!ExcludedObjects.Contains(cgo.gameObject))
                                    objectsInScene.Add(new CutoutObject(cgo.gameObject, false));
                            }
                        }
                    }
                }
            }
        }



        return objectsInScene;
    }


    // In every frame, update _Sphere_position and _Sphere_radius in the shader of affected materials
    // Meaning the materials will "know" where this sphere is and its radius and will produce a cutout effect when intersect.
    private void UpdateShaderValues()
    {
        foreach (Material mat in CutoutMaterials)
        {
            mat.SetVector("_Sphere_position", transform.position);
            mat.SetFloat("_Sphere_radius", sphereRadius);
            mat.SetFloat("_Border_radius", borderRadius);
            mat.SetFloat("_Invert", invertEffect);
        }
    }

    // Returns the sphere radius
    private float GetSphereRadius()
    {
        if (this.gameObject.transform.parent != null)
            return Mathf.Max(this.gameObject.transform.root.localScale.x * this.gameObject.transform.localScale.x, this.gameObject.transform.root.localScale.y * this.gameObject.transform.localScale.y);
        else
            return Mathf.Max(this.gameObject.transform.localScale.x, this.gameObject.transform.localScale.y);
    }

    // Checks scene objects distance relative to the sphere
    // If inside the affect distance, the material is changed for the cutout effect.
    private void CheckObjectDistance()
    {
        foreach (CutoutObject so in allObjects)
        {
            if (!so.cutoutEnabled)
            {
                //float distance = Vector3.Distance(so.gameobject.transform.position, this.transform.position);
                // if (distance < affectDistance) // If under the affect distance
                // {
                    so.cutoutEnabled = true; // Cutout effect is enabled
                    
                    AddToMaterials(so); // Material is added to the affected materials list (and properties are copied)
                // }
            }
        }
    }

    // Object material is added to the observed material list
    private void AddToMaterials(CutoutObject ob)
    {
        if (ob.gameobject.GetComponent<Renderer>() != null)
            if (ob.gameobject.GetComponent<Renderer>().material != null)
                CopyAllShaderProperties(ob.gameobject);
    }

    // Replaces a material with a CutoutShader material (if needed) and adds it to the affected list
    private void CopyAllShaderProperties(GameObject obj)
    {
        // For each of the original materials binded to the GameObject,
        // fill a cache array with a custom cutout material
        int iter = 0;
        Material[] cachedMaterials = new Material[obj.GetComponent<Renderer>().materials.Length];
        foreach (Material mat in obj.GetComponent<Renderer>().materials)
        {
            if (mat.shader.name == "SphereCutout")
                cachedMaterials[iter] = mat; // No need to copy properties if object is already using the SphereCutout shader
            else

            if (transparentShaderActive && ((mat.shader.name.Contains("Transparent")) || (mat.GetFloat("_Mode") == 3.0)))
            {
                cachedMaterials[iter] = CopyStandardTransparent(mat);
            }
            else
            if(mat.GetFloat("_Mode") == 2.0)
            {
                cachedMaterials[iter] = CopyFade(mat);
            }
            else
            if (mat.shader.name.Contains("Specular"))
            {
                cachedMaterials[iter] = CopyStandardSpecular(mat);
            }
            else
            if (mat.shader.name.Contains("Unlit"))
            {
                cachedMaterials[iter] = CopyUnlit(mat);
            }
            else
            if (mat.GetFloat("_Mode") < 2.0)
            {
                cachedMaterials[iter] = CopyStandard(mat);
            }
            else
            {
                cachedMaterials[iter] = mat;
            }
            iter++;
        }
        //Replace the original material array with the new cutout materials array & add to list
        obj.GetComponent<Renderer>().materials = cachedMaterials;
        CutoutMaterials.AddRange(obj.GetComponent<Renderer>().materials);
    }

    //Copies all properties of a Standard shader material and returns a new SphereCutout shader material
    private Material CopyStandard(Material mat)
    {
        // Initialise an instance material of the SphereCutout shader.
        Material newmat = new Material(theCutoutShader);
        newmat.name = "CutOut_" + mat.name;

        // Check, get & map all properties of standard shader from original material to the new material! 
        //~ at least those I have implemented in cutout shader so far..

        // Albedo
        if (mat.HasProperty("_MainTex"))
        {
            newmat.SetTexture("_Albedo", mat.GetTexture("_MainTex"));
            if(!mat.GetTexture("_MainTex")){
                newmat.SetColor("_Color", mat.GetColor("_Color"));
            }
        }
        // Albedo tint
        if (mat.HasProperty("_Color") && mat.HasProperty("_MainTex"))
            newmat.SetColor("_Albedo_tint", mat.GetColor("_Color"));

        // Normal map
        if (mat.HasProperty("_BumpMap"))
            if (mat.GetTexture("_BumpMap") != null)
                newmat.SetTexture("_Normal", mat.GetTexture("_BumpMap"));
            else if (mat.HasProperty("_DetailNormalMap"))
                newmat.SetTexture("_Normal", mat.GetTexture("_DetailNormalMap"));

        // Metallic
        if (mat.HasProperty("_MetallicGlossMap"))
        {
            if (mat.GetTexture("_MetallicGlossMap") != null)
            {

                newmat.SetTexture("_Metallic", mat.GetTexture("_MetallicGlossMap"));
                if (mat.HasProperty("_Metallic"))                                               // Metallic multiplier
                    newmat.SetFloat("_Metallic_multiplier", mat.GetFloat("_Metallic") * 10);
                if (mat.HasProperty("_Glossiness"))                                              // Smoothness
                    newmat.SetFloat("_Smoothness", mat.GetFloat("_Glossiness") / 2);             // Metallic alpha channel
            }
            else
            {
                if (mat.HasProperty("_Metallic"))                                                // Metallic multiplier
                    newmat.SetFloat("_Metallic_multiplier", mat.GetFloat("_Metallic"));
                if (mat.HasProperty("_Glossiness"))                                              // Smoothness
                    newmat.SetFloat("_Smoothness", mat.GetFloat("_Glossiness"));                 // Metallic/Albedo alpha channel with no texture
            }
        }

        // Roughness Glossiness
        if (mat.shader.name.Contains("Rough"))
            if (mat.HasProperty("_Glossiness"))
                newmat.SetFloat("_Smoothness", mat.GetFloat("_Glossiness") * 2); // Metallic alpha channel

        // Occlusion Map
        if (mat.HasProperty("_OcclusionMap"))
            if (mat.GetTexture("_OcclusionMap") != null)
                newmat.SetTexture("_Occlusion", mat.GetTexture("_OcclusionMap"));   // Occlusion  

        // Occlusion Strength
        if (mat.HasProperty("_OcclusionStrength"))
            newmat.SetFloat("_Occlusion_strength", mat.GetFloat("_OcclusionStrength"));


        // Emission map
        if (mat.HasProperty("_EmissionMap"))
            newmat.SetTexture("_Emission", mat.GetTexture("_EmissionMap"));

        // Emission tint
        if (mat.HasProperty("_EmissionColor"))
            newmat.SetColor("_Emission_tint", mat.GetColor("_EmissionColor"));

        // Cutoff
        if (mat.HasProperty("_Cutoff"))
            newmat.SetFloat("_Cutoff", mat.GetFloat("_Cutoff"));

        // Tiling
        if (mat.mainTextureScale != null)
            newmat.SetVector("_Tiling", new Vector4(mat.mainTextureScale.x, mat.mainTextureScale.y));

        // Offset
        if (mat.mainTextureOffset != null)
            newmat.SetVector("_Offset", new Vector4(mat.mainTextureOffset.x, mat.mainTextureOffset.y));

        //Apply cutout-sphere related properties
        newmat.SetFloat("_Sphere_radius", sphereRadius);
        newmat.SetFloat("_Border_radius", 0.15f);
        newmat.SetColor("_Border_color", Color.red);

        //Fill the cache array
        return newmat;

    }

    //Copies all properties of a Standard shader material and returns a new SphereCutout shader material
    private Material CopyFade(Material mat)
    {
        // Initialise an instance material of the SphereCutout shader.
        Material newmat = new Material(theCutoutShaderFade);
        newmat.name = "CutOut_" + mat.name;

        // Check, get & map all properties of standard shader from original material to the new material! 
        //~ at least those I have implemented in cutout shader so far..

        // Albedo
        if (mat.HasProperty("_MainTex"))
            newmat.SetTexture("_MainTex", mat.GetTexture("_MainTex"));

  
        // Albedo tint
        if (mat.HasProperty("_Color"))
            newmat.SetColor("_Color", mat.GetColor("_Color"));

        // Normal map
        if (mat.HasProperty("_BumpMap"))
            if (mat.GetTexture("_BumpMap") != null)
                newmat.SetTexture("_Normal", mat.GetTexture("_BumpMap"));
            else if (mat.HasProperty("_DetailNormalMap"))
                newmat.SetTexture("_Normal", mat.GetTexture("_DetailNormalMap"));

        // Metallic
        if (mat.HasProperty("_MetallicGlossMap"))
        {
            if (mat.GetTexture("_MetallicGlossMap") != null)
            {

                newmat.SetTexture("_Metallic", mat.GetTexture("_MetallicGlossMap"));
                if (mat.HasProperty("_Metallic"))                                               // Metallic multiplier
                    newmat.SetFloat("_Metallic_multiplier", mat.GetFloat("_Metallic") * 10);
                if (mat.HasProperty("_Glossiness"))                                              // Smoothness
                    newmat.SetFloat("_Smoothness", mat.GetFloat("_Glossiness") / 2);             // Metallic alpha channel
            }
            else
            {
                if (mat.HasProperty("_Metallic"))                                                // Metallic multiplier
                    newmat.SetFloat("_Metallic_multiplier", mat.GetFloat("_Metallic"));
                if (mat.HasProperty("_Glossiness"))                                              // Smoothness
                    newmat.SetFloat("_Smoothness", mat.GetFloat("_Glossiness"));                 // Metallic/Albedo alpha channel with no texture
            }
        }

        // Roughness Glossiness
        if (mat.shader.name.Contains("Rough"))
            if (mat.HasProperty("_Glossiness"))
                newmat.SetFloat("_Smoothness", mat.GetFloat("_Glossiness") * 2); // Metallic alpha channel

        // Occlusion Map
        if (mat.HasProperty("_OcclusionMap"))
            if (mat.GetTexture("_OcclusionMap") != null)
                newmat.SetTexture("_Occlusion", mat.GetTexture("_OcclusionMap"));   // Occlusion  

        // Occlusion Strength
        if (mat.HasProperty("_OcclusionStrength"))
            newmat.SetFloat("_Occlusion_strength", mat.GetFloat("_OcclusionStrength"));


        // Emission map
        if (mat.HasProperty("_EmissionMap"))
            newmat.SetTexture("_Emission", mat.GetTexture("_EmissionMap"));

        // Emission tint
        if (mat.HasProperty("_EmissionColor"))
            newmat.SetColor("_Emission_tint", mat.GetColor("_EmissionColor"));

        // Cutoff
        if (mat.HasProperty("_Cutoff"))
            newmat.SetFloat("_Cutoff", mat.GetFloat("_Cutoff"));

        // Tiling
        if (mat.mainTextureScale != null)
            newmat.SetVector("_Tiling", new Vector4(mat.mainTextureScale.x, mat.mainTextureScale.y));

        // Offset
        if (mat.mainTextureOffset != null)
            newmat.SetVector("_Offset", new Vector4(mat.mainTextureOffset.x, mat.mainTextureOffset.y));

        //Apply cutout-sphere related properties
        newmat.SetFloat("_Sphere_radius", sphereRadius);
        newmat.SetFloat("_Border_radius", 0.15f);
        newmat.SetColor("_Border_color", Color.red);
        newmat.renderQueue = mat.renderQueue + 1;
        //Fill the cache array
        return newmat;

    }

    //Copies all properties of a Standard Specular shader material and returns a new SphereCutoutSpecular shader material
    private Material CopyStandardSpecular(Material mat)
    {
        //Initialise an instance material of the SphereCutout shader.
        Material newmat = new Material(theCutoutShaderSpecular);
        newmat.name = "CutOutSpecular_" + mat.name;

        //Check, get & map all properties of standard shader from original material to the new material! 
        //~ at least those I have implemented in cutout shader so far..

        // Albedo
        if (mat.HasProperty("_MainTex"))
            newmat.SetTexture("_Albedo", mat.GetTexture("_MainTex"));

        // Albedo tint
        if (mat.HasProperty("_Color"))
            newmat.SetColor("_Albedo_tint", mat.GetColor("_Color"));

        // Normal map
        if (mat.HasProperty("_BumpMap"))
            if (mat.GetTexture("_BumpMap") != null)
                newmat.SetTexture("_Normal", mat.GetTexture("_BumpMap"));
            else if (mat.HasProperty("_DetailNormalMap"))
                newmat.SetTexture("_Normal", mat.GetTexture("_DetailNormalMap"));

        // Specular
        if (mat.HasProperty("_SpecGlossMap"))
        {
            if (mat.GetTexture("_SpecGlossMap") != null)
                newmat.SetTexture("_Specular", mat.GetTexture("_SpecGlossMap"));

            if (mat.HasProperty("_GlossMapScale"))      // Specular Shiness
                newmat.SetFloat("_Smoothness", mat.GetFloat("_GlossMapScale"));

            if (mat.HasProperty("_Glossiness"))                                 // Smoothness
                newmat.SetFloat("_Shininess", mat.GetFloat("_Glossiness"));
        }

        // Occlusion Map
        if (mat.HasProperty("_OcclusionMap"))
            if (mat.GetTexture("_OcclusionMap") != null)
                newmat.SetTexture("_Occlusion", mat.GetTexture("_OcclusionMap"));   // Occlusion  

        // Occlusion Strength
        if (mat.HasProperty("_OcclusionStrength"))
            newmat.SetFloat("_Occlusion_strength", mat.GetFloat("_OcclusionStrength"));


        // Emission map
        if (mat.HasProperty("_EmissionMap"))
            newmat.SetTexture("_Emission", mat.GetTexture("_EmissionMap"));

        // Emission tint
        if (mat.HasProperty("_EmissionColor"))
            newmat.SetColor("_Emission_tint", mat.GetColor("_EmissionColor"));

        // Cutoff
        if (mat.HasProperty("_Cutoff"))
            newmat.SetFloat("_Cutoff", mat.GetFloat("_Cutoff"));

        // Tiling
        if (mat.mainTextureScale != null)
            newmat.SetVector("_Tiling", new Vector4(mat.mainTextureScale.x, mat.mainTextureScale.y));

        // Offset
        if (mat.mainTextureOffset != null)
            newmat.SetVector("_Offset", new Vector4(mat.mainTextureOffset.x, mat.mainTextureOffset.y));

        //Apply cutout-sphere related properties
        newmat.SetFloat("_Sphere_radius", sphereRadius);
        newmat.SetFloat("_Border_radius", 0.15f);
        newmat.SetColor("_Border_color", Color.red);

        //Fill the cache array
        return newmat;

    }

    //Copies all properties of a Standard Transparent mode shader material and returns a new SphereCutoutSpecular shader material
    private Material CopyStandardTransparent(Material mat)
    {

        //Initialise an instance material of the SphereCutout shader.
        Material newmat = new Material(theCutoutShaderTransparent);
        newmat.name = "CutOutTransparent_" + mat.name;

        //Check, get & map all properties of standard shader from original material to the new material! 
        //~ at least those I have implemented in cutout shader so far..

        // Albedo
        if (mat.HasProperty("_MainTex"))
            newmat.SetTexture("_Albedo", mat.GetTexture("_MainTex"));

        // Albedo tint
        if (mat.HasProperty("_Color"))
            newmat.SetColor("_Albedo_tint", mat.GetColor("_Color"));

        // Normal map
        if (mat.HasProperty("_BumpMap"))
            if (mat.GetTexture("_BumpMap") != null)
                newmat.SetTexture("_Normal", mat.GetTexture("_BumpMap"));
            else if (mat.HasProperty("_DetailNormalMap"))
                newmat.SetTexture("_Normal", mat.GetTexture("_DetailNormalMap"));

        // Metallic
        if (mat.HasProperty("_MetallicGlossMap"))
        {
            if (mat.GetTexture("_MetallicGlossMap") != null)
            {

                newmat.SetTexture("_Metallic", mat.GetTexture("_MetallicGlossMap"));
                if (mat.HasProperty("_Metallic"))           // Metallic multiplier
                    newmat.SetFloat("_Metallic_multiplier", mat.GetFloat("_Metallic") * 10);
                if (mat.HasProperty("_Glossiness"))         // Smoothness
                    newmat.SetFloat("_Smoothness", mat.GetFloat("_Glossiness") / 2); // Metallic alpha channel
            }
            else
            {
                if (mat.HasProperty("_Metallic"))           // Metallic multiplier
                    newmat.SetFloat("_Metallic_multiplier", mat.GetFloat("_Metallic"));
                if (mat.HasProperty("_Glossiness"))         // Smoothness
                    newmat.SetFloat("_Smoothness", mat.GetFloat("_Glossiness"));  // Metallic/Albedo alpha channel with no texture
            }
        }

        // Occlusion Map
        if (mat.HasProperty("_OcclusionMap"))
            if (mat.GetTexture("_OcclusionMap") != null)
                newmat.SetTexture("_Occlusion", mat.GetTexture("_OcclusionMap"));   // Occlusion  

        // Occlusion Strength
        if (mat.HasProperty("_OcclusionStrength"))
            newmat.SetFloat("_Occlusion_strength", mat.GetFloat("_OcclusionStrength"));


        // Emission map
        if (mat.HasProperty("_EmissionMap"))
            newmat.SetTexture("_Emission", mat.GetTexture("_EmissionMap"));

        // Emission tint
        if (mat.HasProperty("_EmissionColor"))
            newmat.SetColor("_Emission_tint", mat.GetColor("_EmissionColor"));

        // Cutoff
        if (mat.HasProperty("_Cutoff"))
            newmat.SetFloat("_Cutoff", mat.GetFloat("_Cutoff"));

        // Tiling
        if (mat.mainTextureScale != null)
            newmat.SetVector("_Tiling", new Vector4(mat.mainTextureScale.x, mat.mainTextureScale.y));

        // Offset
        if (mat.mainTextureOffset != null)
            newmat.SetVector("_Offset", new Vector4(mat.mainTextureOffset.x, mat.mainTextureOffset.y));

        //Apply cutout-sphere related properties
        newmat.SetFloat("_Sphere_radius", sphereRadius);
        newmat.SetFloat("_Border_radius", 0.15f);
        newmat.SetColor("_Border_color", Color.red);

        //Fill the cache array
        return newmat;
    }

    //Copies all properties of an Unlit shader material and returns a new SphereCutoutUnlit shader material
    private Material CopyUnlit(Material mat)
    {
        // Initialise an instance material of the SphereCutout shader.
        Material newmat = new Material(theCutoutShaderUnlit);
        newmat.name = "CutOutUnlit_" + mat.name;

        // Check, get & map all properties of standard shader from original material to the new material! 
        //~ at least those I have implemented in cutout shader so far..

        // Albedo
        if (mat.HasProperty("_MainTex"))
            newmat.SetTexture("_Albedo", mat.GetTexture("_MainTex"));

        // Albedo tint
        if (mat.HasProperty("_Color"))
            newmat.SetColor("_Albedo_tint", mat.GetColor("_Color"));
        // Cutoff
        if (mat.HasProperty("_Cutoff"))
            newmat.SetFloat("_Cutoff", mat.GetFloat("_Cutoff"));

        // Tiling
        if (mat.mainTextureScale != null)
            newmat.SetVector("_Tiling", new Vector4(mat.mainTextureScale.x, mat.mainTextureScale.y));

        // Offset
        if (mat.mainTextureOffset != null)
            newmat.SetVector("_Offset", new Vector4(mat.mainTextureOffset.x, mat.mainTextureOffset.y));

        //Apply cutout-sphere related properties
        newmat.SetFloat("_Sphere_radius", sphereRadius);
        newmat.SetFloat("_Border_radius", 0.15f);
        newmat.SetColor("_Border_color", Color.red);

        //Fill the cache array
        return newmat;

    }


}
