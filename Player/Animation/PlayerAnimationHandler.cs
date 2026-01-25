using System;
using Godot;

using SVector2 = System.Numerics.Vector2;

public partial class PlayerAnimationHandler : Node3D //goes on the playerMesh
{
	[Export] private CharacterBody3D player;
	private PlayerManager pm;
	[Export] public AnimationTree animTree;
	[Export] private string playbackFilePath; //ref to where we are in the animation state machine
	private AnimationNodeStateMachinePlayback playback;
	[Export] private string RunSpeedBlendPath { get; set; }
	[Export] private string AimBlendBlendPath { get; set; } //for blending the different aim directions
	[Export] private string LegAndArmBlendBlendPath { get; set; } //for choosing when to blend between normal animations and aiming
	[Export] private float transitionSpeed = 8f;
	[Export] private string JumpStateName;
	[Export] private string RunningStateName, WallCollisionStateName;
	[Export] private float runBlendSpeed = 2.3f; //run animation speed adjustment
	[Export] private string SlideStateName;
	[Export] private string CrouchStateName;
	[Export] private string HangingStateName;
	[Export] private float _aimingMaxTime = 2f; //how long shoot anim lasts before resetting to default
	private float aimingTimer = 99f; //set to -1 to constantly aim, no timer
	private float currentSpeed;
	public float newMeshRotation = 90f;//ex: -90 for left, 90 for right
	private float _meshRotationDegrees = 90f; //how much to rotate
	private SVector2 aimDirection = SVector2.Zero; //angle to position shooting arm during aiming mode

	public override void _Ready()
	{
		newMeshRotation = RotationDegrees.Y;
		_meshRotationDegrees = MathF.Abs(RotationDegrees.Y);
		playback = (AnimationNodeStateMachinePlayback)animTree.Get(playbackFilePath);
		pm = player.GetNode<PlayerManager>(player.GetPath());
	}

	public override void _PhysicsProcess(double delta)
	{
		if (player == null) { GD.Print("No player node assigned"); return; } //dont calculate if player hasn't been assigned

		if (currentSpeed != 0 || aimingTimer < _aimingMaxTime) //always exit idle when moving or aiming
		{
			animTree.Set("parameters/conditions/ExitIdle", true);
			animTree.Set("parameters/conditions/EnterIdle", false);
			if (aimingTimer != -1 ) //use timer if not -1
			{
				aimingTimer += (float)delta;
				if (currentSpeed != 0 && aimingTimer > 0.5f) aimingTimer = 0.5f; //reset timer when moving
			} 
		}
		else
        {
			animTree.Set("parameters/conditions/ExitIdle", false);
			animTree.Set("parameters/conditions/EnterIdle", true);
        }

		//find the value between current speed and desired speed
		currentSpeed = Mathf.Clamp(Mathf.MoveToward(currentSpeed, Mathf.Abs(player.Velocity.X), transitionSpeed * (float)delta), 0, 1f);

		if (pm.StateMachine._currentState.Name != "mantleState" && pm.StateMachine._currentState.Name != "slideState" 
		&& pm.StateMachine._currentState.Name != "walljumpState" //states to ignore rotation changes
		&& pm.aimDirection.X != 0 && newMeshRotation != _meshRotationDegrees * Mathf.Sign(pm.aimDirection.X))
        {
			newMeshRotation = _meshRotationDegrees * Mathf.Sign(pm.aimDirection.X);
			pm.facingDirection = Mathf.Sign(pm.aimDirection.X);
			pm.StateMachine._currentState.Set("turnAroundTimer", 0f);
        }

		if (newMeshRotation != RotationDegrees.Y)
		{
			var rotation = Mathf.MoveToward(RotationDegrees.Y, newMeshRotation, 35f);
			if (Mathf.Abs(rotation - newMeshRotation) < 2f) rotation = newMeshRotation; //snap to final rotation
			RotationDegrees = new Vector3(0, rotation, 0);
		}

		animTree.Set(RunSpeedBlendPath, currentSpeed * runBlendSpeed); //always blend animation tree with current speed
		animTree.Set(LegAndArmBlendBlendPath, currentSpeed);
		
		float yOffset = .2f * currentSpeed;
		SVector2 newAimDirect = new SVector2(pm.aimDirection.X, pm.aimDirection.Y + yOffset);
		/*if (Input.IsActionPressed("Aim"))
		{
			if (aimingTimer != -1) aimingTimer = .5f;
			newAimDirect = new (Mathf.Sign(Rotation.Y),1f);
		}*/

		if (newAimDirect != aimDirection && playback?.GetCurrentNode() != "Idle")
		{
			aimDirection = SVector2.Lerp(aimDirection, newAimDirect, .5f);
			animTree.Set(AimBlendBlendPath, new Vector2(Mathf.Abs(aimDirection.X), aimDirection.Y));
			animTree.Set("parameters/Crouching/AimBlend/blend_position", new Vector2(Mathf.Abs(aimDirection.X), aimDirection.Y)); //also set the crouching aim blend
			animTree.Set("parameters/Sliding/AimBlend/blend_position", new Vector2(aimDirection.X * pm.facingDirection, aimDirection.Y - yOffset));
		}
	}

