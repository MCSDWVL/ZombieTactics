using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonHack : MonoBehaviour 
{
	public string SceneToLoadName;
	void Start () 
	{
		var button = GetComponent<Button>();

		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(() => Application.LoadLevel(SceneToLoadName));
	}
}
