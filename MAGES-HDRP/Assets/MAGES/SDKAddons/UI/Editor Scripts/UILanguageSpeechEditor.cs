/**
 * Typical UI Location: Resources/MAGESres/UI/
 * Typical font: SegoeUI
 * 
 * Searches all Prefabs inside the folder given. Seaches all childer of each prefab
 * and if it has a Text component it applies the Font Given. This algorithm assumes
 * all prefabs are GameObjects in the typecast
 **/

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ovidVR.UIManagement.LanguageText;
using System.IO;
using ovidVR.UIManagement;

using System.Reflection;
using System.Reflection.Emit;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

#if UNITY_EDITOR
public class UILanguageSpeechEditor : EditorWindow{

    private static string resourcesSpeechDirectory = "Sounds/" + Configuration.ProductCode + "/Speech/";

    public string saveDirectory = "/Assets/Plugins/LocalPlugins";

    private static string displayMessage = "Waiting for dev's input...";

    private static string[] allEnumLanguages;

    private static List<string> allDLLKeys;
    private static List<string> inputKeys;
    private static bool deleteConfirmation = false, showNumInputKeys = false;
    private static int totalInputKeys = 0, totalDeletionKeys = 0;

    private static List<LanguageSpeech> keyDLLSelections;
    private static List<string> deletedKeys;

    private static ApplicationLanguages langDisplay;
    private static LanguageSpeech speechDisplay;

    [MenuItem("MAGES/UIs/Language Speech")]
    public static void ShowWindow()
    {
        InitializeWindowValues();  

        //Show existing window instance. If one doesn't exist, make one.
        GetWindow(typeof(UILanguageSpeechEditor)); 
    }

    private static void InitializeWindowValues()
    {
        allEnumLanguages = Enum.GetNames(typeof(ApplicationLanguages));

        keyDLLSelections = new List<LanguageSpeech>();

        allDLLKeys = new List<string>();
        foreach (LanguageSpeech ls in Enum.GetValues(typeof(LanguageSpeech)))
            allDLLKeys.Add(ls.ToString());

        inputKeys = new List<string>();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Add or delete Keys for multiple-language spoken dialoges (Creates only files automatically per key)");
        EditorGUILayout.LabelField("NOTICE: A new key auto-adds a folder with its name and internally a fodler for each language,");
        EditorGUILayout.LabelField("then the developer must place the different dialoges into the corresponding folders. DO NOT change their names.");
        EditorGUILayout.LabelField("CAUTION: By deleting (or replacing) a DLL key, count also its corresponding file with all the components inside!");
        EditorGUILayout.LabelField("-----------------------------------------------");

        // Initialize from DLL
        if (allDLLKeys == null)
            InitializeWindowValues();

        if (allDLLKeys.Count == 0)
            UpdateLanguageDLL();

        langDisplay = (ApplicationLanguages)EditorGUILayout.EnumPopup("All available languages:", langDisplay);
        speechDisplay = (LanguageSpeech)EditorGUILayout.EnumPopup("All current Keys in use:", speechDisplay);

        EditorGUILayout.LabelField("STATUS UPDATE: " + displayMessage);
        EditorGUILayout.LabelField("-----------------------------------------------");

        totalInputKeys = EditorGUILayout.IntField("Num of new Keys: ", totalInputKeys);
        showNumInputKeys = EditorGUILayout.Toggle("Show Key input fields: ", showNumInputKeys);

        // NEW KEYS START -------------------------

        if (showNumInputKeys)
        {
            showNumInputKeys = false;

            inputKeys.Clear();

            for (int i = 0; i < totalInputKeys; ++i)
                inputKeys.Add("");
        }

        if (inputKeys.Count == totalInputKeys && totalInputKeys > 0)
        {
            for (int i = 0; i < totalInputKeys; ++i)
            {
                EditorGUILayout.LabelField("- Key: " + i.ToString());
                inputKeys[i] = EditorGUILayout.TextField("New Speech Key: ", inputKeys[i]);
            }

            if (GUILayout.Button("Append to DLL"))
            {
                if (GetAreKeysInUse())
                    return;

                int keysAdded = UpdateLanguageDLL(inputKeys);
                displayMessage = "SUCCESSFUL ENTRY, Added: " + keysAdded.ToString() + " out of: " + inputKeys.Count.ToString() + " key inputs. Wait for DLL built.";
                UpdateDirectoriesFromDLL(inputKeys);
                AssetDatabase.Refresh();
            }
        }

        // NEW KEYS END ---------------------------

        EditorGUILayout.LabelField("-----------------------------------------------");

        totalDeletionKeys = EditorGUILayout.IntField("Num of deletion Keys: ", totalDeletionKeys);
        deleteConfirmation = EditorGUILayout.Toggle("Show Key deletion fields: ", deleteConfirmation);

        if (deleteConfirmation)
        {
            deleteConfirmation = false;

            keyDLLSelections.Clear();

            for (int i = 0; i < totalDeletionKeys; ++i)
                keyDLLSelections.Add((LanguageSpeech)i);
        }

        if (keyDLLSelections.Count == totalDeletionKeys && totalDeletionKeys > 0)
        {
            if (keyDLLSelections.Count >= allDLLKeys.Count)
            {
                displayMessage = "SELECTION FAILED, cannot delete all enums! at least one must remain";
                keyDLLSelections.Clear();
                totalDeletionKeys = 0;
                return;
            }

            deletedKeys = new List<string>();

            for (int i = 0; i < totalDeletionKeys; ++i)
            {
                keyDLLSelections[i] = (LanguageSpeech)EditorGUILayout.EnumPopup("Select Key for deletion: ", keyDLLSelections[i]);
                deletedKeys.Add(keyDLLSelections[i].ToString());
            }

            if (GUILayout.Button("Delete Selected Key(s)?"))
            {
                DeleteKeysFromDLL();
                UpdateDirectoriesFromDLL(deletedKeys, false);
                AssetDatabase.Refresh();
            }
        }
    }
    
