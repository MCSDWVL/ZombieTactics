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

	// El Presidente!
	public GamePiece PresidentPiecePrefab;

	// Hazmat
	public GamePiece HazmatPiecePrefab;

	// Skinny
	public GamePiece SkinnyZombie;

	// Fat
	public GamePiece FatZombie;

	// Kinds of pieces
	public enum GP
	{
		I = -1, // Invalid
		F = 0, // Floor
		W = 1, // Wall
		Z = 2, // Zombie
		C = 3, // Cop
		V = 4, // Civilian
		P = 5, // President
		H = 6, // Hazmat
		S = 7, // Skinny
		O = 8, // Obese
	}

	// A test map with some walls, floors, and two each of the piece pieces
	private GP[,] testMap = 
	{
		{GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F,GP.C,},
		{GP.F,GP.F,GP.V,GP.W,GP.F,GP.C,GP.F,GP.W,GP.F,GP.F,},
		{GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.H,GP.F,},
		{GP.F,GP.F,GP.F,GP.W,GP.W,GP.F,GP.W,GP.W,GP.F,GP.P,},
		{GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,},
		{GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,},
		{GP.F,GP.F,GP.O,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,},
		{GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,},
		{GP.F,GP.F,GP.F,GP.F,GP.Z,GP.F,GP.F,GP.F,GP.F,GP.F,},
		{GP.F,GP.F,GP.F,GP.S,GP.F,GP.F,GP.F,GP.F,GP.F,GP.V,},
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

	private GP[,] survivorSmallHouse =
	{
		{GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F},
		{GP.F,GP.W,GP.W,GP.W,GP.W,GP.W,GP.F},
		{GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F},
		{GP.F,GP.F,GP.F,GP.V,GP.F,GP.W,GP.F},
		{GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F},
		{GP.F,GP.W,GP.W,GP.W,GP.W,GP.W,GP.F},
		{GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F},
	};

	private GP[,] emptyStreetWithDebrisOrTrees =
	{
		{GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F},
		{GP.F,GP.W,GP.F,GP.F,GP.F,GP.F,GP.F},
		{GP.F,GP.F,GP.F,GP.F,GP.W,GP.F,GP.F},
		{GP.F,GP.F,GP.F,GP.H,GP.W,GP.F,GP.F},
		{GP.F,GP.W,GP.F,GP.F,GP.F,GP.F,GP.F},
		{GP.H,GP.F,GP.W,GP.F,GP.F,GP.F,GP.F},
		{GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F},
	};

	private GP[,] whiteHouse = 
	{
		{GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F},
		{GP.F,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.F},
		{GP.F,GP.W,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.W,GP.F},
		{GP.F,GP.W,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.W,GP.F},
		{GP.F,GP.W,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.W,GP.F},
		{GP.F,GP.W,GP.W,GP.F,GP.W,GP.W,GP.W,GP.W,GP.W,GP.F,GP.W,GP.F,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.F,GP.W,GP.F},
		{GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.W,GP.F},
		{GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F,GP.C,GP.F,GP.F,GP.F,GP.W,GP.W,GP.F,GP.W,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F},
		{GP.F,GP.W,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F},
		{GP.F,GP.W,GP.F,GP.F,GP.C,GP.W,GP.C,GP.F,GP.F,GP.F,GP.F,GP.W,GP.F,GP.C,GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F},
		{GP.F,GP.C,GP.F,GP.F,GP.C,GP.W,GP.C,GP.F,GP.F,GP.F,GP.F,GP.F,GP.C,GP.P,GP.C,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F},
		{GP.F,GP.W,GP.F,GP.F,GP.C,GP.W,GP.C,GP.F,GP.F,GP.F,GP.F,GP.W,GP.F,GP.C,GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F},
		{GP.F,GP.W,GP.F,GP.F,GP.F,GP.F,GP.F,GP.C,GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F},
		{GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,GP.F,GP.F,GP.F,GP.W,GP.W,GP.F,GP.W,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F},
		{GP.F,GP.W,GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.W,GP.F},
		{GP.F,GP.W,GP.W,GP.F,GP.W,GP.W,GP.W,GP.W,GP.W,GP.F,GP.W,GP.F,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.F,GP.W,GP.F},
		{GP.F,GP.W,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.W,GP.F},
		{GP.F,GP.W,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.W,GP.F},
		{GP.F,GP.W,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.W,GP.F},
		{GP.F,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.F},
		{GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F},
	};

	private GP[,] bigHouse =
	{
		{GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F},
		{GP.F,GP.W,GP.W,GP.W,GP.W,GP.W,GP.F,GP.F,GP.W,GP.W,GP.W,GP.W,GP.W,GP.F},
		{GP.F,GP.W,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.W,GP.F},
		{GP.F,GP.W,GP.V,GP.W,GP.W,GP.W,GP.F,GP.F,GP.W,GP.W,GP.W,GP.F,GP.W,GP.F},
		{GP.F,GP.W,GP.F,GP.W,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.W,GP.F,GP.W,GP.F},
		{GP.F,GP.W,GP.F,GP.W,GP.V,GP.F,GP.F,GP.F,GP.F,GP.V,GP.W,GP.F,GP.W,GP.F},
		{GP.F,GP.F,GP.F,GP.W,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.W,GP.F,GP.W,GP.F},
		{GP.F,GP.W,GP.V,GP.W,GP.F,GP.F,GP.V,GP.F,GP.F,GP.F,GP.W,GP.F,GP.W,GP.F},
		{GP.F,GP.W,GP.F,GP.W,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.W,GP.V,GP.W,GP.F},
		{GP.F,GP.W,GP.F,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.F,GP.W,GP.F},
		{GP.F,GP.W,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.V,GP.F,GP.F,GP.F,GP.W,GP.F},
		{GP.F,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.W,GP.F},
		{GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F,GP.F},
	};

	private GP[,] zombie =
	{
		{GP.Z},
	};

	private GP[,] testTurnAndSkip = 
	{
		{GP.Z,GP.V,GP.Z},
	};

	private GP[,] testPresidentGameOver =
	{
		{GP.Z, GP.P},
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
			case GP.P:
				return PresidentPiecePrefab;
			case GP.H:
				return HazmatPiecePrefab;
			case GP.O:
				return FatZombie;
			case GP.S:
				return SkinnyZombie;
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

	public GP[,] GenerateMap()
	{
		GP[,] generatedMap = new GP[7*20, 7*20];

		// mark every area as unset
		for(var i = 0; i < generatedMap.GetLength(0); ++i)
			for(var j = 0; j < generatedMap.GetLength(1); ++j)
				generatedMap[i,j] = GP.I;

		// place the white house randomly (special one time only chunk)
		var presidentPlaced = false;
		while(!presidentPlaced)
		{
			presidentPlaced = TryToPlaceFeatureRandomly(whiteHouse, ref generatedMap);
		}
		
		// Try to add some other features!  The map is 140x140, keep in mind feature size and the fact that some will fail
		var features = new GP[][,] { survivorSmallHouse /* 7 */, bigHouse /* 14 */, emptyStreetWithDebrisOrTrees /* 7 */ };
		var maxToPlace = new int[]{ 30, 20, 40 };
		for (var i = 0; i < features.Length; ++i)
		{
			var addedCount = 0;
			for (var tries = 0; tries < maxToPlace[i]; ++tries)
			{
				addedCount += TryToPlaceFeatureRandomly(features[i], ref generatedMap) ? 1 : 0;
			}
			Debug.Log(string.Format("Feature {2}: added {0}/{1}", addedCount, maxToPlace[i], i));
		}

		// Place some random starting zombies.
		var startingZombies = 2;
		var zombiesAdded = 0;
		while (zombiesAdded < startingZombies)
		{
			zombiesAdded += TryToPlaceFeatureRandomly(zombie, ref generatedMap) ? 1 : 0;
		}
		
		// Replace all remaining empty spaces with floors
		for (var i = 0; i < generatedMap.GetLength(0); ++i)
			for (var j = 0; j < generatedMap.GetLength(1); ++j)
				if (generatedMap[i, j] == GP.I)
					generatedMap[i, j] = GP.F;

		return generatedMap;
	}

	public bool TryToPlaceFeatureRandomly(GP[,] feature, ref GP[,] map)
	{
		var randomH = UnityEngine.Random.Range(0, map.GetLength(0)-feature.GetLength(0));
		var randomV = UnityEngine.Random.Range(0, map.GetLength(1)-feature.GetLength(1));
		
		if(CanPlaceFeature(feature, ref map, randomH, randomV))
		{
			PlaceFeature(feature, ref map, randomH, randomV);
			return true;
		}
		return false;
	}

	public void PlaceFeature(GP[,] feature, ref GP[,] map, int hPos, int vPos)
	{
		for (var h = hPos; h < hPos + feature.GetLength(0); ++h)
		{
			for (var v = vPos; v < vPos + feature.GetLength(1); ++v)
			{
				map[h,v] = feature[h-hPos, v-vPos];
			}
		}
	}

	public bool CanPlaceFeature(GP[,] feature, ref GP[,] map, int hPos, int vPos)
	{
		for (var h = hPos; h < hPos + feature.GetLength(0); ++h)
		{
			for (var v = vPos; v < vPos + feature.GetLength(1); ++v)
			{
				if(map[h,v] != GP.I)
					return false;
			}
		}
		return true;
	}

	public void Start()
	{
		CreateGameBoard(GenerateMap());
		//CreateGameBoard(testPresidentGameOver);
		//CreateGameBoard(testMap);
		GameManager.Instance.Board.FinalizeBoardCreation();
	}
}