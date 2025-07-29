using Godot;
using System;

public partial class attackState : State
{
	[Export] private float projectileSpeed = 25.0f;
	[Export] private float maxDistance = 50.0f;
	[Export] private int maxProjectilesOnScreen = 3;
	[Export] private PackedScene arrowScene;
	[Export] private float fireRate = 0.3f;
	private MovementStateMachine msm;
	
	private SimpleProjectileManager projectileManager;
	private float timeSinceLastShot = 0f;
	
	public override void _Ready() 
	{
		projectileManager = GetNode<SimpleProjectileManager>("ProjectileManager");
		
		if (projectileManager == null)
		{
			GD.PrintErr("ProjectileManager not found!");
		}
	}
	
	public override void Enter() 
	{
		if (projectileManager != null)
		{
			FireProjectile();
			timeSinceLastShot = 0f;
		}
	}
	
	public override void Exit()
	{
		timeSinceLastShot = 0f;
	}
	
	private void FireProjectile()
	{
		if (projectileManager == null) return;
		
		bool facingRight = parentMesh.RotationDegrees.Y > 0;
		bool aimingUp = Input.IsActionPressed("Aim");
		bool pressingUp = Input.IsActionPressed("Up");
		bool pressingDown = Input.IsActionPressed("Down");
		
		msm = GetNode<MovementStateMachine>("../../MovementStateMachine");
		bool isCurrentlyJumping = (msm._currentState != null && msm._currentState.Name == "jumpState");
		
		if (pressingUp)
		{
			// Up arrow key = shoot straight up
			projectileManager.CreateUpwardProjectile(
				player,
				projectileSpeed,
				facingRight,
				maxDistance * 2.0f
			);
		}
		else if (pressingDown && isCurrentlyJumping)
		{
			// Down arrow key + was jumping = shoot straight down
			projectileManager.CreateDownwardProjectile(
				player,
				projectileSpeed,
				facingRight,
				maxDistance * 1.5f
			);
		}
		else if (aimingUp)
		{
			// Aim button = 45 degree angle
			projectileManager.CreateAngledProjectile(
				player,
				projectileSpeed,
				facingRight,
				45.0f,
				maxDistance * 1.5f
			);
		}
		else
		{
			// Normal horizontal shot
			projectileManager.CreateProjectile(
				player, 
				projectileSpeed, 
				facingRight, 
				maxDistance
			);
		}
	}
	
	public override void PhysicsUpdate(float delta) 
	{
		timeSinceLastShot += delta;
		
		if (timeSinceLastShot >= fireRate)
		{
			FireProjectile();
			timeSinceLastShot = 0f;
		}
	}
	
	public override void HandleInput(InputEvent @event) 
	{
		if (@event.IsActionReleased("Shoot")) 
		{
			asm.TransitionTo("noattackState");
		}
	}
	
	public void ClearAllProjectiles()
	{
		projectileManager?.ClearAllProjectiles();
	}
	
	public int GetActiveProjectileCount()
	{
		return projectileManager?.GetActiveProjectileCount() ?? 0;
	}
}
