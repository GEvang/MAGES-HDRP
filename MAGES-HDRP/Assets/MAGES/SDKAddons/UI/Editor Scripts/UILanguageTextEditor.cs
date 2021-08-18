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
using System.Linq;
using ovidVR.UIManagement;

using System.Reflection;
using System.Reflection.Emit;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

#if UNITY_EDITOR
public class UILanguageTextEditor : EditorWindow
{
    private Vector2 scrollPos;
    private bool showLanguages=true;
    private bool showKeys=false;
    private bool showMessages=false;

    private static string resourcesSpeechDirectory = "Sounds/" + Configuration.ProductCode + "/Speech/";

    public string saveDirectory = "/Assets/Plugins/LocalPlugins";

    public string jsonFile = "LanguageTranslationMsg.json";

    private static string displayMessage = "Waiting for dev's input...";
    private static string messageChange = "";

    private static List<string> allEnumLanguages;

    private static List<string> allDLLKeys;
    private static List<string> inputKeys, inputKeyMessages;
    private static List<LanguageTranslator> keyDLLSelections;
    private static bool deleteConfirmation = false, showNumInputKeys = false;
    private static int totalInputKeys = 0, totalDeletionKeys = 0;

    private static LanguageTranslator keyDisplay;
    private static LanguageTranslator keyDisplayMes;
    private static ApplicationLanguages langDisplay;
    private static ApplicationLanguages langDisplayMes;
    private static ApplicationLanguages langToBeRemoved;

    private static List<UILanguageImporter.UserReadableJsonData> jsonData;
    private static string newLanguage = "";
    private static List<string> newLanguages;
    [MenuItem("MAGES/UIs/Language Text")]
    public static void ShowWindow()
    {
        InitializeWindowValues();
        
        //Show existing window instance. If one doesn't exist, make one.
        GetWindow(typeof(UILanguageTextEditor));
    }

    private static void InitializeWindowValues()
    {
        allEnumLanguages = new List<string>();
        foreach (ApplicationLanguages lang in Enum.GetValues(typeof(ApplicationLanguages)))
        {
            allEnumLanguages.Add(lang.ToString());
        }
        allDLLKeys = new List<string>();
        foreach (LanguageTranslator lt in Enum.GetValues(typeof(LanguageTranslator)))
            allDLLKeys.Add(lt.ToString());

        inputKeys = new List<string>();
        inputKeyMessages = new List<string>();
        keyDLLSelections = new List<LanguageTranslator>();
        
        if (jsonData == null)
            jsonData = new List<UILanguageImporter.UserReadableJsonData>();
        else
            jsonData.Clear();

        jsonData = UILanguageImporter.ImportJsonFile();
        
    }
    
