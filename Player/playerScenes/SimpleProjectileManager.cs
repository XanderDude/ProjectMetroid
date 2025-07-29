using Godot;
using System;
using System.Collections.Generic;

public partial class SimpleProjectileManager : Node
{
	[Export] public int maxProjectiles = 5;
	[Export] public float defaultMaxDistance = 50.0f;
	[Export] public PackedScene arrowScene;
	
	private List<ProjectileInfo> activeProjectiles = new List<ProjectileInfo>();
	
	public class ProjectileInfo
	{
		public CharacterBody3D projectile;
		public Vector3 startPosition;
		public float speed;
		public float maxDistance;
		public bool facingRight;
		public float lifetime = 0f;
		public Vector3 customVelocity = Vector3.Zero; // for angled shots
		public bool useGravity = false; // make arrow drop down
	}
	
	// normal arrow shoots straight
	public bool CreateProjectile(CharacterBody3D shooter, float speed, bool facingRight, float maxDistance = 0)
	{
		Vector3 spawnPos = CalculateSpawnPosition(shooter.GlobalPosition, facingRight, false, false, false);
		return CreateProjectileWithVelocity(spawnPos, speed, facingRight, maxDistance, Vector3.Zero, false, Vector3.Zero);
	}
	
	// arrow goes down like rain
	public bool CreateDownwardProjectile(CharacterBody3D shooter, float speed, bool facingRight, float maxDistance = 0)
	{
		Vector3 spawnPos = CalculateSpawnPosition(shooter.GlobalPosition, facingRight, false, false, true);
		
		Vector3 velocity = new Vector3(0, -speed, 0); // straight down
		Vector3 rotation = new Vector3(0, 0, -90); // point down
		
		return CreateProjectileWithVelocity(spawnPos, speed, facingRight, maxDistance, velocity, true, rotation);
	}
	
	// angled shot for skilled players
	public bool CreateAngledProjectile(CharacterBody3D shooter, float speed, bool facingRight, float angle, float maxDistance = 0)
	{
		Vector3 spawnPos = CalculateSpawnPosition(shooter.GlobalPosition, facingRight, true, false, false);
		
		float radians = Mathf.DegToRad(angle);
		Vector3 velocity;
		Vector3 rotation;
		if (facingRight)
		{
			velocity = new Vector3(speed * Mathf.Cos(radians), speed * Mathf.Sin(radians), 0);
			rotation = new Vector3(0, 0, angle);
		}
		else
		{
			velocity = new Vector3(-speed * Mathf.Cos(radians), speed * Mathf.Sin(radians), 0);
			rotation = new Vector3(0, 180, angle);
		}
		
		return CreateProjectileWithVelocity(spawnPos, speed, facingRight, maxDistance, velocity, true, rotation);
	}
	
	// shoot arrow up to sky
	public bool CreateUpwardProjectile(CharacterBody3D shooter, float speed, bool facingRight, float maxDistance = 0)
	{
		Vector3 spawnPos = CalculateSpawnPosition(shooter.GlobalPosition, facingRight, false, true, false);
		
		Vector3 velocity = new Vector3(0, speed, 0); // straight up
		Vector3 rotation = new Vector3(0, 0, 90); // point up
		
		return CreateProjectileWithVelocity(spawnPos, speed, facingRight, maxDistance, velocity, true, rotation);
	}
	
	// find where arrow should spawn near player
	private Vector3 CalculateSpawnPosition(Vector3 playerPosition, bool facingRight, bool isAngled, bool isUpward = false, bool isDownward = false)
	{
		Vector3 position = playerPosition;
		
		if (isUpward)
		{
			position += Vector3.Up * 1.5f; // higher for up shots
			if (facingRight)
				position += Vector3.Right * 2.0f; // offset right
			else
				position += Vector3.Right * 0.5f; // offset left same distance
		}
		else if (isDownward)
		{
			position += Vector3.Down * 0.5f; // below player for down shots
			if (facingRight)
				position += Vector3.Left * 1.0f; // this looks wrong
			else
				position += Vector3.Left * 2.0f; // both go left because bug i think
		}
		else if (isAngled)
		{
			position += Vector3.Up * 1.0f; // higher for angled shots
			if (facingRight)
				position += Vector3.Right * 2.0f; // spread out more
			else
				position += Vector3.Left * 2.0f; // same but left
		}
		else
		{
			position += Vector3.Down * 0.1f; // normal shots slightly down
			if (facingRight)
				position += Vector3.Right * 1.5f;
			else
				position += Vector3.Left * 1.5f;
		}
		
		return position;
	}
	
