using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarManager : MonoBehaviour {
	
	public struct AvatarSkeletonReference
	{
		public GameObject avatar;
		public GameObject root;
		public GameObject chest;
		public GameObject head;
		public GameObject spine;
		public GameObject neck;
        
	}
	public struct CustomizationData
	{
		public int genderIdx;
		public int suitIdx;
		public int skinIdx;
	}

	public AvatarSkeletonReference currentAvatarSkeletonReference;
	public CustomizationData currentCustomizationData;
	public static AvatarManager Instance { get; private set; }

	private void Awake()
	{
		// if the singleton hasn't been initialized yet
		if (Instance != null && Instance != this) 
		{
			Destroy(this.gameObject);
		}
 
		Instance = this;
		DontDestroyOnLoad( this.gameObject );
	}


	public void SetCustomizationData(CustomizationData cD)
	{
		currentCustomizationData = cD;
	}
	
	public void SetAvatarReference(AvatarSkeletonReference aSr)
	{
		currentAvatarSkeletonReference = aSr;
	}
}
