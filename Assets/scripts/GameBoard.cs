using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameBoard : MonoBehaviour 
{
	public GamePiece[,] NonFloorPieces { get; set; }
	public GamePiece[,] FloorPieces { get; set; }
	public SpriteRenderer[,] VisionHidePieces { get; set; }
	public List<GamePiece> AllPiecesList;
	public List<Character> AllPlayerOwnedPieces;
	public List<Character> AllAIOwnedPieces;
	public float PieceSpace = .3f;

	public SpriteRenderer VisionHidePrefab;
	
	// Game Board Events
	public delegate void PieceEventHandler(GameBoard board, GamePiece piece);
	public event PieceEventHandler PieceAdded;
	public void OnPieceAdded(GameBoard board, GamePiece piece) { if (PieceAdded != null) PieceAdded(board, piece); }

	public event PieceEventHandler PieceRemoved;
	public void OnPieceRemoved(GameBoard board, GamePiece piece) { if (PieceRemoved != null) PieceRemoved(board, piece); }

	public bool BoardDoneBeingCreated { get; set; }

	//---------------------------------------------------------------------------
	public void CreateBoard(int width, int height)
	{
		BoardDoneBeingCreated = false;
		NonFloorPieces = new GamePiece[width, height];
		FloorPieces = new GamePiece[width, height];
		AllPiecesList = new List<GamePiece>(width * height);
		AllPlayerOwnedPieces = new List<Character>();
		AllAIOwnedPieces = new List<Character>();
		VisionHidePieces = new SpriteRenderer[width, height];
		for (var h = 0; h < width; ++h)
			for (var v = 0; v < height; ++v)
			{
				VisionHidePieces[h, v] = GameObject.Instantiate(VisionHidePrefab);
				VisionHidePieces[h, v].transform.position = GetWorldPositionForBoardPosition(h, v);
			}
	}

	//---------------------------------------------------------------------------
	public void FinalizeBoardCreation()
	{
		BoardDoneBeingCreated = true;
		foreach (var character in AllPlayerOwnedPieces)
			character.Controller.UpdateVisiblePieces();
		foreach (var character in AllAIOwnedPieces)
			character.Controller.UpdateVisiblePieces();
	}

	//---------------------------------------------------------------------------
	public void SetPiece(int hPos, int vPos, GamePiece piece)
	{
		// Check if something is already in that spot
		var currentPiece = NonFloorPieces[hPos, vPos];
		if (currentPiece && !currentPiece.IsFloorPiece)
		{
			// Remove whatever is in that spot.
			OnPieceRemoved(this, currentPiece);
			GameObject.Destroy(currentPiece.gameObject);
			NonFloorPieces[hPos, vPos] = null;
			AllPiecesList.Remove(currentPiece);
		}

		// Make sure we weren't just cleaning up the old piece.
		if (piece == null)
			return;

		// add and position the piece.
		piece.transform.position = GetWorldPositionForBoardPosition(hPos, vPos);
		if (piece.IsFloorPiece)
			FloorPieces[hPos, vPos] = piece;
		else
			NonFloorPieces[hPos, vPos] = piece;
		AllPiecesList.Add(piece);
		piece.BoardHPos = hPos;
		piece.BoardVPos = vPos;

		var character = piece as Character;
		if (character)
		{
			if (character.Controller.IsAIControlled())
				AllAIOwnedPieces.Add(character);
			else
				AllPlayerOwnedPieces.Add(character);

			if(BoardDoneBeingCreated)
				character.Controller.UpdateVisiblePieces();
		}

		OnPieceAdded(this, piece);
	}

	public void MovePiecetoNewLocation(int hPos, int vPos, GamePiece piece)
	{
		if (NonFloorPieces[piece.BoardHPos, piece.BoardVPos] != piece)
		{
			Debug.LogError("Moving piece that isn't there! " + piece);
			return;
		}

		if (NonFloorPieces[hPos, vPos] != null)
		{
			Debug.LogError("Trying to move into already occupied space! " + hPos + ", " + vPos);
			return;
		}

		NonFloorPieces[piece.BoardHPos, piece.BoardVPos] = null;
		NonFloorPieces[hPos, vPos] = piece;
		piece.BoardHPos = hPos;
		piece.BoardVPos = vPos;
	}

	//---------------------------------------------------------------------------
	public GamePiece GetFloorPieceIfAvailable(int hPos, int vPos, GamePiece ignorePiece = null)
	{
		if (NonFloorPieces[hPos, vPos] == null || NonFloorPieces[hPos, vPos] == ignorePiece)
			return FloorPieces[hPos, vPos];
		return null;
	}

	//---------------------------------------------------------------------------
	public bool PosInBounds(int hPos, int vPos)
	{
		if (hPos < 0 || vPos < 0 || hPos >= NonFloorPieces.GetLength(0) || vPos >= NonFloorPieces.GetLength(1))
			return false;
		else
			return true;
	}

	//---------------------------------------------------------------------------
	public static int ManhattenDistance(GamePiece A, GamePiece B)
	{
		return ManhattenDistance(A.BoardHPos, A.BoardVPos, B.BoardHPos, B.BoardVPos);
	}

	//---------------------------------------------------------------------------
	public static int ManhattenDistance(GamePiece A, int hPosB, int vPosB)
	{
		return ManhattenDistance(A.BoardHPos, A.BoardVPos, hPosB, vPosB);
	}

	//---------------------------------------------------------------------------
	public static int ManhattenDistance(int hPosA, int vPosA, int hPosB, int vPosB)
	{
		return Mathf.Abs(hPosA - hPosB) + Mathf.Abs(vPosA - vPosB);
	}

	//---------------------------------------------------------------------------
	public void GetAvailableTargets(GamePiece startingPiece, int attackRange, bool throughWalls, bool pierce, out List<GamePiece> allCharactersInRange, out List<GamePiece> allFloorPiecesInRange)
	{
		// Default - hit everything but floor
		var hitMask =  ~(1 << LayerMask.NameToLayer("Floor"));

		// Default - characters are piercable
		var piercableMask = 1 << LayerMask.NameToLayer("Character");

		// Default - walls are impentrable
		var impenetrableMask = 1 << LayerMask.NameToLayer("Wall");

		GetAvailableTargets(startingPiece, attackRange, throughWalls, pierce, out allCharactersInRange, out allFloorPiecesInRange, hitMask, piercableMask, impenetrableMask);
	}

	//---------------------------------------------------------------------------
	public void GetAvailableTargets(GamePiece startingPiece, int attackRange, bool throughWalls, bool pierce, out List<GamePiece> allCharactersInRange, out List<GamePiece> allFloorPiecesInRange, int hitMask, int piercableMask, int impenetrableMask)
	{
		allFloorPiecesInRange = new List<GamePiece>();
		allCharactersInRange = new List<GamePiece>();
		for (var hDiff = -attackRange; hDiff <= attackRange; ++hDiff)
		{
			for (var vDiff = -attackRange; vDiff <= attackRange; ++vDiff)
			{
				var targetPosH = startingPiece.BoardHPos + hDiff;
				var targetPosV = startingPiece.BoardVPos + vDiff;
				if (Mathf.Abs(hDiff) + Mathf.Abs(vDiff) <= attackRange && PosInBounds(targetPosH, targetPosV))
				{
					// assume we can hit the square until we can't
					var canHitThisSquare = true;

					// Check for line of sight?
					if (!throughWalls)
					{
						// ignore floor hits
						var startVector = GetWorldPositionForBoardPosition(startingPiece.BoardHPos, startingPiece.BoardVPos);
						var endVector = GetWorldPositionForBoardPosition(targetPosH, targetPosV);
						var hits = Physics2D.RaycastAll(startVector, endVector - startVector, (endVector - startVector).magnitude, hitMask);

						// check if it needs to stop due to piercing problems
						foreach (var hit in hits)
						{
							// ignore hitting self
							if (Mathf.Approximately(hit.fraction, 0f))
								continue;

							var isImpenetrableLayer = ((1 << hit.collider.gameObject.layer) & impenetrableMask) != 0;
							var isPiercableLayer = ((1 << hit.collider.gameObject.layer) & piercableMask) != 0;

							// if a piece was hit, check if it was at the target terminus (yes we can hit) or not (no we can't hit)
							if (isImpenetrableLayer || (!pierce && isPiercableLayer))
							{
								var piece = hit.collider.GetComponent<GamePiece>();
								if (piece)
								{
									if (piece.BoardHPos != targetPosH || piece.BoardVPos != targetPosV)
									{
										canHitThisSquare = false;
										break;
									}
								}
							}
						}
					}
					if (canHitThisSquare)
					{
						var charThisSquare = NonFloorPieces[targetPosH, targetPosV];
						var floorThisSquare = FloorPieces[targetPosH, targetPosV];
						if (charThisSquare != null)
							allCharactersInRange.Add(charThisSquare);
						allFloorPiecesInRange.Add(floorThisSquare);
					}
				}
			}
		}
	}

	//---------------------------------------------------------------------------
	public List<GamePiece> GetAvailableMovePositionsRecursive(GamePiece startingPiece, ref List<GamePiece> alreadyIn, int hPos, int vPos, int movesLeft)
	{
		// are we out of moves or out of bounds?
		if (movesLeft <= 0 || hPos < 0 || vPos < 0 || hPos >= NonFloorPieces.GetLength(0) || vPos >= NonFloorPieces.GetLength(1))
			return alreadyIn;

		var openFloor = GetFloorPieceIfAvailable(hPos, vPos, startingPiece);
		if (openFloor != null)
		{
			// Is this a new spot?  If it is, add and recurse, if it's not, don't.
			if (!alreadyIn.Contains(openFloor))
			{
				alreadyIn.Add(openFloor);
			}
			GetAvailableMovePositionsRecursive(startingPiece, ref alreadyIn, hPos - 0, vPos - 1, movesLeft - 1);
			GetAvailableMovePositionsRecursive(startingPiece, ref alreadyIn, hPos - 0, vPos + 1, movesLeft - 1);
			GetAvailableMovePositionsRecursive(startingPiece, ref alreadyIn, hPos - 1, vPos - 0, movesLeft - 1);
			GetAvailableMovePositionsRecursive(startingPiece, ref alreadyIn, hPos + 1, vPos - 0, movesLeft - 1);
		}

		return alreadyIn;
	}

	//---------------------------------------------------------------------------
	private List<GamePiece> GetShortestList(List<List<GamePiece>> lists)
	{
		var shortest = int.MaxValue;
		List<GamePiece> ret = null;
		foreach (var list in lists)
		{
			if (list == null)
				continue;
			else if (list.Count < shortest)
			{
				shortest = list.Count;
				ret = list;
			}
		}
		return ret;
	}

	//---------------------------------------------------------------------------
	public List<GamePiece> GetShortestPath(GamePiece startingPiece, int hPos, int vPos, int targetHPos, int targetVPos, int movesLeft)
	{
		if (!PosInBounds(hPos, vPos))
			return null;

		// get the current piece
		var currentPiece = GetFloorPieceIfAvailable(hPos, vPos, startingPiece);

		// can't move here!
		if (currentPiece == null)
			return null;

		// add this piece
		var ret = new List<GamePiece>();
		ret.Add(currentPiece);

		// if we reached the goal return
		if (hPos == targetHPos && vPos == targetVPos)
		{
			return ret;
		}

		// no moves left and haven't hit the target oops give up here
		if (movesLeft <= 0)
			return null;

		// recurse and get the shortest list
		var listOfLists = new List<List<GamePiece>>();
		listOfLists.Add(GetShortestPath(startingPiece, hPos - 0, vPos - 1, targetHPos, targetVPos, movesLeft - 1));
		listOfLists.Add(GetShortestPath(startingPiece, hPos - 0, vPos + 1, targetHPos, targetVPos, movesLeft - 1));
		listOfLists.Add(GetShortestPath(startingPiece, hPos - 1, vPos - 0, targetHPos, targetVPos, movesLeft - 1));
		listOfLists.Add(GetShortestPath(startingPiece, hPos + 1, vPos - 0, targetHPos, targetVPos, movesLeft - 1));
		var shortestList = GetShortestList(listOfLists);

		// no path from here to the end?  return null.
		if(shortestList == null)
			return null;
		
		ret.AddRange(shortestList);
		return ret;
	}

	//---------------------------------------------------------------------------
	public Vector2 GetWorldPositionForBoardPosition(int hPos, int vPos)
	{
		return new Vector2(PieceSpace * hPos, PieceSpace * vPos);
	}
}
