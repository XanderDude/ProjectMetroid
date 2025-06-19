using Godot;
using System;

public partial class Player : CharacterBody3D 
{
	[Export] public float Speed = 5.0f;
	[Export] public float JumpVelocity = 4.0f;
	[Export] public float gravity = 12.0f;
	
	public override void _PhysicsProcess(double delta) 
	{
		Vector3 velocity = Velocity;
		
		if (!IsOnFloor()) velocity.Y -= gravity * (float)delta;
		
		velocity.X = 0;
		if (Input.IsKeyPressed(Key.Left)) velocity.X = -Speed;
		if (Input.IsKeyPressed(Key.Right)) velocity.X = Speed;
		if (Input.IsKeyPressed(Key.Space) && IsOnFloor()) {
			velocity.Y = JumpVelocity;
		}
		
		Velocity = velocity;
		MoveAndSlide();
	}
}
