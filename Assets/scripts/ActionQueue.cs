using UnityEngine;
using System.Collections;

using System.Collections.Generic;

// Action Queue maintains the list of turns that are upcoming.
public class ActionQueue : MonoBehaviour 
{
	List<GameCharacterController> actionQueue;

	//---------------------------------------------------------------------------
	public void Start()
	{
		actionQueue = new List<GameCharacterController>();
		RegisterEvents();
	}

	//---------------------------------------------------------------------------
	public void OnDisable()
	{
		UnregisterEvents();
	}

	//---------------------------------------------------------------------------
	public void RegisterEvents()
	{
		GameManager.Instance.Board.PieceAdded += OnPieceAdded;
		GameManager.Instance.Board.PieceRemoved += OnPieceRemoved;
	}

	//---------------------------------------------------------------------------
	public void UnregisterEvents()
	{
		GameManager.Instance.Board.PieceAdded -= OnPieceAdded;
		GameManager.Instance.Board.PieceRemoved -= OnPieceRemoved;
	}

	//---------------------------------------------------------------------------
	void OnPieceAdded(GameBoard board, GamePiece piece)
	{
		AddCharacterToEndOfQueue(piece);
	}

	//---------------------------------------------------------------------------
	void OnPieceRemoved(GameBoard board, GamePiece piece)
	{
		RemovePiece(piece);
	}

	//---------------------------------------------------------------------------
	public bool RemovePiece(GamePiece piece)
	{
		var character = piece as Character;
		return character != null && actionQueue.Remove(character.Controller);
	}

	//---------------------------------------------------------------------------
	public void AddCharacterToEndOfQueue(GamePiece piece)
	{
		var character = piece as Character;
		if (character)
		{
			actionQueue.Add(character.Controller);
		}
	}

	//---------------------------------------------------------------------------
	public GameCharacterController GetNextActiveCharacter()
	{
		if (actionQueue != null && actionQueue.Count > 0)
		{
			var next = actionQueue[0];
			actionQueue.RemoveAt(0);
			actionQueue.Add(next);
			return next;
		}
		return null;
	}
}
