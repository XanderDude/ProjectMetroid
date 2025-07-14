using Godot;
using System;

public partial class Player : CharacterBody3D 
{
	[ExportGroup("Player Movement")]
	[ExportSubgroup("Grounded State")]
	[Export] public float groundInitSpeed = 1.0f;
	[Export] public float groundMaxSpeed = 6.0f;
	[Export] public float groundAcceleration = 15.0f;
	[Export] public float groundDeacceleration = 15.0f;
	
	private void groundedState(double delta, ref Vector3 velocity) {
			playerMesh.RotationDegrees = new Vector3(0, 0, 0);
			if (Input.IsKeyPressed(Key.Left)) {
				if (velocity.X > -groundMaxSpeed) velocity.X -= groundAcceleration * (float)delta; 
			}
			else if (Input.IsKeyPressed(Key.Right)) {
				if (velocity.X <= groundMaxSpeed) velocity.X += groundAcceleration * (float)delta; 
			}
			else { 
		  		if ( velocity.X > 0.1) velocity.X -= groundDeacceleration * (float)delta;
				else if ( velocity.X < -0.1) velocity.X += groundDeacceleration * (float)delta;
				else { velocity.X = 0; }
			}
		}
	
	[ExportSubgroup("Jump State")]
	[Export] public float jumpInitSpeed = 1.0f;
	[Export] public float jumpAcceleration = 3.0f;
	[Export] public float jumpMaxSpeed = 5.0f;
 	 [Export] public float jumpVelocity = 5.0f;
	[Export] public float jumpGravity = 9.8f;
	[Export] public float jumpMaxHeight = 0.35f;
	public float jumpHeight = 0.0f;
	
	
	private bool isAscending(double delta, ref Vector3 velocity) {
		if (Input.IsKeyPressed(Key.Space) && jumpHeight < jumpMaxHeight) { 
			velocity.Y = jumpVelocity;
			jumpHeight += (float) delta; 
			GD.Print("Jump Height: " + jumpHeight);
			return true; 
		}
		else {
			jumpHeight = jumpMaxHeight;
			return false;
		}
	}
	
	private void jumpState(double delta, ref Vector3 velocity) {
		playerMesh.RotationDegrees = new Vector3(0, 0, 0);
		velocity.Y -= jumpGravity * (float)delta;
		
		if (Input.IsKeyPressed(Key.Down)) {
				velocity.Y -= (jumpGravity * 4) * (float)delta;
				
			}
		
		if (isAscending(delta, ref velocity)) {
		
			if (Input.IsKeyPressed(Key.Left)) {
				velocity.X = -jumpMaxSpeed;
			}
			else if (Input.IsKeyPressed(Key.Right)) {
				velocity.X = jumpMaxSpeed;
			}
			else { 
				if ( velocity.X > 0.1f) velocity.X -= 1.0f;
				else if ( velocity.X < -0.1f) velocity.X += 1.0f;
				else { velocity.X = 0; }
			}
		}
		
		else {
			
			if (Input.IsKeyPressed(Key.Left)) {
				velocity.X = -jumpMaxSpeed;
			}
			else if (Input.IsKeyPressed(Key.Right)) {
				velocity.X = jumpMaxSpeed ;
			}
			else { 
				if ( velocity.X > 0.1f) velocity.X = 0.5f;
				else if ( velocity.X < -0.1f) velocity.X = -0.5f;
				
			}
		}
	
	}

	private Node3D playerMesh;
	
	
	public override void _Ready() { 
		playerMesh = GetNode<Node3D>("Idle");
		
	}
	
	
	private bool slideEnter = true;
	private void slideState(double delta, ref Vector3 velocity) {
		
		playerMesh.RotationDegrees = new Vector3(0, 0, 90);
		if (slideEnter) { 
		if (velocity.X > 0) velocity.X = groundMaxSpeed + 6.0f;
		else if (velocity.X <= 0) { velocity.X = -groundMaxSpeed - 6.0f; }
		slideEnter = false;
		}
		if (velocity.X > 0.1f) velocity.X -= groundDeacceleration * (float)delta * 2;
		else if ( velocity.X < -0.1f) velocity.X += groundDeacceleration * (float)delta * 2;
		else { velocity.X = 0; }
		
	}
	
	public override void _PhysicsProcess(double delta) {
		Vector3 velocity = Velocity;
		
		GD.Print("Velocity X " + velocity.X);
		GD.Print("velocity.Y " + velocity.Y);
	 	
	if (IsOnFloor()) {
		groundedState(delta, ref velocity);
	}
	else if (!IsOnFloor()) {
		slideEnter = true;
		jumpState(delta, ref velocity);
	}
	if (Input.IsKeyPressed(Key.Space) && IsOnFloor()) { 
		jumpHeight = 0.0f;
		velocity.Y = jumpVelocity;
		
	}
	if (IsOnFloor() && Input.IsKeyPressed(Key.Down)) {
		slideState(delta, ref velocity);
	}
	
	Velocity = velocity;
	MoveAndSlide();
	}
}
