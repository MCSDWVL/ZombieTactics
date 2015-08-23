using UnityEngine;
using System.Collections;

public class Character : GamePiece {
	public GameCharacterController Controller;

	private void OnMouseEnter()
	{
		
		GameManager.Instance.HealthTooltip.SetActive(true);
		GameManager.Instance.HealthTooltipText.text = "HP: " + Controller.CurrentHP + "/" + Controller.MaxHP + "\n" + "Infection: " + Controller.CurrentInfection + "/" + Controller.MaxInfection;
	}

	private void OnMouseExit()
	{
		GameManager.Instance.HealthTooltip.SetActive(false);
		GameManager.Instance.HealthTooltipText.text = "";
	}

	private void OnMouseOver()
	{
		if (!GameManager.Instance.HealthTooltip.activeInHierarchy)
			OnMouseEnter();

		//GameManager.Instance.HealthTooltip.transform.position = Input.mousePosition;
	}
}
