using Godot;
using System;
public partial class attackState : State
{
	[Export] private float projectileSpeed = 25.0f;
	[Export] private float maxDistance = 50.0f;
	[Export] private int maxProjectilesOnScreen = 3;
	[Export] private PackedScene arrowScene;

	
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
		
		// Simple projectile - crossbow animation handles everything
		projectileManager.CreateProjectile(
			player, 
			projectileSpeed, 
			facingRight, 
			maxDistance
		);
	}
	
	public override void PhysicsUpdate(float delta) 
	{
		
	
			
		
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
