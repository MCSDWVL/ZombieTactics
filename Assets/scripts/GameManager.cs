using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using System.Linq;

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

	public GameObject ToolTip;
	public UnityEngine.UI.Text TooltipText;

	public CameraController MainCameraController;

	//---------------------------------------------------------------------------
	private void StartNextCharactersTurn()
	{
		if (transformationTargets.Count > 0)
		{
			Debug.Log("Waiting for Transformation to finish...");
			ActionMenu.UpdateWithTransformationChoices();
			return;
		}
			
		// Unsubscribe the old event
		if (activeCharacter)
		{
			activeCharacter.CharacterTurnEnded -= OnCharacterTurnEnded;
			activeCharacter.CharacterMovementComplete -= OnCharacterDoneMoving;
		}

		// Get a new piece started.
		activeCharacter = ActQueue.GetNextActiveCharacter();
		activeCharacter.CharacterTurnEnded += OnCharacterTurnEnded;
		activeCharacter.BeginTurn();
		OnCharactersTurnBegins(activeCharacter);

		if(PlayerCanSeePiece(activeCharacter.CharacterLink))
			MainCameraController.FlyToPiece(activeCharacter.CharacterLink);
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
			ForceEndTurn();
			//StartNextCharactersTurn();
		}
	}

	//---------------------------------------------------------------------------
	// Hacky garbage lol
	private void ForceEndTurn()
	{
		Debug.Log("Force ending turn");
		if (activeCharacter)
		{
			activeCharacter.EndTurn();
			ActionMenu.HideActionButtons();
		}
	}

	//---------------------------------------------------------------------------
	private void Awake()
	{
		Debug.Log("Instance set");
		Instance = this;
	}

	//---------------------------------------------------------------------------
	private List<GameCharacterController> transformationTargets = new List<GameCharacterController>();
	public void BeginSelectTransformationPhase(GameCharacterController target)
	{
		transformationTargets.Add(target);
	}

	//---------------------------------------------------------------------------
	private void OnActionSelected(int idx)
	{
		ActionMenu.HideActionButtons();
		if (transformationTargets.Count > 0)
		{
			Debug.Log("Transforming?");
			var target = transformationTargets[0];
			transformationTargets.RemoveAt(0);
			target.TransformationTarget = ActionMenu.TransformationOptions[idx];
			target.Transform();

			// Try to end the turn??
			StartNextCharactersTurn();
		}
		else
		{
			activeCharacter.OnActionSelected(idx);

			// TODO: stop being garbage
			activeCharacter.BeginTargetSelectPhase();
		}
	}

	//---------------------------------------------------------------------------
	// Every FLOOR PIECE that is visible to the player.
	public List<GamePiece> AllVisibleFloorPieces = new List<GamePiece>();
	public List<GamePiece> UpdateAllPlayerVisibleGamePieces()
	{
		var oldVisiblePieces = new List<GamePiece>();
		oldVisiblePieces.AddRange(AllVisibleFloorPieces);

		AllVisibleFloorPieces.Clear();
		foreach (var character in Board.AllPlayerOwnedPieces)
			AllVisibleFloorPieces = AllVisibleFloorPieces.Union(character.Controller.VisibleFloors).ToList();

		var noLongerVisible = oldVisiblePieces.Except(AllVisibleFloorPieces);
		var newlyVisible = AllVisibleFloorPieces.Except(oldVisiblePieces);
		foreach (var piece in noLongerVisible)
		{
			if (piece == null)
				continue;

			// if this is a wall space, keep the walls visible
			var pieceHere = GameManager.Instance.Board.NonFloorPieces[piece.BoardHPos, piece.BoardVPos];
			if (pieceHere != null && pieceHere.gameObject.layer == LayerMask.NameToLayer("Wall"))
				continue;

			// make invisible
			GameManager.Instance.Board.VisionHidePieces[piece.BoardHPos, piece.BoardVPos].enabled = true;
		}
		foreach (var piece in newlyVisible)
		{
			if (piece == null)
				continue;
			GameManager.Instance.Board.VisionHidePieces[piece.BoardHPos, piece.BoardVPos].enabled = false;
		}

		// Stub - just return all pieces.
		return AllVisibleFloorPieces;
	}

	//---------------------------------------------------------------------------
	public bool PlayerCanSeePiece(GamePiece piece)
	{
		return PlayerCanSeePosition(piece.BoardHPos, piece.BoardVPos);
	}

	//---------------------------------------------------------------------------
	public bool PlayerCanSeePosition(int hPos, int vPos)
	{
		return AllVisibleFloorPieces.Contains(Board.FloorPieces[hPos, vPos]);
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
				// is this a noop movement?
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