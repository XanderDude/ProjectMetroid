using Godot;

using SVector2 = System.Numerics.Vector2;

public partial class PlayerAnimationHandler : Node3D //goes on the playerMesh
{
	[Export] private CharacterBody3D player;
	[Export] public AnimationTree animTree;
	[Export] private string playbackFilePath; //ref to where we are in the animation state machine
	private AnimationNodeStateMachinePlayback playback;
	[Export] private string WalkingBlendPath { get; set; }
	[Export] private string RunSpeedBlendPath { get; set; }
	[Export] private string AimBlendBlendPath { get; set; } //for blending the different aim directions
	[Export] private string LegAndArmBlendBlendPath { get; set; } //or choosing when to blend between normal animations and aiming
	[Export] private float transitionSpeed = 8f;
	[Export] private string JumpStateName;
	[Export] private string RunningStateName, WallCollisionStateName;
	[Export] private float runBlendSpeed = 2.3f; //run animation speed adjustment
	[Export] private string SlideStateName;
	[Export] private string CrouchStateName;
	[Export] private string HangingStateName;

	[Export] private float shootingAnimTime = 2f; //how long shoot anim lasts before resetting to default
	private float shootingTimer = 99f; //set to -1 to constantly aim, no timer

	private float currentSpeed;

	private int currentDirection = 90; //-90 for left, 90 for right
	private SVector2 aimDirection = SVector2.Zero; //angle to position shooting arm during aiming mode

	public override void _Ready()
	{
		currentDirection *= Mathf.Sign(GlobalRotation.Y);
		playback = (AnimationNodeStateMachinePlayback)animTree.Get(playbackFilePath);
	}

	public override void _Process(double delta)
	{
		if (player == null) { GD.Print("No player node assigned"); return; } //dont calculate if player hasn't been assigned

		if (shootingTimer != -1 && shootingTimer < shootingAnimTime) shootingTimer += (float)delta;

		//find the value between current speed and desired speed
		currentSpeed = Mathf.Clamp(Mathf.MoveToward(currentSpeed, Mathf.Abs(player.Velocity.X), 2), 0, 1f);


		if (player.Velocity.X != 0) //direction has changed
		{
			RotationDegrees = new Vector3(0, currentDirection * Mathf.Sign(player.Velocity.X), 0); //rotate mesh
		}
		else if (playback.GetCurrentNode() == "Crouching" && Input.GetAxis("Left", "Right") != 0) //holding a direction while crouching
		{
			RotationDegrees = new Vector3(0, currentDirection * Mathf.Sign(Input.GetAxis("Left", "Right")), 0);
		}

		animTree.Set(WalkingBlendPath, currentSpeed); //always blend animation tree with current speed
		animTree.Set(RunSpeedBlendPath, currentSpeed * runBlendSpeed); //always blend animation tree with current speed
		SVector2 newAimDirect = new SVector2(Mathf.Abs(Input.GetAxis("Left", "Right")), Input.GetAxis("Down", "Up")); //get up or down (1, -1,) and if holding a direction

		if (newAimDirect != aimDirection)
		{
			aimDirection = SVector2.Lerp(aimDirection, newAimDirect, (float)delta * transitionSpeed);
			animTree.Set(AimBlendBlendPath, new Vector2(aimDirection.X, aimDirection.Y));
		}

		//only blend upper body when aiming
		if (shootingTimer < shootingAnimTime) animTree.Set(LegAndArmBlendBlendPath, Mathf.MoveToward((float)animTree.Get(LegAndArmBlendBlendPath), 1, (float)delta * transitionSpeed));
		else animTree.Set(LegAndArmBlendBlendPath, Mathf.MoveToward((float)animTree.Get(LegAndArmBlendBlendPath), 0, (float)delta * (transitionSpeed/3)));
	}

	public void BeginJump()
	{
		playback?.Travel(JumpStateName);
	}

	public void Grounded()
	{
		playback?.Travel(RunningStateName);
	}

	public void Sliding(bool value) //value = slide true or sliding false
	{
		animTree.Set("parameters/conditions/slideEnd", !value); //set slideEnd true when Sliding(false) is called
		if (!player.IsOnFloor()) playback?.Travel(JumpStateName);
		else if (value) playback?.Travel(SlideStateName); //only transition to slide when true
	}

	public void Crouch(bool value)
	{
		if (value) playback?.Travel(CrouchStateName);
		else playback?.Travel(RunningStateName);
	}

	public void WallCollided()
	{
		if (Mathf.Abs(player.Velocity.X) == 0) playback?.Travel(WallCollisionStateName);
	}

	public void Hanging()
	{
		playback?.Travel(HangingStateName);
	}

	public void Aiming(bool isAiming)
	{
		if (isAiming) shootingTimer = -1;
		else
		shootingTimer = 0;
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
		animTree.Set(WalkingBlendPath, 0f);
		animTree.Set(RunSpeedBlendPath, 0f);
		animTree.Set(AimBlendBlendPath, Vector2.Zero);
		animTree.Set(LegAndArmBlendBlendPath, 0f);
		
		// Reset internal values
		currentSpeed = 0f;
		shootingTimer = 0f;
		aimDirection = SVector2.Zero;
	}
}
