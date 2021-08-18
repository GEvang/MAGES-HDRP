using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text.RegularExpressions;
using System;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Linq;

namespace ovidVR.UIManagement.LanguageText
{  

    public class UILanguageImporter : MonoBehaviour
    {
        public static string filePath = "MAGESres/UI/Text/";

        public static string resourcesJsonDirectory = "MAGESres/UI/Text/";
        public static string jsonFileName = "LanguageTranslationMsg";

        public class UserReadableJsonData
        {
            public string key { get; set; }
            public Dictionary<string,string> languages { get; set; }
        }

        public class LanguageData
        {
            public LanguageTranslator key { get; set; }
            public Dictionary<ApplicationLanguages, string> languages { get; set; }
        }

        internal Dictionary<LanguageTranslator, Dictionary<ApplicationLanguages, string>> languageData;

        private static UILanguageImporter uiMulLangTxt;

        public static UILanguageImporter Get
        {
            get
            {
                if (!uiMulLangTxt)
                {
                    uiMulLangTxt = FindObjectOfType(typeof(UILanguageImporter)) as UILanguageImporter;

                    if (!uiMulLangTxt) { Debug.LogError("Error, No UILanguageImporter was found"); return null; }

                    if(uiMulLangTxt.languageData == null)
                        uiMulLangTxt.Initialize();
                }

                return uiMulLangTxt;
            }
        }   

        private void Awake()
        {
            Configuration SceneManagement = GameObject.Find("SCENE_MANAGEMENT").GetComponent<Configuration>();
            if(!String.IsNullOrEmpty(SceneManagement.LanguageTranslationJsonPath))
            {
                filePath = SceneManagement.LanguageTranslationJsonPath.Remove(SceneManagement.LanguageTranslationJsonPath.LastIndexOf("/")) + "/";

                resourcesJsonDirectory = filePath;

                jsonFileName = Path.GetFileNameWithoutExtension(SceneManagement.LanguageTranslationJsonPath);
            }
          
            Initialize();
        }

        private void Initialize()
        {
            languageData = new Dictionary<LanguageTranslator, Dictionary<ApplicationLanguages, string>>();
            ImportAndConverUITexts();                 
        }

        private void ImportAndConverUITexts()
        {
            // IMPORT --------------------

            List<UserReadableJsonData> importData = ImportJsonFile();

            // TRANSLATE TO DICTIONARIES --------------
            try
            {
                foreach (UserReadableJsonData entry in importData)
                {
                    Dictionary<ApplicationLanguages, string> keyLanguages = new Dictionary<ApplicationLanguages, string>();

                    LanguageTranslator key = (LanguageTranslator)Enum.Parse(typeof(LanguageTranslator), entry.key);

                    foreach (KeyValuePair<string, string> entryValue in entry.languages)
                    {
                        string message = entryValue.Value;
                        message = message.Replace("\n", Environment.NewLine);
                        message = message.Replace("\r", string.Empty);
                        message = message.Replace("\t", "    ");

                        keyLanguages.Add((ApplicationLanguages)Enum.Parse(typeof(ApplicationLanguages), entryValue.Key), message);
                    }

                    languageData.Add(key, keyLanguages);
                }
            }
            catch(Exception e)
            {
                Debug.LogError("Failed to convert imported json file data from UserReadableJsonData to LanguageData");
                Debug.LogError("Exception: " + e.Message);
            }
            
        }

        // PUBLIC FUNCTIONS ----------------------------------------

        public static List<UserReadableJsonData> ImportJsonFile()
        {
            List<UserReadableJsonData> importData = new List<UserReadableJsonData>();

            TextAsset jsonImportFile = Resources.Load(resourcesJsonDirectory + jsonFileName) as TextAsset;
            Stream jsonStream = new MemoryStream(jsonImportFile.bytes);

            using (StreamReader r = new StreamReader(jsonStream))
            {
                string json = r.ReadToEnd();
                importData = JsonConvert.DeserializeObject<List<UserReadableJsonData>>(json);
            }

            return importData;
        }
    }
}