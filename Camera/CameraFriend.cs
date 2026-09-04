using Godot;
using System;

public partial class CameraFriend : Camera3D
{
	[ExportGroup("Follow Settings")]

	[Export] public float followSpeed = 8.0f;
	[Export] public float lookAheadDistance = 3.0f; 
	[Export] public float lookAboveOrBelowDistance = 2.0f; 
	[Export] public float lookAheadSpeed = 2.0f; 
	[Export] public float cameraYOffset = 1.5f;
	
	[ExportGroup("Room Boundaries")]
	[Export] public float roomMinX = -10.0f;
	[Export] public float roomMaxX = 10.0f;
	[Export] public float roomMinY = -5.0f;
	[Export] public float roomMaxY = 5.0f;
	private Vector4 defaultBoundaries;
	[Export] public float minMoveThreshold = 0.1f; 

	[Export] public float cameraDistance = 25.0f;

	private Vector3 playerPosition;
	private float playerDirection = 1.0f;
	private float verticalLookDirection = 0f;
	private Vector3 lookAheadOffset, adjustedCamPos = Vector3.Zero;

	public void init_camera()
	{
		defaultBoundaries = new Vector4(roomMinX, roomMaxX, roomMinY, roomMaxY);
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
		playerPosition = GameFriend.gameinstance.player.GlobalPosition;
		Vector3 playerVelocity = GetPlayerVelocity();
		var lookSpeed = lookAheadSpeed;
		if (Mathf.Abs(playerVelocity.X) > minMoveThreshold) {
			playerDirection = Mathf.Sign(playerVelocity.X);
			verticalLookDirection = 0;
		}
		else if (playerVelocity.X == 0 && GameFriend.gameinstance.player.IsOnFloor() && GameFriend.gameinstance.player.aimDirection.Y != 0)
		{
			verticalLookDirection = Mathf.Sign(GameFriend.gameinstance.player.aimDirection.Y);
			lookSpeed = 4f;
		}
		else 
		{
			verticalLookDirection = 0;
			lookSpeed = lookAheadSpeed * 1.5f;
		}

		
		Vector3 desiredLookAheadPos = playerPosition + new Vector3(playerDirection * lookAheadDistance, verticalLookDirection * lookAboveOrBelowDistance, 0);
		desiredLookAheadPos.X = Mathf.Clamp(desiredLookAheadPos.X, roomMinX, roomMaxX);
		desiredLookAheadPos.Y = Mathf.Clamp(desiredLookAheadPos.Y, roomMinY, roomMaxY);
		lookAheadOffset = lookAheadOffset.Lerp(desiredLookAheadPos - playerPosition, lookSpeed * delta);

		adjustedCamPos = playerPosition + lookAheadOffset;
		adjustedCamPos.X = Mathf.Clamp(adjustedCamPos.X, roomMinX, roomMaxX);
		adjustedCamPos.Y = Mathf.Clamp(adjustedCamPos.Y, roomMinY, roomMaxY);

		if (playerVelocity.Y > -14.0f) {
			cameraYOffset = 1.5f;
		} else {
			cameraYOffset = -0.5f;
		}

		if (GameFriend.gameinstance.player.StateMachine._currentState.Name == "deathState")
		{
			GlobalPosition = new Vector3(playerPosition.X, playerPosition.Y, GlobalPosition.Z);
			GD.Print("Death State - Camera Locked to Player");
		}
		else
		{
			var newPosition = new Vector3(adjustedCamPos.X, adjustedCamPos.Y + cameraYOffset, cameraDistance);
			GlobalPosition = GlobalPosition.Lerp(newPosition, followSpeed * delta);
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

	public void SetToDefaultBounds()
	{
		roomMinX = defaultBoundaries.X;
		roomMaxX = defaultBoundaries.Y;
		roomMinY = defaultBoundaries.Z;
		roomMaxY = defaultBoundaries.W;
	}

	public void ZoomTo(float targetDistance, float duration)
	{
		var tween = CreateTween();
		tween.TweenProperty(this, "cameraDistance", targetDistance, duration)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.InOut);
	}

}