	public void BeginJump()
	{
		playback?.Start(JumpStateName);
	}

	public void Grounded()
	{
		if (playback?.GetCurrentNode() != "Idle") playback?.Travel(RunningStateName);
	}

	public void Sliding(bool value) //value = slide true or sliding false
	{
		animTree.Set("parameters/conditions/slideEnd", !value); //set slideEnd true when Sliding(false) is called
		if (value) 
		{
			playback?.Travel(SlideStateName); //only transition to slide when true
			RotationDegrees = new Vector3(0, newMeshRotation, 0);
		}
	}

	public void RotateMesh(int direction)
	{
		newMeshRotation = _meshRotationDegrees * direction;
		pm.facingDirection = direction;
	}

	public void Crouch(bool force)
	{
		if (force) playback?.Start(CrouchStateName);
		else playback?.Travel(CrouchStateName);
	}

	public void WallCollided(bool colliding)
	{
		if (colliding) playback?.Travel(WallCollisionStateName);
		else playback?.Travel(RunningStateName);
	}

	public void StandingBlocked()
    {
		GD.Print("Play Stand blocked animation");
		animTree.Set("parameters/Crouching/OneShot_StandBlock/request", (int)AnimationNodeOneShot.OneShotRequest.Fire); //fire = 1
		GD.Print("Stand blocked animation played");
    }

	public void Hanging()
	{
		playback?.Travel(HangingStateName);
	}

	public void Aiming(bool isAiming)
	{
		if (isAiming) aimingTimer = -1;
		else
		aimingTimer = 0;
	}

	public void Shooting()
    {
        animTree.Set("parameters/Running/AimTimeSeek/seek_request", 0f); //reset shoot animation to start
		animTree.Set("parameters/Crouching/AimTimeSeek/seek_request", 0f); //reset shoot animation to start
		animTree.Set("parameters/Sliding/AimTimeSeek/seek_request", 0f); //reset shoot animation to start
    }

	public void Death()
    {
		animTree.Active = false;
		//StopAllAnimations();
        var animPlayer = GetParent().GetNode<AnimationPlayer>("AnimationPlayerMain");
		animPlayer.Play("Death");
    }
	public void StopAllAnimations()
	{
		if (playback != null)
		{
			playback.Stop();
		}
		
		// Reset all blend values to 0
		animTree.Set(RunSpeedBlendPath, 0f);
		animTree.Set(AimBlendBlendPath, Vector2.Zero);
		animTree.Set(LegAndArmBlendBlendPath, 0f);
		
		// Reset internal values
		currentSpeed = 0f;
		aimingTimer = 0f;
		aimDirection = SVector2.Zero;
	}
}
