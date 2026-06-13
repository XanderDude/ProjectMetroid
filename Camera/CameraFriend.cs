using Godot;
using System;

public partial class CameraFriend : Camera3D
{
	[ExportGroup("Follow Settings")]

	[Export] public float followSpeed = 8.0f;
	[Export] public float lookAheadDistance = 3.0f; 
	[Export] public float lookAheadSpeed = 2.0f; 
	[Export] public float cameraYOffset = 1.5f;
	
	[ExportGroup("Room Boundaries")]
	[Export] public float roomMinX = -10.0f;
	[Export] public float roomMaxX = 10.0f;
	[Export] public float roomMinY = -5.0f;
	[Export] public float roomMaxY = 5.0f;
	
	[Export] public float minMoveThreshold = 0.1f; 

	[Export] public float cameraDistance = 25.0f;

	private Vector3 playerPosition;
	private float lastPlayerDirection = 1.0f;
	private Vector3 lookAheadOffset = Vector3.Zero;

	public Vector3 camOffset = Vector3.Zero;

	public void init_camera()
	{
		if (GameFriend.gameinstance.player != null)
			{
				playerPosition = GameFriend.gameinstance.player.GlobalPosition;
				GlobalPosition = new Vector3(playerPosition.X, playerPosition.Y, 25.0f);

			}
	}

	public override void _Process(double delta) {
		UpdateCameraPosition((float)delta);
	}

	private void UpdateCameraPosition(float delta) {
		Vector3 playerPos = GameFriend.gameinstance.player.GlobalPosition;
		Vector3 playerVelocity = GetPlayerVelocity();
		if (Mathf.Abs(playerVelocity.X) > minMoveThreshold) {
			lastPlayerDirection = Mathf.Sign(playerVelocity.X);
		}

		
		Vector3 desiredLookAhead = new Vector3(lastPlayerDirection * lookAheadDistance, 0, 0);
		lookAheadOffset = lookAheadOffset.Lerp(desiredLookAhead, lookAheadSpeed * delta);

		
		playerPosition = playerPos + lookAheadOffset;
		playerPosition.X = Mathf.Clamp(playerPosition.X, roomMinX, roomMaxX);
		playerPosition.Y = Mathf.Clamp(playerPosition.Y, roomMinY, roomMaxY);
		
		Vector3 currentPos = GlobalPosition;

		if (playerVelocity.Y > -14.0f) {
			cameraYOffset = 1.5f;
		} else {
			cameraYOffset = -0.5f;
		}

		Vector3 newPosition;

		if (GameFriend.gameinstance.player.StateMachine._currentState.Name == "deathState")
		{
			newPosition = new Vector3(playerPosition.X, playerPosition.Y + cameraYOffset, cameraDistance) + camOffset;
			GlobalPosition = newPosition;
			GD.Print("Death State - Camera Locked to Player");
		}
		else
		{
			newPosition = new Vector3(playerPosition.X, playerPosition.Y + cameraYOffset, cameraDistance);
			GlobalPosition = currentPos.Lerp(newPosition, followSpeed * delta);
		}
	}
	


	private Vector3 GetPlayerVelocity()
	{
		if (GameFriend.gameinstance.player is CharacterBody3D characterBody) {
			return characterBody.Velocity;
		}

		
		return Vector3.Zero;
	}

	
	public void SetRoomBounds(float minX, float maxX, float minY, float maxY)
	{
		roomMinX = minX;
		roomMaxX = maxX;
		roomMinY = minY;
		roomMaxY = maxY;

		
	}

	public void ZoomTo(float targetDistance, float duration)
	{
		var tween = CreateTween();
		tween.TweenProperty(this, "cameraDistance", targetDistance, duration)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.InOut);
	}

	public void OffsetTo(Vector3 target, float duration)
	{
		var tween = CreateTween();
		tween.TweenProperty(this, "camOffset", target, duration)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.InOut);
	}

}