    private void OnGUI()
    {
        // Initialize from DLL
        if (allDLLKeys == null || allEnumLanguages == null)
        {
            InitializeWindowValues();
        }
        
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        showLanguages = EditorGUILayout.Foldout(showLanguages, "LANGUAGES");
        if (showLanguages)
        {
            if (allEnumLanguages.Count == 0)
            {
                UpdateLanguagesDLL();
            }

            EditorGUILayout.LabelField("Add a new language");
            EditorGUILayout.BeginHorizontal();

            newLanguage = EditorGUILayout.TextField(newLanguage);
            newLanguages = new List<string> {newLanguage};
            if (GUILayout.Button("Add Language"))
            {
                if (allEnumLanguages.Contains(newLanguage)) return;

                UpdateLanguagesDLL(newLanguages);
                UpdateDirectoriesFromDLL(newLanguages,0,true);
                ConvertAndExportJsonFile(1);
                AssetDatabase.Refresh();
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("-----------------------------------------------");
            EditorGUILayout.LabelField("Remove a language");
            EditorGUILayout.BeginHorizontal();
            langToBeRemoved =
                (ApplicationLanguages) EditorGUILayout.EnumPopup("All available languages:", langToBeRemoved);
            if (GUILayout.Button("Remove Language"))
            {
                DeleteLanguageFromDll();
                UpdateDirectoriesFromDLL(new List<string> { langToBeRemoved.ToString() }, 1, true);
                ConvertAndExportJsonFile(-1);
                AssetDatabase.Refresh();
            }
            
            EditorGUILayout.EndHorizontal();
        }

        showKeys = EditorGUILayout.Foldout(showKeys, "KEYS");
        if (showKeys)
        {
            if (allDLLKeys.Count == 0)
                UpdateKeysDLL();

            EditorGUILayout.LabelField(
                "Add or delete Keys for multiple languages (and the texts that bind to the corresponding key)");
            EditorGUILayout.LabelField(
                "CAUTION: By deleting a DLL key, count also the message deletion in all languages bound to this key in Json file");
            EditorGUILayout.LabelField("-----------------------------------------------");

            langDisplay = (ApplicationLanguages) EditorGUILayout.EnumPopup("All available languages:", langDisplay);
            keyDisplay = (LanguageTranslator) EditorGUILayout.EnumPopup("All current Keys in use:", keyDisplay);


            totalInputKeys = EditorGUILayout.IntField("Num of new Keys: ", totalInputKeys);
            showNumInputKeys = EditorGUILayout.Toggle("Show Key input fields: ", showNumInputKeys);

            // NEW KEYS START ---------------------------

            if (showNumInputKeys)
            {
                showNumInputKeys = false;

                inputKeys.Clear();
                inputKeyMessages.Clear();

                for (int i = 0; i < totalInputKeys; ++i)
                    inputKeys.Add("");

                for (int i = 0; i < totalInputKeys * allEnumLanguages.Count; ++i)
                    inputKeyMessages.Add("");
            }

            if (inputKeys.Count == totalInputKeys && totalInputKeys > 0)
            {
                for (int i = 0; i < totalInputKeys; ++i)
                {
                    EditorGUILayout.LabelField("- Key: " + i.ToString());

                    inputKeys[i] = EditorGUILayout.TextField("New Message Key: ", inputKeys[i]);

                    for (int j = 0; j < allEnumLanguages.Count; ++j)
                    {
                        int currMsgIndex = (i * allEnumLanguages.Count) + j;
                        inputKeyMessages[currMsgIndex] =
                            EditorGUILayout.TextField("New msg in " + allEnumLanguages[j] + " :",
                                inputKeyMessages[currMsgIndex]);
                    }
                }

                if (GUILayout.Button("Append to File: " + UILanguageImporter.jsonFileName))
                {
                    if (GetAreKeysInUse())
                        return;

                    int keysAdded = UpdateKeysDLL(inputKeys);
                    displayMessage = "SUCCESSFUL ENTRY, Added: " + keysAdded.ToString() + " out of: " +
                                     inputKeys.Count.ToString() + " key inputs. Wait for DLL built.";
                    ConvertAndExportJsonFile();
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
                    keyDLLSelections.Add((LanguageTranslator) (allDLLKeys.Count - 1 - i)); // Suggest keys from bottom up to be deleted first
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

                for (int i = 0; i < totalDeletionKeys; ++i)
                    keyDLLSelections[i] =
                        (LanguageTranslator) EditorGUILayout.EnumPopup("Select Key for deletion: ",
                            keyDLLSelections[i]);

                if (GUILayout.Button("Delete Selected Key(s)?"))
                {
                    DeleteKeysFromDLL();
                    ConvertAndExportJsonFile();
                    AssetDatabase.Refresh();
                }
            }
        }

        showMessages = EditorGUILayout.Foldout(showMessages, "MESSAGES");
        if (showMessages)
        {
            EditorGUILayout.BeginHorizontal();
            langDisplayMes = (ApplicationLanguages)EditorGUILayout.EnumPopup("All available languages:", langDisplayMes);
            keyDisplayMes = (LanguageTranslator)EditorGUILayout.EnumPopup("All current Keys in use:", keyDisplayMes);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            messageChange=EditorGUILayout.TextField("Message: ", messageChange);
            if (GUILayout.Button("Change Message"))
            {
                ConvertAndExportJsonFile(2);
                AssetDatabase.Refresh();
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.LabelField("-----------------------------------------------\n");
        EditorGUILayout.LabelField("STATUS UPDATE: " + displayMessage);
        EditorGUILayout.LabelField("-----------------------------------------------\n");

        EditorGUILayout.EndScrollView();
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

    private int UpdateLanguagesDLL(List<string> languages=null)
    {
        AppDomain currentDomain = AppDomain.CurrentDomain;

        // Create a dynamic assembly in the current application domain,
        // and allow it to be executed and saved to disk.
        AssemblyName name = new AssemblyName("Languages");
        AssemblyBuilder assemblyBuilder = currentDomain.DefineDynamicAssembly(name,
            AssemblyBuilderAccess.RunAndSave, Environment.CurrentDirectory + saveDirectory);

        // Define a dynamic module in "MyEnums" assembly.
        // For a single-module assembly, the module has the same name as the assembly.
        ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(name.Name, name.Name + ".dll");

        // Define a public enumeration with the name "MyEnum" and an underlying type of Integer.
        EnumBuilder myEnum = moduleBuilder.DefineEnum("ovidVR.UIManagement.ApplicationLanguages", TypeAttributes.Public, typeof(int));

        int totalAddedKeys = 0;
        if (languages != null && languages.Count > 0)
        {
            foreach (string s in languages)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    string key = s.Replace(" ", string.Empty);
                    key = key.Replace("\n", string.Empty);
                    key = key.Replace("\r", string.Empty);
                    key = key.Replace("\t", string.Empty);
                    allEnumLanguages.Add(key);
                    ++totalAddedKeys;
                }
            }
        }

        int currentEnumValue = 0;
        foreach (string s in allEnumLanguages)
        {
            myEnum.DefineLiteral(s, currentEnumValue);
            ++currentEnumValue;
        }
        myEnum.CreateType();

        // Finally, save the assembly
        assemblyBuilder.Save(name.Name + ".dll");

        return totalAddedKeys;
    }

    private int UpdateKeysDLL(List<string> _addedKeys = null)
    {
        AppDomain currentDomain = AppDomain.CurrentDomain;

        // Create a dynamic assembly in the current application domain,
        // and allow it to be executed and saved to disk.
        AssemblyName name = new AssemblyName("LanguageTranslatorDLL");
        AssemblyBuilder assemblyBuilder = currentDomain.DefineDynamicAssembly(name,
            AssemblyBuilderAccess.RunAndSave, Environment.CurrentDirectory + saveDirectory);

        // Define a dynamic module in "MyEnums" assembly.
        // For a single-module assembly, the module has the same name as the assembly.
        ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(name.Name, name.Name + ".dll");

        // Define a public enumeration with the name "MyEnum" and an underlying type of Integer.
        EnumBuilder myEnum = moduleBuilder.DefineEnum("ovidVR.UIManagement.LanguageTranslator", TypeAttributes.Public, typeof(int));

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

    private void DeleteLanguageFromDll()
    {
        bool failedKey = false, failedJson = false;
        // Delete keys from DLL
        if (allEnumLanguages.Contains(langToBeRemoved.ToString()))
            allEnumLanguages.Remove(langToBeRemoved.ToString());
        else
        {
            displayMessage = "FAILED SPECIFIC LANGUAGE DELETION-DLL,  LANGUAGE: '" + langToBeRemoved.ToString() + "' does not exist!";
            failedKey = true;
        }

        // Delete keys from Json
        failedJson = false;

        for (var i = jsonData.Count - 1; i >= 0; --i)
        {
            if (jsonData[i].languages.ContainsKey(langToBeRemoved.ToString()))
            {
                jsonData[i].languages.Remove(langToBeRemoved.ToString());
            }
            else
            {
                failedJson = true;
                break;
            }
            
        }

        if (failedJson)
            displayMessage = "FAILED SPECIFIC Language DELETION-JSON,  Key: '" + langToBeRemoved.ToString() + "'";

        UpdateLanguagesDLL();
        
        if (!failedKey && !failedJson)
            displayMessage = "SUCCESSFUL DELETION, wait for DLL built.";
    }

    private void DeleteKeysFromDLL()
    {
        bool failedKey = false, failedJson = false;

        foreach(LanguageTranslator lt in keyDLLSelections)
        {
            // Delete keys from DLL
            if (allDLLKeys.Contains(lt.ToString()))
                allDLLKeys.Remove(lt.ToString());
            else
            {
                displayMessage = "FAILED SPECIFIC KEY DELETION-DLL,  Key: '" + lt.ToString() + "' does not exist!";
                failedKey = true;
            }

            // Delete keys from Json
            failedJson = true;

            for(int i = jsonData.Count - 1; i >= 0; --i)
            {
                if (jsonData[i].key == lt.ToString())
                {
                    jsonData.RemoveAt(i);
                    failedJson = false;
                    break;
                }
            }

            if(failedJson)
                displayMessage = "FAILED SPECIFIC KEY DELETION-JSON,  Key: '" + lt.ToString() + "'";
        }

        UpdateKeysDLL();
        // reset deletion number after the keys are deleted
        totalDeletionKeys = 0;

        if(!failedKey && !failedJson)
            displayMessage = "SUCCESSFUL DELETION, wait for DLL built.";
    }

    public static void ConvertAndExportJsonFile(int mode=0)
    {

        // CONVERSION -------------------
        // Keys and Messages (per key) are both stored in separate lists
        // so for every 1 key step there are as mush as all languages available, in steps, for messages
        // e.g. if Languages {ENG, GR, FR}, +1 key step => +3 msg steps
        switch (mode)
        {
            case 2:
            {
                for (int i = 0; i < allDLLKeys.Count; ++i)
                {
                    if (allDLLKeys[i].Equals(keyDisplayMes.ToString()))
                    {
                        jsonData[i].languages[langDisplayMes.ToString()] = messageChange;
                    }
                }
                break;
            }
            case 1:
                for (int i = 0; i < allDLLKeys.Count; ++i)
                {
                    jsonData[i].languages.Add(newLanguage, "");
                }
                break;
            case 0:
                for (int i = 0; i < inputKeys.Count; ++i)
                {
                    EditorGUILayout.LabelField("- Key: " + i.ToString());

                    inputKeys[i] = EditorGUILayout.TextField("New Message Key: ", inputKeys[i]);

                    Dictionary<string, string> lang = new Dictionary<string, string>();
                    for (int j = 0; j < allEnumLanguages.Count; ++j)
                    {
                        int currMsgIndex = (i * allEnumLanguages.Count) + j;
                        lang.Add(allEnumLanguages[j], inputKeyMessages[currMsgIndex]);
                    }

                    UILanguageImporter.UserReadableJsonData d = new UILanguageImporter.UserReadableJsonData()
                        {key = inputKeys[i], languages = lang};

                    jsonData.Add(d);
                }
                break;
            case -1:
                break;
        }

        
        // EXPORT -----------------------

        string json = JsonConvert.SerializeObject(jsonData.ToArray(), Formatting.Indented);

        string jsonFullPath = Environment.CurrentDirectory + "/Assets/Resources/" +  
            UILanguageImporter.resourcesJsonDirectory + UILanguageImporter.jsonFileName + ".json";

        if (!File.Exists(jsonFullPath))
            File.Create(jsonFullPath);

        File.WriteAllText(jsonFullPath, json);
    }

    private void UpdateDirectoriesFromDLL(List<string> _dirs, int mode, bool _createDir = true)
    {
        
        string speechFileFullPath = Environment.CurrentDirectory + "/Assets/Resources/" + resourcesSpeechDirectory;
        if (!Directory.Exists(speechFileFullPath))
            return;
        if (mode == 0)
        {
            var directories = Directory.GetDirectories(speechFileFullPath);
            foreach (string dir in directories)
            {
                foreach (var lang in _dirs)
                {
                    if (_createDir)
                    {
                        Directory.CreateDirectory(dir + "/" + lang + "/");
                    }
                }
            }
        }
        else if (mode == 1)
        {
            var directories = Directory.GetDirectories(speechFileFullPath);
            foreach (string dir in directories)
            {
                foreach (var lang in _dirs)
                {
                    EmptyDir(new DirectoryInfo(dir + "/" + lang + "/"));
                }
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
