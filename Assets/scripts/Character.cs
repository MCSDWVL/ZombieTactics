using UnityEngine;
using System.Collections;

public class Character : GamePiece {
	public GameCharacterController Controller;
	public string Name;
	public string Description;

	public string GetNameAndDescription()
	{
		return Name + ": " + Description;
	}

	private void OnMouseEnter()
	{
		if (Controller.VisibleToPlayer)
		{
			GameManager.Instance.ToolTip.SetActive(true);
			GameManager.Instance.TooltipText.text = "HP: " + Controller.CurrentHP + "/" + Controller.MaxHP + "\n" + "Infection: " + Controller.CurrentInfection + "/" + Controller.MaxInfection;
		}
	}

	private void OnMouseExit()
	{
		GameManager.Instance.ToolTip.SetActive(false);
		GameManager.Instance.TooltipText.text = "";
	}

	private void OnMouseOver()
	{
		if (Controller.VisibleToPlayer)
		{
			//if (!GameManager.Instance.ToolTip.activeInHierarchy)
				OnMouseEnter();
		}

		//GameManager.Instance.ToolTip.transform.position = Input.mousePosition;
	}
}
