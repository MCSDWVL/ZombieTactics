using UnityEngine;
using System.Collections;

using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }

	public GameBoard Board;
	public ActionQueue ActQueue;

	public GameCharacterController activeCharacter;

	public delegate void CharacterEventHandler(GameCharacterController character);
	public event CharacterEventHandler CharactersTurnBegins;
	public void OnCharactersTurnBegins(GameCharacterController character) { if (CharactersTurnBegins != null) CharactersTurnBegins(character); }

	public event CharacterEventHandler CharactersTurnEnds;
	public void OnCharactersTurnEnds(GameCharacterController character) { if (CharactersTurnEnds != null) CharactersTurnEnds(character); }

	public ActionSelectMenu ActionMenu;

	public GameObject HealthTooltip;
	public UnityEngine.UI.Text HealthTooltipText;

	//---------------------------------------------------------------------------
	private void StartNextCharactersTurn()
	{
		// Unsubscribe the old event
		if (activeCharacter)
		{
			activeCharacter.CharacterTurnEnded -= OnCharacterTurnEnded;
		}

		// Get a new character started.
		activeCharacter = ActQueue.GetNextActiveCharacter();
		activeCharacter.CharacterTurnEnded += OnCharacterTurnEnded;
		activeCharacter.BeginTurn();
		OnCharactersTurnBegins(activeCharacter);
	}

	//---------------------------------------------------------------------------
	private void Start()
	{
		ActionMenu.ActionSelected += OnActionSelected;
		ActionMenu.HideActionButtons();
		StartNextCharactersTurn();
	}

	//---------------------------------------------------------------------------
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			if(activeCharacter)
				activeCharacter.EndTurn();
			StartNextCharactersTurn();
		}
	}

	//---------------------------------------------------------------------------
	private void Awake()
	{
		Debug.Log("Instance set");
		Instance = this;
	}

	//---------------------------------------------------------------------------
	private void OnActionSelected(int idx)
	{
		ActionMenu.HideActionButtons();
		activeCharacter.OnActionSelected(idx);

		// TODO: stop being garbage
		activeCharacter.BeginTargetSelectPhase();
	}

	//---------------------------------------------------------------------------
	// Every FLOOR PIECE that is visible to the player.
	public List<GamePiece> GetAllPlayerVisibleGamePieces()
	{
		// Stub - just return all pieces.
		return Board.AllPiecesList;
	}

	//---------------------------------------------------------------------------
	private void OnCharacterDoneMoving(GameCharacterController character)
	{
		character.CharacterMovementComplete -= OnCharacterDoneMoving;
		
		character.BeginActionSelectPhase();

		// Trigger the menu
		ActionMenu.UpdateWithActiveCharacterActions(activeCharacter);
	}

	//---------------------------------------------------------------------------
	private void OnCharacterTurnEnded(GameCharacterController character)
	{
		if (character == activeCharacter)
		{
			StartNextCharactersTurn();
		}
	}

	public void SkipPhase()
	{

	}

	//---------------------------------------------------------------------------
	public void PlayerClickedSquare(GamePiece piece, bool aiPick = false)
	{
		if (activeCharacter == null || (activeCharacter.IsAIControlled() && !aiPick))
		{
			return;
		}

		// If the AI picks null it is a skip.
		if(piece == null && aiPick)
		{
			activeCharacter.PerformAction(null);
			return;
		}

		// is it empty?
		var floorAtClick = piece ? Board.FloorPieces[piece.BoardHPos, piece.BoardVPos] : null;
		if (floorAtClick)
		{
			// Is it available for movement?
			if (activeCharacter.AvailableMovePositions.Contains(floorAtClick))
			{
				activeCharacter.CharacterMovementComplete += OnCharacterDoneMoving;
				activeCharacter.BeginMoveTo(floorAtClick);
			}
		}

		// Is it an attack?
		if (activeCharacter.AvailableAttackTargetFloorSquares.Contains(floorAtClick))
		{
			activeCharacter.PerformAction(floorAtClick);
		}
	}
}