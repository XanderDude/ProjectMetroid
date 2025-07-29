using Godot;
using System;

public partial class attackState : State
{
	
	[Export] private float projectileSpeed = -25.0f;
	
	
	public override void Enter() {
		projectileArrow = GetNode<CharacterBody3D>("..//..//%Arrow");
		if (projectileArrow == null) GD.Print( "CANT FIND ARROW");
		GD.Print("In Attack State");
		SetProjectileToPlayer();
		projectileArrow.Visible = true;
		
	}
	
	public override void _Ready() {
		
		
	}
	
	public void SetProjectileToPlayer() {
		
		projectileArrow.GlobalPosition = player.GlobalPosition + Vector3.Down;
		
		Vector3 offset = Vector3.Zero;
		
		if (parentMesch.RotationDegrees.Y > 0) offset.X = 0.5f;
		else offset.X = -0.5f;
		
		projectileArrow.GlobalPosition = player.GlobalPosition + offset + Vector3.Down * 0.6f;
	
	}
	
	public void FireProjectile(float delta) {
		if (parentMesh.RotationDegrees.Y > 0.0f) {
			
			projectileArrow.Velocity = new Vector3(Mathf.Abs(projectileSpeed),0, 0);
		}
		else {
			projectileArrow.Velocity = new Vector3(projectileSpeed,0, 0); 
			GD.Print("Facing Left");
		}
		
		
	}
	
	public override void PhysicsUpdate(float delta) {
		FireProjectile(delta);
	
		projectileArrow.MoveAndSlide();
	}
	
	
	public override void HandleInput(InputEvent @event) {
			
			
			if (@event.IsActionReleased("Shoot")) {
				GD.Print("Heading to groundedState");
				asm.TransitionTo("noattackState");
		}
		
			
	}
		
	
	
}
