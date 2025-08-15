using Godot;
using System;

public partial class PlayerAnimationHandler : Node3D //goes on the playerMesh
{
	[Export] private CharacterBody3D player;
	[Export] public AnimationTree animTree;
	[Export] private string playbackFilePath; //ref to where we are in the animation state machine
	private AnimationNodeStateMachinePlayback playback;
	[Export] private string WalkingBlendPath {get; set;}
	[Export] private string RunSpeedBlendPath {get; set;}
	[Export] private string AimBlendBlendPath {get; set;}
	[Export] private float transitionSpeed = 8f;
	[Export] private string JumpStateName;
	[Export] private string RunningStateName, WallCollisionStateName;
	[Export] private float runBlendSpeed = 2.3f; //run animation speed adjustment
	[Export] private string SlideStateName;
	[Export] private string CrouchStateName;
	[Export] private string HangingStateName;

	private float currentSpeed;

	private int currentDirection = 90; //-90 for left, 90 for right
	private Vector2 aimAngle; //angle to position shooting arm during aiming mode

	public override void _Ready()
	{
		//player = GetNode<Node3D>("%Player");
		//currentDirection = (int)playerMesh.Rotation.Y;
		playback = (AnimationNodeStateMachinePlayback)animTree.Get(playbackFilePath);
		animTree.Active = true;
	}

	public override void _Process(double delta)
	{
		if (player == null) { GD.Print("No player node assigned"); return; } //dont calculate if player hasn't been assigned 

		currentSpeed = Mathf.MoveToward(currentSpeed, Mathf.Abs(player.Velocity.X), (float)delta * transitionSpeed); //find the value between current speed and desired speed
																													 //if (newDelta > transitionSpeed * delta) //clamp new speed if it's greater than transition speed
																													 //	newDelta = transitionSpeed * (float)delta;

		if (player.Velocity.X != 0)
		{
			RotationDegrees = new Vector3(0, currentDirection * Mathf.Sign(player.Velocity.X), 0); //only rotate when moving
		}
		if (currentSpeed > 1) currentSpeed = 1;
		animTree.Set(WalkingBlendPath, currentSpeed); //always blend animation tree with current speed
		animTree.Set(RunSpeedBlendPath, currentSpeed * runBlendSpeed); //always blend animation tree with current speed
		Vector2 aimDirect = new Vector2(Mathf.Abs(Input.GetAxis("Left", "Right")), Input.GetAxis("Down", "Up")); //get up or down (1, -1,) and if holding a direction
		//only update aimBlend with aimDirection when in attack state
		animTree.Set(AimBlendBlendPath, aimDirect);
		//GD.Print(currentSpeed);
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

	public override void _UnhandledInput(InputEvent @inputEvent)
	{
		if (@inputEvent is InputEventJoypadMotion stickMotionEvent) //left stick motion during aiming mode
		{
			//get aim direction x and y

			//aimAngle = new Vector2(newX, newY);
		}
		if (@inputEvent is InputEventMouseMotion mouseMotionEvent) //mouse motion during aiming mode
		{
			Vector2 delta = mouseMotionEvent.Relative;
			aimAngle = delta;

			//get aim direction x and y
			//float newX
			//float newY
			//aimAngle = new Vector2(newX, newY);

			//remember to toggle mouse visiblity
		}
	}
}
