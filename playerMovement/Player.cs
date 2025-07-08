using Godot;
using System;

public partial class Player : CharacterBody3D 
{
	[ExportGroup("Player Movement")]
	[ExportSubgroup("Grounded State")]
	[Export] public float groundInitSpeed = 1.0f;
	[Export] public float groundMaxSpeed = 6.0f;
	[Export] public float groundAcceleration = 3.0f;
	[Export] public float groundDeacceleration = 6.0f;
	
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
	[Export] public float jumpVelocity = 9.0f;
	[Export] public float jumpGravity = 9.8f;
	
	private void jumpState(double delta, ref Vector3 velocity) {
		
		velocity.Y -= jumpGravity * (float)delta;
		
		if (velocity.Y < 6.5f) velocity.Y -= jumpGravity * (float)delta;
		
		if (Input.IsKeyPressed(Key.Left)) {
			velocity.X = -jumpMaxSpeed;
		}
		else if (Input.IsKeyPressed(Key.Right)) {
			velocity.X = jumpMaxSpeed;
		}
		
		
		else { 
			if ( velocity.X > 0.1f) velocity.X = jumpMaxSpeed;
			else if ( velocity.X < -0.1f) velocity.X = -jumpMaxSpeed;
			else { velocity.X = 0; }
		}
	}
	public override void _PhysicsProcess(double delta) {
		Vector3 velocity = Velocity;
		GD.Print("Velocity X " + velocity.X);
		GD.Print("velocity.Y " + velocity.Y);
		GD.Print("Time " + delta);
	 
	if (IsOnFloor()) {
		groundedState(delta, ref velocity);
	}
	else if (!IsOnFloor()) {
		jumpState(delta, ref velocity);
	}
	if (Input.IsKeyPressed(Key.Space) &&   IsOnFloor()) { 
		velocity.Y = jumpVelocity;
	}
	
	Velocity = velocity;
	MoveAndSlide();
	}
}
