#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEditor;
using UnityEngine;

public class ToolEnumDllCreator : ScriptableWizard
{
    public List<String> ToolNames;
    //if is you let this empty will be saved on Assets/MAGES/ORScene/PlatformPlugins/Platform
    public string saveDirectory = "/Assets/MAGES/Plugins/LocalPlugins";

    [MenuItem ("MAGES/Tools/Create Tools Dll")]
    static void CreateWizard()
    {
        ToolEnumDllCreator t =ScriptableWizard.DisplayWizard<ToolEnumDllCreator>("Create Tools dll","Create DLL");
        t.ToolNames = new List<string>();
        foreach (ovidVR.toolManager.tool.ToolsEnum en in Enum.GetValues(typeof(ovidVR.toolManager.tool.ToolsEnum)))
        {
            t.ToolNames.Add(en.ToString());
        }
    }

    private void OnWizardCreate()
    {
        // Get the current application domain for the current thread
        AppDomain currentDomain = AppDomain.CurrentDomain;

        // Create a dynamic assembly in the current application domain,
        // and allow it to be executed and saved to disk.
        AssemblyName name = new AssemblyName("ToolsEnumDLL");
        AssemblyBuilder assemblyBuilder = currentDomain.DefineDynamicAssembly(name,
                                              AssemblyBuilderAccess.RunAndSave, System.Environment.CurrentDirectory + saveDirectory);

        // Define a dynamic module in "MyEnums" assembly.
        // For a single-module assembly, the module has the same name as the assembly.
        ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(name.Name,
                                          name.Name + ".dll");

        // Define a public enumeration with the name "MyEnum" and an underlying type of Integer.
        EnumBuilder myEnum = moduleBuilder.DefineEnum("ovidVR.toolManager.tool.ToolsEnum",
                                 TypeAttributes.Public, typeof(int));

        int currentEnumValue = 0;
        foreach(string s in ToolNames)
        {
            myEnum.DefineLiteral(s, currentEnumValue);
            ++currentEnumValue;
        }
        myEnum.CreateType();

        // Finally, save the assembly
        assemblyBuilder.Save(name.Name + ".dll");
    }
    
}

#endif