    private bool GetAreKeysInUse()
    {
        int keyIndex = 0;
        foreach(string s in inputKeys)
        {
            string key = s.Replace(" ", string.Empty);
            key = key.Replace("\n", string.Empty);
            key = key.Replace("\r", string.Empty);
            key = key.Replace("\t", string.Empty);

            if (allDLLKeys.Contains(key))
            {
                displayMessage = "FAILED ENTRY, Key " + keyIndex.ToString() + " already exists!";
                return true;
            }

            ++keyIndex;
        } 

        return false;
    }

    private int UpdateLanguageDLL(List<string> _addedKeys = null)
    {
        string speechFileFullPath = Environment.CurrentDirectory + "/Assets/Resources/" + resourcesSpeechDirectory;

        if (!Directory.Exists(speechFileFullPath))
            Directory.CreateDirectory(speechFileFullPath);

        AppDomain currentDomain = AppDomain.CurrentDomain;

        // Create a dynamic assembly in the current application domain,
        // and allow it to be executed and saved to disk.
        AssemblyName name = new AssemblyName("LanguageSpeechDLL");
        AssemblyBuilder assemblyBuilder = currentDomain.DefineDynamicAssembly(name,
            AssemblyBuilderAccess.RunAndSave, Environment.CurrentDirectory + saveDirectory);

        // Define a dynamic module in "MyEnums" assembly.
        // For a single-module assembly, the module has the same name as the assembly.
        ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(name.Name, name.Name + ".dll");

        // Define a public enumeration with the name "MyEnum" and an underlying type of Integer.
        EnumBuilder myEnum = moduleBuilder.DefineEnum("ovidVR.UIManagement.LanguageSpeech", TypeAttributes.Public, typeof(int));

        int totalAddedKeys = 0;
        if (_addedKeys != null && _addedKeys.Count > 0)
        {
            foreach(string s in _addedKeys)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    string key = s.Replace(" ", string.Empty);
                    key = key.Replace("\n", string.Empty);
                    key = key.Replace("\r", string.Empty);
                    key = key.Replace("\t", string.Empty);

                    allDLLKeys.Add(key);
                    ++totalAddedKeys;
                }
            }
        }

        int currentEnumValue = 0;
        foreach (string s in allDLLKeys)
        {
            myEnum.DefineLiteral(s, currentEnumValue);
            ++currentEnumValue;
        }
        myEnum.CreateType();

        // Finally, save the assembly
        assemblyBuilder.Save(name.Name + ".dll");

        return totalAddedKeys;
    }

    private void DeleteKeysFromDLL()
    {
        bool failedKey = false, failedJson = false;

        foreach (LanguageSpeech ls in keyDLLSelections)
        {
            // Delete keys from DLL
            if (allDLLKeys.Contains(ls.ToString()))
                allDLLKeys.Remove(ls.ToString());
            else
            {
                displayMessage = "FAILED SPECIFIC KEY DELETION,  Key: '" + ls.ToString() + "' does not exist!";
                failedKey = true;
            }
        }

        UpdateLanguageDLL();
        // reset deletion number after the keys are deleted
        totalDeletionKeys = 0;

        if (!failedKey && !failedJson)
            displayMessage = "SUCCESSFUL DELETION, wait for DLL built.";
    }

    private void UpdateDirectoriesFromDLL(List<string> _dirs, bool _createDir = true)
    {
        string speechFileFullPath = Environment.CurrentDirectory + "/Assets/Resources/" + resourcesSpeechDirectory;
        if (!Directory.Exists(speechFileFullPath))
            return;

        foreach(string dir in _dirs)
        {
            if (Directory.Exists(speechFileFullPath + dir + "/"))
                EmptyDir(new DirectoryInfo(speechFileFullPath + dir + "/"));

            if (_createDir)
            {
                Directory.CreateDirectory(speechFileFullPath + dir + "/");

                foreach (string al in allEnumLanguages)
                    Directory.CreateDirectory(speechFileFullPath + dir + "/" + al + "/");
            }           
        }
    }

    private void EmptyDir(DirectoryInfo directory)
    {
        foreach (FileInfo file in directory.GetFiles()) file.Delete();
        foreach (DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);

        Directory.Delete(directory.FullName);
    }

}
#endif