	// main function that creates arrow in world
	private bool CreateProjectileWithVelocity(Vector3 position, float speed, bool facingRight, float maxDistance, Vector3 customVelocity, bool useGravity, Vector3 rotation = default)
	{
		// too many arrows? remove oldest one
		if (activeProjectiles.Count >= maxProjectiles)
		{
			RemoveProjectile(0);
		}
		
		// need arrow scene to make arrows
		if (arrowScene == null)
		{
			GD.PrintErr("No arrow scene assigned to ProjectileManager!");
			return false;
		}
		
		// create new arrow from scene
		ArrowProjectile newArrow = arrowScene.Instantiate<ArrowProjectile>();
		GetTree().CurrentScene.AddChild(newArrow);
		
		newArrow.GlobalPosition = position;
		newArrow.Visible = true;
		newArrow.Scale = new Vector3(3.0f, 3.0f, 3.0f); 
		
		float finalSpeed = speed > 0 ? speed : newArrow.speed;
  		float finalMaxDistance = maxDistance > 0 ? maxDistance : newArrow.maxDistance;
   		bool finalUseGravity = useGravity || newArrow.useGravity;
		
		// rotate arrow if needed
		if (rotation != Vector3.Zero)
		{
			newArrow.RotationDegrees = rotation;
		}
		
		// disable collision at start so no hit player
		newArrow.SetCollisionLayerValue(1, false);
		newArrow.SetCollisionMaskValue(1, false);
		
		// save arrow info for tracking
		ProjectileInfo info = new ProjectileInfo
		{
		projectile = newArrow,
		startPosition = position,
		speed = finalSpeed, // use final calculated speed
		maxDistance = finalMaxDistance, // use final calculated distance
		facingRight = facingRight,
		customVelocity = customVelocity,
		useGravity = finalUseGravity // use final calculated gravity
		};
		
		activeProjectiles.Add(info);
		return true;
	}
	
	// update all arrows every frame
	public override void _PhysicsProcess(double delta)
	{
		float deltaF = (float)delta;
		
		// loop backwards so we can remove safely
		for (int i = activeProjectiles.Count - 1; i >= 0; i--)
		{
			ProjectileInfo info = activeProjectiles[i];
			
			// arrow got deleted? remove from list
			if (!IsInstanceValid(info.projectile))
			{
				activeProjectiles.RemoveAt(i);
				continue;
			}
			
			info.lifetime += deltaF;
			
			// enable collision after half second
			if (info.lifetime > 0.5f && !info.projectile.GetCollisionMaskValue(1))
			{
				info.projectile.SetCollisionLayerValue(1, true);
				info.projectile.SetCollisionMaskValue(1, true);
			}
			
			Vector3 velocity = Vector3.Zero;
			if (info.customVelocity != Vector3.Zero)
			{
				// use custom movement for special arrows
				velocity = info.customVelocity;
				if (info.useGravity)
				{
					velocity.Y -= 9.8f * info.lifetime; // gravity pulls down
				}
			}
			else
			{
				// normal left/right movement
				if (info.facingRight)
				{
					velocity.X = Mathf.Abs(info.speed);
				}
				else
				{
					velocity.X = -Mathf.Abs(info.speed);
				}
			}
			
			// move the arrow
			info.projectile.Velocity = velocity;
			info.projectile.MoveAndSlide();
			
			float distance = info.startPosition.DistanceTo(info.projectile.GlobalPosition);
			
			bool hitWall = false;
			// check wall collision after arrow becomes solid
			if (info.lifetime > 0.5f && info.projectile.GetSlideCollisionCount() > 0)
			{
				for (int c = 0; c < info.projectile.GetSlideCollisionCount(); c++)
				{
					KinematicCollision3D collision = info.projectile.GetSlideCollision(c);
					Node collider = (Node)collision.GetCollider();
					
					// only care about solid things with visible mesh
					if (collider is StaticBody3D || collider is RigidBody3D)
					{
						if (HasVisibleMesh(collider))
						{
							hitWall = true;
							break;
						}
					}
				}
			}
			
			// remove if too far or hit wall
			if (distance >= info.maxDistance || hitWall)
			{
				RemoveProjectile(i);
			}
		}
	}
	
	// check if object has visible mesh
	private bool HasVisibleMesh(Node node)
	{
		if (node is MeshInstance3D meshInstance)
		{
			return meshInstance.Visible && meshInstance.Mesh != null;
		}
		
		// check children for mesh
		foreach (Node child in node.GetChildren())
		{
			if (child is MeshInstance3D childMesh && childMesh.Visible && childMesh.Mesh != null)
			{
				return true;
			}
		}
		
		return false;
	}
	
	// remove arrow from game
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
	
	// delete all arrows at once
	public void ClearAllProjectiles()
	{
		for (int i = activeProjectiles.Count - 1; i >= 0; i--)
		{
			RemoveProjectile(i);
		}
	}
	
	// count how many arrows are active
	public int GetActiveProjectileCount()
	{
		return activeProjectiles.Count;
	}

}
