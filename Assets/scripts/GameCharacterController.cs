using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameCharacterController : MonoBehaviour 
{
	public Character CharacterLink;
	public AIController AILink;

	public int MovementPoints = 3;
	public int SightRange = 7;

	public int MaxHP = 100;
	public int CurrentHP { get; set; }

	public int MaxInfection = 100;
	public int CurrentInfection { get; set; }

	public CharacterAction[] PostMoveActions;
	public CharacterAction SelectedAction { get; set; }
	
	public List<GamePiece> AvailableMovePositions;
	public List<GamePiece> AvailableAttackTargetFloorSquares;
	public List<GamePiece> AvailableAttackTargetCharacters;

	public delegate void CharacterEventHandler(GameCharacterController character);
	public event CharacterEventHandler CharacterMovementComplete;
	public void OnCharactersMovementComplete(GameCharacterController character) { if (CharacterMovementComplete != null) CharacterMovementComplete(character); }

	public event CharacterEventHandler CharacterActionComplete;
	public void OnCharactersActionComplete(GameCharacterController character) { if (CharacterActionComplete != null) CharacterActionComplete(character); }

	public event CharacterEventHandler CharacterActionSelectComplete;
	public void OnCharactersActionSelectComplete(GameCharacterController character) { if (CharacterActionSelectComplete != null) CharacterActionSelectComplete(character); }
	
	public event CharacterEventHandler CharacterTurnEnded;
	public void OnCharactersTurnEnded(GameCharacterController character) { if (CharacterTurnEnded != null) CharacterTurnEnded(character); }

	private List<GamePiece> activeMovement;
	private bool moving = false;

	public Character TransformationTarget;

	public enum Team
	{
		Human,
		Zombie,
	}

	public Team CurrentTeam;

	public bool IsAIControlled()
	{
		return AILink != null;
	}

	//---------------------------------------------------------------------------
	public void Awake()
	{
		CharacterLink = gameObject.GetComponent<Character>();
		AvailableMovePositions = new List<GamePiece>();
		AvailableAttackTargetFloorSquares = new List<GamePiece>();
		AvailableAttackTargetCharacters = new List<GamePiece>();
		activeMovement = new List<GamePiece>();
		AILink = gameObject.GetComponent<AIController>();

		CurrentHP = MaxHP;
		CurrentInfection = 0;
	}

	//---------------------------------------------------------------------------
	public void GetAvailableMovementPositions()
	{
		AvailableMovePositions.Clear();
		GameManager.Instance.Board.GetAvailableMovePositionsRecursive(CharacterLink, ref AvailableMovePositions, CharacterLink.BoardHPos, CharacterLink.BoardVPos, MovementPoints+1);
	}

	//---------------------------------------------------------------------------
	public void BeginMoveTo(GamePiece target)
	{
		if (activeMovement.Count > 0)
		{
			Debug.LogError("Moving while moving!");
			return;
		}

		ClearPotentialMoves();
		activeMovement = GetPathTo(target.BoardHPos, target.BoardVPos);

		// remove the first node of the path (since it will be our own space)
		if (activeMovement != null && activeMovement.Count > 0)
			activeMovement.RemoveAt(0);

		moving = true;
		Debug.Log("Path found with " + activeMovement.Count + " steps: " + activeMovement);
	}

	//---------------------------------------------------------------------------
	public void BeginMovePhase()
	{
		GetAvailableMovementPositions();
		foreach (var floorPiece in AvailableMovePositions)
		{
			floorPiece.GetComponent<SpriteRenderer>().color = Color.blue;
		}

		if (AILink != null)
			AILink.CountdownToSelectMovement = 1f;
	}

	//---------------------------------------------------------------------------
	public void BeginActionSelectPhase()
	{
		// 
		if (AILink != null)
			AILink.CountdownToSelectAction = 0.01f;
	}

	//---------------------------------------------------------------------------
	public void EndActionSelectPhase()
	{
		OnCharactersActionSelectComplete(this);
	}

	//---------------------------------------------------------------------------
	public void OnActionSelected(int idx)
	{
		Debug.Log("Action Selected " + idx);
		SelectedAction = PostMoveActions[idx];
		EndActionSelectPhase();
	}

	//---------------------------------------------------------------------------
	public void PerformAction(GamePiece targetFloorSquare)
	{
		if(targetFloorSquare != null)
		{
			// Do something to the characters in the AOE of the target
			var dummyFloorPieces = new List<GamePiece>();
			var hitTargets = new List<GamePiece>();

			GameManager.Instance.Board.GetAvailableTargets(targetFloorSquare, SelectedAction.AoeAtTarget, false, true, out hitTargets, out dummyFloorPieces);
			foreach (var hitTarget in hitTargets)
			{
				var characterController = hitTarget.GetComponent<GameCharacterController>();
				if(characterController)
					characterController.GetHit(SelectedAction.GetHealthDamage(), SelectedAction.GetInfectionDamage());
			}
		}
		EndTargetSelectPhase();
	}

	//---------------------------------------------------------------------------
	public void GetHit(int damage, int infection)
	{
		Debug.Log(gameObject + " getting hit " + damage + ", " + infection);
		CurrentHP -= damage;
		CurrentInfection += infection;

		if (CurrentHP > MaxHP)
			CurrentHP = MaxHP;

		if (CurrentHP <= 0)
			Die();
		else if (CurrentInfection >= MaxInfection)
			Transform();
	}

	//---------------------------------------------------------------------------
	private void Transform()
	{
		if (TransformationTarget != null)
		{
			var newPiece = GameObject.Instantiate(TransformationTarget);
			EndTurn();
			GameManager.Instance.Board.SetPiece(CharacterLink.BoardHPos, CharacterLink.BoardVPos, newPiece);
		}
	}

	//---------------------------------------------------------------------------
	private void Die()
	{
		EndTurn();
		GameManager.Instance.Board.SetPiece(CharacterLink.BoardHPos, CharacterLink.BoardVPos, null);
	}

	//---------------------------------------------------------------------------
	public void BeginTargetSelectPhase()
	{
		// find available targets
		GameManager.Instance.Board.GetAvailableTargets(CharacterLink, SelectedAction.Range, SelectedAction.RequiresLineOfSight, SelectedAction.Pierces, out AvailableAttackTargetCharacters, out AvailableAttackTargetFloorSquares);
		
		// highlight all floor targets red
		foreach (var floorPiece in AvailableAttackTargetFloorSquares)
		{
			floorPiece.GetComponent<SpriteRenderer>().color = Color.red;
		}

		if (SelectedAction.IsNoOp())
			EndTargetSelectPhase();
		else if (AILink != null)
			AILink.CountdownToSelectTarget = 1f;
	}

	//---------------------------------------------------------------------------
	public void EndTargetSelectPhase()
	{
		ClearPotentialTargets();
		OnCharactersActionComplete(this);
		EndTurn();
	}

	//---------------------------------------------------------------------------
	public void BeginTurn()
	{
		if (AILink)
			AILink.OnTurnBegins();
		BeginMovePhase();
	}

	//---------------------------------------------------------------------------
	public List<GamePiece> GetPathTo(int hPos, int vPos)
	{
		return GameManager.Instance.Board.GetShortestPath(CharacterLink, CharacterLink.BoardHPos, CharacterLink.BoardVPos, hPos, vPos, MovementPoints);
	}

	//---------------------------------------------------------------------------
	private void ClearPotentialMoves()
	{
		foreach (var floorPiece in AvailableMovePositions)
		{
			floorPiece.GetComponent<SpriteRenderer>().color = Color.white;
		}
		AvailableMovePositions.Clear();
	}

	//---------------------------------------------------------------------------
	private void ClearPotentialTargets()
	{
		foreach (var floorPiece in AvailableAttackTargetFloorSquares)
		{
			floorPiece.GetComponent<SpriteRenderer>().color = Color.white;
		}
		AvailableAttackTargetFloorSquares.Clear();
		AvailableAttackTargetCharacters.Clear();
	}

	//---------------------------------------------------------------------------
	private void ClearAllActiveSelectionUI()
	{
		ClearPotentialMoves();
		ClearPotentialTargets();
	}

	//---------------------------------------------------------------------------
	public void EndTurn()
	{
		ClearAllActiveSelectionUI();
		OnCharactersTurnEnded(this);
	}

	//---------------------------------------------------------------------------
	// What can this guy see?
	public List<GamePiece> GetPiecesVisible()
	{
		// STUB - return all piece
		return GameManager.Instance.Board.AllPiecesList;
	}

	//---------------------------------------------------------------------------
	public bool CanSeePosition(int hPos, int vPos)
	{
		// STUB - pretend you can always see everything
		return true;
	}

	public float SecondsToMoveOneSquare = .3f;
	private float movementTime = 0f;
	public void Update()
	{
		if (moving)
		{
			if (activeMovement.Count > 0)
			{
				movementTime += Time.deltaTime;
				var movePercent = movementTime / SecondsToMoveOneSquare;
				var startPosition = GameManager.Instance.Board.GetWorldPositionForBoardPosition(CharacterLink.BoardHPos, CharacterLink.BoardVPos);
				var targetPosition = GameManager.Instance.Board.GetWorldPositionForBoardPosition(activeMovement[0].BoardHPos, activeMovement[0].BoardVPos);
				var currentPosition = Vector2.Lerp(startPosition, targetPosition, movePercent);

				gameObject.transform.position = currentPosition;
				if (movePercent >= 1f)
				{
					movementTime = 0f;
					GameManager.Instance.Board.MovePiecetoNewLocation(activeMovement[0].BoardHPos, activeMovement[0].BoardVPos, CharacterLink);
					activeMovement.RemoveAt(0);
				}
			}
			else
			{
				moving = false;
				OnCharactersMovementComplete(this);
			}
		}
	}
}
