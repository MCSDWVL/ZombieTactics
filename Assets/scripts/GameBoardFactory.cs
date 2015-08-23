using UnityEngine;
using System.Collections;

public class GameBoardFactory : MonoBehaviour
{
	// The generic wall piece.
	public GamePiece WallPiecePrefab;

	// The generic floor piece.
	public GamePiece FloorPiecePrefab;

	// The generic zombie piece.
	public GamePiece ZombiePiecePrefab;

	// The Generic cop piece.
	public GamePiece CopPiecePrefab;

	// The Generic civilian piece.
	public GamePiece CivPiecePrefab;

	// Kinds of pieces
	public enum GP
	{
		F = 0, // Floor
		W = 1, // Wall
		Z = 2, // Zombie
		C = 3, // Cop
		V = 4, // Civilian
	}

	// A test map with some walls, floors, and two each of the piece pieces
	private GP[,] testMap = 
	{
		{GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F,GP.C,},
		{GP.F,GP.F,GP.V,GP.W,GP.F,GP.C,GP.F,GP.W,GP.F,GP.F,},
		{GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,},
		{GP.F,GP.F,GP.F,GP.W,GP.W,GP.F,GP.W,GP.W,GP.F,GP.F,},
		{GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,},
		{GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,},
		{GP.F,GP.F,GP.Z,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,},
		{GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,},
		{GP.F,GP.F,GP.F,GP.F,GP.Z,GP.F,GP.F,GP.F,GP.F,GP.F,},
		{GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.V,},
	};

	private GP[,] visibilitytestMap = 
	{
		{GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,},
		{GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,},
		{GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,},
		{GP.F,GP.F,GP.F,GP.F,GP.Z,GP.F,GP.F,GP.W,GP.F,GP.F,},
		{GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,},
		{GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,},
		{GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,},
		{GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,},
		{GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,},
		{GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,},
		{GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,},
		{GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,},
		{GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,},
		{GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,},
		{GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,},
		{GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,},
		{GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,},
		{GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.Z,GP.F,},
	};

	public GamePiece PrefabFromGP(GP gp)
	{
		switch (gp)
		{
			case GP.C:
				return CopPiecePrefab;
			case GP.F:
				return FloorPiecePrefab;
			case GP.V:
				return CivPiecePrefab;
			case GP.W:
				return WallPiecePrefab;
			case GP.Z:
				return ZombiePiecePrefab;
			default:
				return null;
		}
	}

	public void CreateGameBoard(GP[,] pieceArray)
	{
		var board = GameManager.Instance.Board;
		var width = pieceArray.GetLength(0);
		var height = pieceArray.GetLength(1);
		board.CreateBoard(width, height);

		for (var h = 0; h < width; ++h)
		{
			for (var v = 0; v < height; ++v)
			{
				// create the piece
				var piece = GameObject.Instantiate(PrefabFromGP(pieceArray[h,v]));

				// always add a floor piece even if piece isn't a floor piece.
				var isFloor = piece.IsFloorPiece;
				if (!isFloor)
				{
					var floorFirst = GameObject.Instantiate(FloorPiecePrefab);
					board.SetPiece(h, v, floorFirst);
				}

				// tell the game NonFloorPieces
				board.SetPiece(h, v, piece);
			}
		}
	}

	public void Start()
	{
		CreateGameBoard(testMap);
		GameManager.Instance.Board.FinalizeBoardCreation();
	}
}