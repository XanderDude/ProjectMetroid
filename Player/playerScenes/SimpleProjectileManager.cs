using Godot;
using System;
using System.Collections.Generic;

public partial class SimpleProjectileManager : Node
{
	[Export] public int maxProjectiles = 5;
	[Export] public float defaultMaxDistance = 50.0f;
	[Export] public float defaultGravity = 30.0f;
	[Export] public PackedScene arrowScene;
	Vector3 offset;
	private List<ProjectileInfo> activeProjectiles = new List<ProjectileInfo>();
	CharacterBody3D playerNode;
	Node3D playerMesh;
	Skeleton3D skeleton;
	Node3D crossbow;
	
	public class ProjectileInfo
	{
		public CharacterBody3D projectile;
		public Vector3 startPosition;
		public float speed;
		public float maxDistance;
		public bool facingRight;
		public float lifetime = 0f;
		public Vector3 customVelocity = Vector3.Zero;
		public bool useGravity = false;
	}
	
	public override void _Ready() {
		playerNode = GetNode<CharacterBody3D>("../../../");
		playerMesh = playerNode?.GetNode<Node3D>("PlayerMesh");
		skeleton = playerMesh?.GetNode<Skeleton3D>("Skeleton3D");
		crossbow = skeleton?.GetNode<Node3D>("Crossbow");
	}
	// Standard projectile - spawns from crossbow position
	public bool CreateProjectile(CharacterBody3D shooter, float speed, bool facingRight, float maxDistance = 0)
	{
		//offset = facingRight ? new Vector3(2,0,0) : new Vector3(-2,0,0);
		Vector3 spawnPos = CalculateSpawnPosition();
		return CreateProjectileWithVelocity(spawnPos, speed, facingRight, maxDistance, Vector3.Zero, false);
	}
	
	private Vector3 CalculateSpawnPosition()
	{
		
		var playerNode = GetNode<CharacterBody3D>("../../../");
		var playerMesh = playerNode.GetNode<Node3D>("PlayerMesh");
		var skeleton = playerMesh.GetNode<Skeleton3D>("Skeleton3D");
		var crossbow = skeleton.GetNode<Node3D>("Crossbow");
		
		
		// Use crossbow position directly - animation handles everything
		return crossbow.GlobalPosition;
	}
	
	private bool CreateProjectileWithVelocity(Vector3 position, float speed, bool facingRight, float maxDistance, Vector3 customVelocity, bool useGravity, Vector3 rotation = default)
	{
		// Remove oldest projectile if at limit
		if (activeProjectiles.Count >= maxProjectiles)
		{
			RemoveProjectile(0);
		}
		
		if (arrowScene == null)
		{
			GD.PrintErr("No arrow scene assigned to ProjectileManager!");
			return false;
		}
		
		// Create and setup new arrow
		CharacterBody3D newArrow = arrowScene.Instantiate<CharacterBody3D>();
		GetTree().CurrentScene.AddChild(newArrow);
		
		newArrow.GlobalPosition = position;
		newArrow.Visible = true;
		
		
		
		// Get crossbow rotation for natural arrow orientation
		
		
		if (crossbow != null)
		{
			// Use crossbow's rotation
			newArrow.GlobalRotationDegrees = new Vector3(crossbow.GlobalRotationDegrees.X,crossbow.GlobalRotationDegrees.Y, -crossbow.GlobalRotationDegrees.Z);
		}
		
		// Start with collision disabled to avoid hitting the player
		newArrow.SetCollisionLayerValue(1, false);
		newArrow.SetCollisionMaskValue(1, false);
		
		// Store projectile info
		ProjectileInfo info = new ProjectileInfo
		{
			projectile = newArrow,
			startPosition = position,
			speed = speed > 0 ? speed : defaultMaxDistance,
			maxDistance = maxDistance > 0 ? maxDistance : defaultMaxDistance,
			facingRight = facingRight,
			customVelocity = customVelocity,
			useGravity = useGravity
		};
		
		activeProjectiles.Add(info);
		return true;
	}
	
	public override void _PhysicsProcess(double delta)
	{
		float deltaF = (float)delta;
		
		// Process all projectiles (backwards for safe removal)
		for (int i = activeProjectiles.Count - 1; i >= 0; i--)
		{
			ProjectileInfo info = activeProjectiles[i];
			
			// Clean up invalid projectiles
			if (!IsInstanceValid(info.projectile))
			{
				activeProjectiles.RemoveAt(i);
				continue;
			}
			
			info.lifetime += deltaF;
			
			// Enable collision after brief delay
			if (info.lifetime > 0.1f && !info.projectile.GetCollisionMaskValue(1))
			{
				info.projectile.SetCollisionLayerValue(1, true);
				info.projectile.SetCollisionMaskValue(1, true);
			}
			
			bool shouldRemove = false;
			
			// Update movement and check collisions
			if (info.lifetime > 0.1f)
			{
				Vector3 velocity = CalculateVelocity(info, deltaF);
				info.projectile.Velocity = velocity;
				info.projectile.MoveAndSlide();
				
				// Check for collision
				if (info.projectile.GetSlideCollisionCount() > 0)
				{
					shouldRemove = ShouldRemoveOnCollision(info);
				}
			}
			
			// Check distance limit
			float distance = info.startPosition.DistanceTo(info.projectile.GlobalPosition);
			if (distance >= info.maxDistance)
			{
				shouldRemove = true;
			}
			
			if (shouldRemove)
			{
				RemoveProjectile(i);
			}
		}
	}
	
	private Vector3 CalculateVelocity(ProjectileInfo info, float deltaF)
	{

			float horizontalSpeed = info.facingRight ? Mathf.Abs(info.speed) : -Mathf.Abs(info.speed);
			return new Vector3(horizontalSpeed, 0, 0);
	}
	
	private bool ShouldRemoveOnCollision(ProjectileInfo info)
	{
		var collision = info.projectile.GetSlideCollision(0);
		Vector3 normal = collision.GetNormal();
		bool isGroundHit = normal.Y > 0.7f; // Ground has upward normal
		
		// For horizontal arrows, ignore ground hits
		if (info.customVelocity == Vector3.Zero && isGroundHit)
		{
			return false; // Don't remove on ground hit for horizontal arrows
		}
		
		return true; // Remove on any other collision
	}
	
	private void RemoveProjectile(int index)
	{
		if (index >= 0 && index < activeProjectiles.Count)
		{
			if (IsInstanceValid(activeProjectiles[index].projectile))
			{
				activeProjectiles[index].projectile.QueueFree();
			}
			activeProjectiles.RemoveAt(index);
		}
	}
	
	public void ClearAllProjectiles()
	{
		for (int i = activeProjectiles.Count - 1; i >= 0; i--)
		{
			RemoveProjectile(i);
		}
	}
	
	public int GetActiveProjectileCount()
	{
		return activeProjectiles.Count;
	}
}
