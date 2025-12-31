using Godot;

using SVector2 = System.Numerics.Vector2;

public partial class PlayerAnimationHandler : Node3D //goes on the playerMesh
{
	[Export] private CharacterBody3D player;
	private PlayerManager pm;
	[Export] public AnimationTree animTree;
	[Export] private string playbackFilePath; //ref to where we are in the animation state machine
	private AnimationNodeStateMachinePlayback playback;
	[Export] private string IdleBlendPath { get; set; }
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
	[Export] private float shootingAnimTime = 2f; //how long shoot anim lasts before resetting to default
	private float aimingTimer = 99f; //set to -1 to constantly aim, no timer
	private float currentSpeed;
	private int meshRotation = 90; //-90 for left, 90 for right
	private SVector2 aimDirection = SVector2.Zero; //angle to position shooting arm during aiming mode

	public override void _Ready()
	{
		meshRotation *= Mathf.Sign(GlobalRotation.Y);
		playback = (AnimationNodeStateMachinePlayback)animTree.Get(playbackFilePath);
		pm = player.GetNode<PlayerManager>(player.GetPath());
	}

	public override void _Process(double delta)
	{
		if (player == null) { GD.Print("No player node assigned"); return; } //dont calculate if player hasn't been assigned

		if (aimingTimer == -1) animTree.Set(IdleBlendPath, 0); //aiming no timer
		else if (aimingTimer < shootingAnimTime) //aiming with timer
		{
			aimingTimer += (float)delta;
			animTree.Set(IdleBlendPath, 0);
		}
		else animTree.Set(IdleBlendPath, Mathf.MoveToward((float)animTree.Get(IdleBlendPath), 1, (float)delta * (transitionSpeed/3))); //idling

		//find the value between current speed and desired speed
		currentSpeed = Mathf.Clamp(Mathf.MoveToward(currentSpeed, Mathf.Abs(player.Velocity.X), 1), 0, 1f);

		if (pm.StateMachine._currentState.Name != "mantleState" && pm.StateMachine._currentState.Name != "slideState" 
		&& pm.StateMachine._currentState.Name != "walljumpState" && pm.aimDirection.X != 0) //player is moving, check for input to rotate mesh
		{
			RotationDegrees = new Vector3(0, meshRotation * Mathf.Sign(pm.aimDirection.X), 0); //rotate mesh
		}

		//animTree.Set(IdleBlendPath, currentSpeed); //always blend animation tree with current speed
		animTree.Set(RunSpeedBlendPath, currentSpeed * runBlendSpeed); //always blend animation tree with current speed
		animTree.Set(LegAndArmBlendBlendPath, currentSpeed);
		
		float yOffset = .2f * currentSpeed;
		SVector2 newAimDirect = new SVector2(Mathf.Abs(pm.aimDirection.X), pm.aimDirection.Y + yOffset); //get up or down (1, -1,) and if holding a direction
		if (Input.IsActionPressed("Aim"))
		{
			if (aimingTimer != -1) aimingTimer = .5f;
			newAimDirect = new (1f,1f);
		}

		if (newAimDirect != aimDirection)
		{
			aimDirection = SVector2.Lerp(aimDirection, newAimDirect, .5f);
			animTree.Set(AimBlendBlendPath, new Vector2(aimDirection.X, aimDirection.Y));
			animTree.Set("parameters/Crouching/AimBlend/blend_position", new Vector2(aimDirection.X, aimDirection.Y)); //also set the crouching aim blend
		}

		//only blend upper (aiming) and lower (idle and running) body when idle or running
		//if () animTree.Set(LegAndArmBlendBlendPath, Mathf.MoveToward((float)animTree.Get(LegAndArmBlendBlendPath), 1, (float)delta * transitionSpeed));
		//else animTree.Set(LegAndArmBlendBlendPath, Mathf.MoveToward((float)animTree.Get(LegAndArmBlendBlendPath), 0, (float)delta * (transitionSpeed/3)));
	}

	public void BeginJump()
	{
		playback?.Start(JumpStateName);
		//playback?.Travel(JumpStateName);
	}

	public void Grounded()
	{
		playback?.Travel(RunningStateName);
	}

	public void Sliding(bool value) //value = slide true or sliding false
	{
		animTree.Set("parameters/conditions/slideEnd", !value); //set slideEnd true when Sliding(false) is called
		//if (!player.IsOnFloor()) playback?.Travel(JumpStateName);
		if (value) playback?.Travel(SlideStateName); //only transition to slide when true
	}

	public void Crouch(bool value)
	{
		if (value) playback?.Travel(CrouchStateName);
		else playback?.Travel(RunningStateName);
	}

	public void WallCollided(bool colliding)
	{
		if (colliding) playback?.Travel(WallCollisionStateName);
		else playback?.Travel(RunningStateName);
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
        //play shooting animation
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
		animTree.Set(IdleBlendPath, 0f);
		animTree.Set(RunSpeedBlendPath, 0f);
		animTree.Set(AimBlendBlendPath, Vector2.Zero);
		animTree.Set(LegAndArmBlendBlendPath, 0f);
		
		// Reset internal values
		currentSpeed = 0f;
		aimingTimer = 0f;
		aimDirection = SVector2.Zero;
	}
}
