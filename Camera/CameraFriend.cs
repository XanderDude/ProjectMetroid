using Godot;
using System;

public partial class CameraFriend : Camera3D
{
	[ExportGroup("Follow Settings")]

	[Export] public float followSpeed = 10.0f;
	[Export] public float lookAheadDistance = 1.5f; 
	[Export] public float lookAboveOrBelowDistance = 2.0f; 
	[Export] public float lookAheadSpeed = 2.0f; 
	[Export] public float cameraYOffset = 1.5f;
	
	[ExportGroup("Default Boundaries")]
	[Export] public float minBoundX = -100f;
	[Export] public float minBoundY = -100f;
	[Export] public float maxBoundX = 100f;
	private Vector4 defaultBoundaries, newBounds;

	[Export] public float minMoveThreshold = 0.1f; 

	[Export] public float cameraDistance = 25.0f;

	private Vector3 playerPosition;
	private float playerDirection = 1.0f;
	private float verticalLookDirection = 0f;
	private Vector3 lookAheadDist, adjustedCamPos = Vector3.Zero;
	private float lookDelayTimer, lookDelay = .4f;

	public void init_camera()
	{
		defaultBoundaries = new Vector4(minBoundX, minBoundY, maxBoundX, maxBoundY); //(bottom-left, top-right)
		newBounds = defaultBoundaries;
		if (GameFriend.gameinstance.player != null)
		{
			playerPosition = GameFriend.gameinstance.player.GlobalPosition;
			GlobalPosition = new Vector3(playerPosition.X, playerPosition.Y, cameraDistance);
		}
	}

	public override void _Process(double delta) 
	{
		UpdateCameraPosition((float)delta);
		if (newBounds != GetCurrentBounds()) TransitionCamerBounds((float)delta); //new bounds, transition current bounds to new values
	}
	
	private void TransitionCamerBounds(float delta)
	{
		if (GetCurrentBounds().DistanceSquaredTo(newBounds) < .001f) SetCurrentBounds(newBounds);
		else
		{
			SetCurrentBounds(GetCurrentBounds().Lerp(newBounds, 6f * delta));
		}
	}

	private void UpdateCameraPosition(float delta) {
		playerPosition = GameFriend.gameinstance.player.GlobalPosition;
		Vector3 playerVelocity = GetPlayerVelocity();
		var lookSpeed = lookAheadSpeed;
		var camYOffset = cameraYOffset;
		playerDirection = GameFriend.gameinstance.player.facingDirection;
		if (Mathf.Abs(playerVelocity.X) > minMoveThreshold) {
			verticalLookDirection = 0;
			lookDelayTimer = lookDelay;
		}
		else if (playerVelocity.X == 0 && GameFriend.gameinstance.player.IsOnFloor() && GameFriend.gameinstance.player.aimDirection.Y != 0)
		{
			lookDelayTimer -= delta;
			if (lookDelayTimer <= 0)
			{
				verticalLookDirection = Mathf.Sign(GameFriend.gameinstance.player.aimDirection.Y);
				lookSpeed = lookAheadSpeed * 3;
			}
		}
		else 
		{
			verticalLookDirection = 0;
			lookDelayTimer = lookDelay;
			lookSpeed = lookAheadSpeed * 3;
		}

		if (playerVelocity.Y < -14.0f)
		{
			camYOffset -= 2f;
		} 

		//(1) Get desired camera position according to player's orientation, movement, and current bounds
		Vector3 desiredLookAheadDist = new Vector3(playerDirection * lookAheadDistance, verticalLookDirection * lookAboveOrBelowDistance + camYOffset, 0);
		desiredLookAheadDist = new Vector3 (
			Mathf.Clamp(desiredLookAheadDist.X + playerPosition.X, minBoundX, maxBoundX), 
			Mathf.Clamp(desiredLookAheadDist.Y + playerPosition.Y, minBoundY, maxBoundY), playerPosition.Z) - playerPosition;

		//(2) Smoothly assign new camera position and clamp
		lookAheadDist = lookAheadDist.Lerp(desiredLookAheadDist, lookSpeed * delta);
		adjustedCamPos = lookAheadDist + playerPosition;
		
		adjustedCamPos.X = Mathf.Clamp(adjustedCamPos.X, minBoundX, maxBoundX);
		adjustedCamPos.Y = Mathf.Clamp(adjustedCamPos.Y, minBoundY, maxBoundY);

		//(3) Apply position to camera transform
		if (GameFriend.gameinstance.player.StateMachine._currentState.Name == "deathState")
		{
			GlobalPosition = new Vector3(playerPosition.X, playerPosition.Y, cameraDistance);
			GD.Print("Death State - Camera Locked to Player");
		}
		else
		{
			var newPosition = new Vector3(adjustedCamPos.X, adjustedCamPos.Y, cameraDistance);
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

	
	public void SetNewCamBounds(float minX, float minY, float maxX, float maxY, bool snap = true)
	{
		if (snap) //skip smooth transition
		{
			minBoundX = minX;
			minBoundY = minY;
			maxBoundX = maxX;

			maxBoundY = maxY;
		}
		newBounds = new(minX, minY, maxX, maxY);
	}

	public void SetNewCamBounds(Vector4 minMax, bool snap = true)
	{
		if (snap) //skip smooth transition
        {
            SetCurrentBounds(minMax);
        }
        newBounds = minMax;
	}

    private void SetCurrentBounds(Vector4 minMax)
    {
        minBoundX = minMax.X;
        minBoundY = minMax.Y;
        maxBoundX = minMax.Z;
        maxBoundY = minMax.W;
    }

	private Vector4 GetCurrentBounds() //returns roomMin and Max as a vector4
	{
		Vector4 bounds = new(minBoundX, minBoundY, maxBoundX, maxBoundY);
		return bounds;
	}

	public void ZoomTo(float targetDistance, float duration)
	{
		var tween = CreateTween();
		tween.TweenProperty(this, "cameraDistance", targetDistance, duration)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.InOut);
	}

	public void SetToDefaultBounds()
	{
		minBoundX = defaultBoundaries.X;
		minBoundY = defaultBoundaries.Y;
		maxBoundX = defaultBoundaries.Z;
		maxBoundY = defaultBoundaries.W;
	}
}
