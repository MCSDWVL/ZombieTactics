using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour 
{
	private Vector3 lastMousePos;
	private bool lastRightClick;
	private Vector3 mouseVelocity;

	public float MouseDeltaAmplifier = 0.01f;
	public float VelocityDampingCoefficient = 0.9f;
	public float MinVelocity = 0.1f;

	public float MinCameraX { get; set; }
	public float MaxCameraX { get; set; }
	public float MinCameraY { get; set; }
	public float MaxCameraY { get; set; }

	public bool CenteredOnActivePiece { get; set; }

	// Update is called once per frame
	void LateUpdate () 
	{
		var rightclicked = Input.GetMouseButton(1);

		if (rightclicked || mouseVelocity.sqrMagnitude > MinVelocity)
		{
			var inputVector = Vector3.zero;
			if (rightclicked)
			{
				// Cancel any fly to 
				flyTimeRemaining = -1;

				// check for movement from last update
				if (lastRightClick)
				{
					// what's the delta
					inputVector = Input.mousePosition - lastMousePos;
					mouseVelocity = inputVector;
				}
			}
			else
			{
				inputVector = mouseVelocity;
				mouseVelocity *= VelocityDampingCoefficient;
			}

			// track last mouse position
			lastMousePos = Input.mousePosition;

			// calculate new camera transform
			Camera.main.transform.position = CalculateCameraPosition(inputVector);
		}
		else if (flyTimeRemaining > 0)
		{
			Camera.main.transform.position = Vector3.Lerp(startPosition, targetPosition, (FlyTime - flyTimeRemaining) / FlyTime);
			flyTimeRemaining -= Time.deltaTime;
		}

		// track if right click is being held down
		lastRightClick = rightclicked;
	}


	private Vector3 CalculateCameraPosition(Vector3 inputVector)
	{
		var newPosition = Camera.main.transform.position - MouseDeltaAmplifier * inputVector;
		newPosition.x = Mathf.Clamp(newPosition.x, MinCameraX, MaxCameraX);
		newPosition.y = Mathf.Clamp(newPosition.y, MinCameraY, MaxCameraY);
		return newPosition;
	}

	public float FlyTime = 0.3f;
	private Vector3 startPosition;
	private Vector3 targetPosition;
	private float flyTimeRemaining = -1f;
	public void FlyToPiece(GamePiece piece)
	{
		var rightclicked = Input.GetMouseButton(1);
		if(rightclicked)
			return;

		// cancel any camera velocity
		mouseVelocity = Vector3.zero;

		// start flying
		flyTimeRemaining = FlyTime;
		startPosition = Camera.main.transform.position;
		targetPosition = GameManager.Instance.Board.GetWorldPositionForBoardPiece(piece);
		targetPosition.z = startPosition.z;
	}
}
