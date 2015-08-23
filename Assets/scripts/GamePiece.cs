using UnityEngine;
using System.Collections;

public class GamePiece : MonoBehaviour {

	public int BoardHPos { get; set; }
	public int BoardVPos { get; set; }

	public bool IsFloorPiece = false;

	void OnMouseDown()
	{
		Debug.Log("Clicked on " + gameObject);
		// Is It a floor piece?
		GameManager.Instance.PlayerClickedSquare(this);
	}
}
