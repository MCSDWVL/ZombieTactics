﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
	public List<GamePiece> VisibleCharacters;
	public List<GamePiece> VisibleFloors;

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

	public Animator AnimatorLink;

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
		VisibleCharacters = new List<GamePiece>();
		VisibleFloors = new List<GamePiece>();
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
		if(AnimatorLink)
			AnimatorLink.SetBool("Walking", moving);
	}

	public bool VisibleToPlayer { get { return GameManager.Instance.PlayerCanSeePiece(CharacterLink); } }

	//---------------------------------------------------------------------------
	public void BeginMovePhase()
	{
		GetAvailableMovementPositions();
		foreach (var floorPiece in AvailableMovePositions)
		{
			floorPiece.GetComponent<SpriteRenderer>().color = Color.blue;
		}

		if (AILink != null)
			AILink.CountdownToSelectMovement = VisibleToPlayer ? 1f : 0.01f;
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

			GameManager.Instance.Board.GetAvailableTargets(targetFloorSquare, SelectedAction.AoeAtTarget, false /* throughWalls */, true /* throughPeople */, out hitTargets, out dummyFloorPieces);
			foreach (var hitTarget in hitTargets)
			{
				var characterController = hitTarget.GetComponent<GameCharacterController>();
				if(characterController)
					characterController.GetHit(SelectedAction.GetHealthDamage(), SelectedAction.GetInfectionDamage());
			}

			if(AnimatorLink)
				AnimatorLink.SetTrigger(SelectedAction.TriggerAnimName);

			// apply the self hit aspect of the action
			GetHit(SelectedAction.GetSelfHealthDamage(), SelectedAction.GetSelfInfectionDamage(), true /* suppressReaction against self */);
		}
		EndTargetSelectPhase();
	}

	//---------------------------------------------------------------------------
	public void GetHit(int damage, int infection, bool supressReaction = false)
	{
		CurrentHP -= damage;
		CurrentInfection += infection;

		if (CurrentHP > MaxHP)
			CurrentHP = MaxHP;

		if (CurrentHP <= 0)
		{
			Die();
		}
		else if (CurrentInfection >= MaxInfection)
		{
			// Show transformation menu!
			GameManager.Instance.BeginSelectTransformationPhase(this);
		}
		else
		{
			if (AnimatorLink && !supressReaction)
				AnimatorLink.SetTrigger("HitReact");
		}
	}

	//---------------------------------------------------------------------------
	public void Transform()
	{
		if (TransformationTarget != null)
		{
			var newPiece = GameObject.Instantiate(TransformationTarget);
			// Keep health when transforming.
			newPiece.Controller.CurrentHP = CurrentHP;
			EndTurn();
			GameManager.Instance.Board.SetPiece(CharacterLink.BoardHPos, CharacterLink.BoardVPos, newPiece);
		}
	}

	//---------------------------------------------------------------------------
	private static int CORPSE_SORT_LAYER = 8;
	private void Die()
	{
		EndTurn();
		if (AnimatorLink)
			AnimatorLink.SetTrigger("Die");

		// Set sort order to make it go underneath people moving into this square.
		GetComponent<SpriteRenderer>().sortingOrder = CORPSE_SORT_LAYER;

		// remove the game piece but leave the corpse
		GameManager.Instance.Board.SetPiece(CharacterLink.BoardHPos, CharacterLink.BoardVPos, null /*replacement piece, null means remove*/, true /* leave corpse */);

		// delete some scripts that would make this keep doing stuff
		GameObject.Destroy(CharacterLink);
		GameObject.Destroy(this);
	}

	//---------------------------------------------------------------------------
	public void BeginTargetSelectPhase()
	{
		// find available targets
		GameManager.Instance.Board.GetAvailableTargets(CharacterLink, SelectedAction.Range, SelectedAction.ThroughWalls, SelectedAction.Pierces, out AvailableAttackTargetCharacters, out AvailableAttackTargetFloorSquares);
		
		// highlight all floor targets red
		foreach (var floorPiece in AvailableAttackTargetFloorSquares)
		{
			floorPiece.GetComponent<SpriteRenderer>().color = Color.red;
		}

		if (SelectedAction.IsNoOp())
			EndTargetSelectPhase();
		else if (AILink != null)
			AILink.CountdownToSelectTarget = VisibleToPlayer ? 1f : 0.01f;
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
		UpdateVisiblePieces();
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
	public void UpdateVisiblePieces()
	{
		GameManager.Instance.Board.GetAvailableTargets(CharacterLink, SightRange, false /* through walls */, true /* through people */, out VisibleCharacters, out VisibleFloors);
		if (IsAIControlled())
		{
			AILink.OnSeeCharacters(VisibleCharacters);
		}
		else
		{
			GameManager.Instance.UpdateAllPlayerVisibleGamePieces();
		}		
	}

	//---------------------------------------------------------------------------
	// What can this guy see?
	public List<GamePiece> GetPiecesVisible()
	{
		return VisibleFloors;
	}

	//---------------------------------------------------------------------------
	public bool CanSeePosition(int hPos, int vPos)
	{
		var floorAtTarget = GameManager.Instance.Board.FloorPieces[hPos, vPos];
		return VisibleFloors.Contains(floorAtTarget);
	}

	//---------------------------------------------------------------------------
	public float SecondsToMoveOneSquare = .3f;
	private float movementTime = 0f;
	public void Update()
	{
		if (moving)
		{
			if (activeMovement.Count > 0)
			{
				movementTime += Time.deltaTime;
				var movePercent = VisibleToPlayer ? movementTime / SecondsToMoveOneSquare : 1f;
				var startPosition = GameManager.Instance.Board.GetWorldPositionForBoardPosition(CharacterLink.BoardHPos, CharacterLink.BoardVPos);
				var targetPosition = GameManager.Instance.Board.GetWorldPositionForBoardPosition(activeMovement[0].BoardHPos, activeMovement[0].BoardVPos);
				var currentPosition = Vector2.Lerp(startPosition, targetPosition, movePercent);

				gameObject.transform.position = currentPosition;
				if (movePercent >= 1f)
				{
					movementTime = 0f;
					GameManager.Instance.Board.MovePiecetoNewLocation(activeMovement[0].BoardHPos, activeMovement[0].BoardVPos, CharacterLink);
					UpdateVisiblePieces();
					activeMovement.RemoveAt(0);
				}
			}
			else
			{
				moving = false;
				if(AnimatorLink)
					AnimatorLink.SetBool("Walking", moving);
				OnCharactersMovementComplete(this);
			}
		}
	}
}
