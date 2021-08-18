using ovidVR.GameController;
using ovidVR.Utilities;
using System.Collections;
using UnityEngine;

public class GeneralPrefabSpawner : MonoBehaviour {

	public void SpawnNetworkPrefabs() {

        GameObject gameobject = GameObject.Find("NetworkReplacable");
        if (gameobject)
        {
            int k = gameobject.transform.childCount;
            for (int i = 0; i < k; i++)
            {
                Destroy(gameobject.transform.GetChild(i).gameObject);
            }
        }

        StartCoroutine(Spawn());
    }

    private IEnumerator Spawn()
    {
        yield return new WaitForEndOfFrame();
        
        object[] res = Resources.LoadAll(@"LessonPrefabs\NetworkSpawns\");
        foreach (object g in res)
        {
            PrefabImporter.SpawnGenericPrefab(@"LessonPrefabs\NetworkSpawns\" + ((GameObject) g).name);
        }
    }
}
