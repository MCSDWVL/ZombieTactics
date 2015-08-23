using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AIController : MonoBehaviour 
{
	// AI needs to listen for:
	//	- Start of movement select (pick spot based on priorities)
	//  - Start of action select (pick an attack to hurt the zombos)
	//	- Start of target select (pick a good target)

	public GameCharacterController ControllerLink;

	// What enemies do we know about?
	public List<GameCharacterController> KnownEnemies;

	// At what health should we run away?
	public int HealthFleeThreshold = 10;

	//---------------------------------------------------------------------------
	public int GetDesiredDistance()
	{
		// If we're almost dead, run away - otherwise try to get into range for our primary attack.
		if (ControllerLink.CurrentHP <= HealthFleeThreshold)
				return int.MaxValue;
		else
			return ControllerLink.PostMoveActions[0].Range;
	}

	//---------------------------------------------------------------------------
	public bool IsCharacterMyEnemy(GameCharacterController other)
	{
		return other.CurrentTeam != ControllerLink.CurrentTeam;
	}

	//---------------------------------------------------------------------------
	public GamePiece GetDesiredMovePosition()
	{
		var availablePositions = ControllerLink.AvailableMovePositions;
		var desiredDistance = GetDesiredDistance();

		// Default move to self (stay still)
		GamePiece bestPosition = ControllerLink.CharacterLink;
		int bestDelta = -1;

		Debug.Log(gameObject + " About to consider " + availablePositions.Count + " positions to find closest to desired distance " + desiredDistance);

		foreach (var position in availablePositions)
		{
			Debug.Log(gameObject + " Considering position " + position.BoardHPos + "," + position.BoardVPos + " desired distance: " + desiredDistance);
			foreach (var enemy in KnownEnemies)
			{
				var dist = GameBoard.ManhattenDistance(enemy.CharacterLink, position);
				var deltaFromDesired = Mathf.Abs(dist - desiredDistance);
				if (bestDelta < 0 || deltaFromDesired < bestDelta)
				{
					Debug.Log("Found position with distance " + dist);
					bestDelta = deltaFromDesired;
					bestPosition = position;
				}
			}
		}
		Debug.Log(gameObject + "Best position " + bestPosition.BoardHPos + "," + bestPosition.BoardVPos);
		return bestPosition;
	}

	//---------------------------------------------------------------------------
	GamePiece desiredTarget = null;
	public GamePiece GetDesiredTarget()
	{
		foreach (var enemy in KnownEnemies)
		{
			if (ControllerLink.AvailableAttackTargetCharacters.Contains(enemy.CharacterLink))
				return enemy.CharacterLink;
		}
		return null;
	}

	//---------------------------------------------------------------------------
	public int GetDesiredActionIndex()
	{
		// If someone is in range, select the attack, otherwise select no-op
		return 0;
	}

	public float CountdownToSelectMovement { get; set; }
	public float CountdownToSelectAction { get; set; }
	public float CountdownToSelectTarget { get; set; }

	//---------------------------------------------------------------------------
	public void Update()
	{
		// Action Select
		if (CountdownToSelectAction > 0)
		{
			CountdownToSelectAction -= Time.deltaTime;
			if (CountdownToSelectAction <= 0)
				GameManager.Instance.ActionMenu.OnActionSelected(GetDesiredActionIndex());
		}

		// Target Select
		if (CountdownToSelectTarget > 0)
		{
			CountdownToSelectTarget -= Time.deltaTime;
			if (CountdownToSelectTarget <= 0)
				GameManager.Instance.PlayerClickedSquare(GetDesiredTarget(), true);
		}

		// Movement Select
		if (CountdownToSelectMovement > 0)
		{
			CountdownToSelectMovement -= Time.deltaTime;
			if (CountdownToSelectMovement <= 0)
				GameManager.Instance.PlayerClickedSquare(GetDesiredMovePosition(), true);
		}
	}

	public void OnTurnBegins()
	{
	}

	public void OnSeeCharacters(List<GamePiece> seenobjects)
	{
		var characters = from character in seenobjects where (character as Character != null) select (Character)character;
		var enemies = from character in characters where IsCharacterMyEnemy(character.Controller) select character.Controller;
		KnownEnemies = KnownEnemies.Union(enemies).ToList();
	}
}
