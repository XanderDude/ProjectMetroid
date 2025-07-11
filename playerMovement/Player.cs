using Godot;
using System;

public partial class Player : CharacterBody3D 
{
	[ExportGroup("Player Movement")]
	[ExportSubgroup("Grounded State")]
	[Export] public float groundInitSpeed = 1.0f;
	[Export] public float groundMaxSpeed = 6.0f;
	[Export] public float groundAcceleration = 3.0f;
	[Export] public float groundDeacceleration = 15.0f;
	
	private void groundedState(double delta, ref Vector3 velocity) {
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
	[Export] public float jumpMaxSpeed = 3.0f;
	[Export] public float jumpVelocity = 5.0f;
	[Export] public float jumpGravity = 9.8f;
	[Export] public float jumpMaxHeight = 0.4f;
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
		velocity.Y -= jumpGravity * (float)delta;
		if (isAscending(delta, ref velocity)) {
		
			if (Input.IsKeyPressed(Key.Left)) {
				velocity.X = -jumpMaxSpeed;
			}
			else if (Input.IsKeyPressed(Key.Right)) {
				velocity.X = jumpMaxSpeed;
			}
			else { 
				if ( velocity.X > 0.1f) velocity.X -= 0.1f;
				else if ( velocity.X < -0.1f) velocity.X += 0.1f;
				else { velocity.X = 0; }
			}
		}
		
		else {
			
			if (Input.IsKeyPressed(Key.Left)) {
				velocity.X = -jumpMaxSpeed / 2;
			}
			else if (Input.IsKeyPressed(Key.Right)) {
				velocity.X = jumpMaxSpeed / 2;
			}
			else { 
				if ( velocity.X > 0.1f) velocity.X = jumpMaxSpeed / 2;
				else if ( velocity.X < -0.1f) velocity.X = -jumpMaxSpeed / 2;
				else { velocity.X = 0; }
			}
		}
	
	}
	
	public override void _PhysicsProcess(double delta) {
		Vector3 velocity = Velocity;
		GD.Print("Velocity X " + velocity.X);
		GD.Print("velocity.Y " + velocity.Y);
	 	
	if (IsOnFloor()) {
		groundedState(delta, ref velocity);
	}
	else if (!IsOnFloor()) {
		jumpState(delta, ref velocity);
	}
	if (Input.IsKeyPressed(Key.Space) && IsOnFloor()) { 
		jumpHeight = 0.0f;
		velocity.Y = jumpVelocity;
		
	}
	
	Velocity = velocity;
	MoveAndSlide();
	}
}
