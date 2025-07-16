using Godot;
using System;

public partial class groundedState : State
{
	[Export] public float groundInitSpeed = 1.0f;
	[Export] public float groundMaxSpeed = 6.0f;
	[Export] public float groundAcceleration = 15.0f;
	[Export] public float groundDeacceleration = 15.0f;
	
	private CharacterBody3D player;
	private Node3D playerMesh;
	private Vector3 velocity;
	
	public override void Ready() {
		player = GetNode<CharacterBody3D>("../..");
		playerMesh = player.GetNode<Node3D>("Idle");
	}
	
	public override void Enter() {
		GD.Print("Entered Grounded State");
		velocity = player.Velocity;
		playerMesh.RotationDegrees = new Vector3(0, 0, 0);
	}
	
	public override void Exit() {
		GD.Print("Exited Grounded State");
	}
	
	public override void Update(float delta) {
		GD.Print("Grounded Update called");
		HandleGroundedMovement(delta);
		CheckTransitions();
	}
	
	public override void PhysicsUpdate(float delta) {
		player.Velocity = velocity;
		player.MoveAndSlide();
		velocity = player.Velocity;
	}
	
	public override void HandleInput(InputEvent @event) {
		if (@event.IsActionPressed("ui_accept") && player.IsOnFloor()) {
			msm.TransitionTo("jumpState");
		}
	}
	
	private void HandleGroundedMovement(float delta) {
		if (velocity.X > groundMaxSpeed) velocity.X = groundMaxSpeed;
		else if (velocity.X < -groundMaxSpeed) velocity.X = -groundMaxSpeed;
		
		if (Input.IsKeyPressed(Key.Left)) {
			velocity.X = -groundMaxSpeed;
			GD.Print("Moving left, velocity.X = " + velocity.X);
		}
		else if (Input.IsKeyPressed(Key.Right)) {
			velocity.X = groundMaxSpeed;
			GD.Print("Moving right, velocity.X = " + velocity.X);
		}
		else { 
			velocity.X = 0; 
		}
	}
	
	private void CheckTransitions() {
		if (!player.IsOnFloor()) {
			msm.TransitionTo("jumpState");
		}
		
		if (player.IsOnFloor() && Input.IsKeyPressed(Key.Down)) {
			msm.TransitionTo("slideState");
		}
	}
}
