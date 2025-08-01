using Godot;
using System;

public partial class Camera3d : Camera3D
{
	[ExportGroup("Follow Settings")]
	[Export] public NodePath playerPath = ("%Player");
	[Export] public float followSpeed = 4.0f;
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

	private Node3D player;
	private Vector3 playerPosition;
	private float lastPlayerDirection = 1.0f;
	private Vector3 lookAheadOffset = Vector3.Zero;

	public override void _Ready() {
		if (playerPath != null) {
			player = GetNode<Node3D>(playerPath);
			if (player != null) {
				playerPosition = player.GlobalPosition;
				GlobalPosition = new Vector3(playerPosition.X, playerPosition.Y, 25.0f);
			
			} else {
				GD.PrintErr("Player not found!");
				}
		}
	}

	public override void _Process(double delta) {
		UpdateCameraPosition((float)delta);
	}

	private void UpdateCameraPosition(float delta) {
		Vector3 playerPos = player.GlobalPosition;
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
		Vector3 newPosition = new Vector3(playerPosition.X, playerPosition.Y + cameraYOffset, currentPos.Z);

		
		GlobalPosition = currentPos.Lerp(newPosition, followSpeed * delta);
		
	}
	


	private Vector3 GetPlayerVelocity()
	{
		if (player is CharacterBody3D characterBody) {
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

	
	public void SetPlayer(Node3D newPlayer)
	{
		player = newPlayer;
		if (player != null) {
			playerPosition = player.GlobalPosition;
		}
	}
}
