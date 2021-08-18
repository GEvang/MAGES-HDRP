using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractiveUI : MonoBehaviour {

	[SerializeField, Header("Fade out destroy duration"), Tooltip("Concept is the bigger the UI the more time it needs"), Range(0.2f,2f)]
	private float duration = 0.5f;

	void Start () { }

	void OnDisable()
	{
		StopAllCoroutines();
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.P))
			DestroyInteractiveUI();
	}


	IEnumerator DestroyAnimation()
	{
		float timer = 0f;

		List<Text> allTexts = new List<Text>();
		List<Image> allImages = new List<Image>();

		allTexts.AddRange(GetComponentsInChildren<Text>());
		allImages.AddRange(GetComponentsInChildren<Image>());

		while(timer < duration)
		{
			float alpha = Mathf.SmoothStep(1f,0f, timer/duration);
			Color c;

			foreach(Text t in allTexts)
			{
				c = t.color;
				c.a = alpha;
				t.color = c;
			}

			foreach (Image i in allImages)
			{
				c = i.color;
				c.a = alpha;
				i.color = c;
			}

			timer += Time.deltaTime;
			yield return null;
		}

		Destroy(this.gameObject);
	}

	public void DestroyInteractiveUI(bool _immediate = false)
	{
		if (_immediate)
		{
			Destroy(this.gameObject);
			return;
		}

		StartCoroutine("DestroyAnimation");
	}
}
