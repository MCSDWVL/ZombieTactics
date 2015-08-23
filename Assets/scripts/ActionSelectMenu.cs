using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ActionSelectMenu : MonoBehaviour {

	public Button[] actionButtons;

	public delegate void ActionSelectedHandler(int idx);
	public event ActionSelectedHandler ActionSelected;
	public void OnActionSelected(int idx) { if (ActionSelected != null) ActionSelected(idx); }
	
	public void UpdateWithActiveCharacterActions(GameCharacterController characterController)
	{
		var actions = characterController.PostMoveActions;
		for (var i = 0; i < actions.Length; ++i)
		{
			SetButtonVisible(actionButtons[i], true);
			actionButtons[i].GetComponentInChildren<Text>().text = actions[i].Name;

			actionButtons[i].onClick.RemoveAllListeners();
			
			// Add event listener which takes index and triggers event
			int trickIndex = i;
			actionButtons[i].onClick.AddListener(() => OnActionSelected(trickIndex));
		}
	}

	public void HideActionButtons()
	{
		foreach (var button in actionButtons)
			SetButtonVisible(button, false);
	}

	public static void SetButtonVisible(Button but, bool visible)
	{
		if (!visible)
		{
			but.enabled = false;
			but.GetComponentInChildren<CanvasRenderer>().SetAlpha(0);
			but.GetComponentInChildren<Text>().color = Color.clear;
		}
		else
		{
			but.enabled = true;
			but.GetComponentInChildren<CanvasRenderer>().SetAlpha(1);
			but.GetComponentInChildren<Text>().color = Color.black;
		}
	}
}
