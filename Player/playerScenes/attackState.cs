using Godot;
using System;

public partial class attackState : State
{
	[Export] public PackedScene arrowScene;
	[Export] public float arrowSpeed = 20.0f;
	[Export] public float shootCooldown = 0.1f;
	
	private float cooldownTimer = 0.0f;
	private Node3D crossbowMesh;
	private PlayerManager playerManager;
	
	public override void _Ready() 
	{
		Node current = this;
		while (current != null && !(current is PlayerManager))
		{
			current = current.GetParent();
		}
		
		if (current is PlayerManager)
		{
			playerManager = current as PlayerManager;
			GD.Print($"PlayerManager found: {playerManager.Name}");
		}
		else
		{
			GD.PrintErr("Could not find PlayerManager in parent hierarchy!");
		}
		
		if (playerManager != null)
		{
			crossbowMesh = playerManager.GetNode<Node3D>("PlayerMesh/Skeleton3D/Crossbow");
			GD.Print($"CrossbowMesh found: {crossbowMesh != null}");
		}
		
		GD.Print($"ArrowScene assigned: {arrowScene != null}");
	}
	
	public override void Enter() 
	{
		cooldownTimer = 0.0f;
		GD.Print("Entered ranged attack state");
	}
	
	public override void Exit()
	{
		GD.Print("Exited ranged attack state");
	}
	
	public override void PhysicsUpdate(float delta) 
	{
		if (cooldownTimer > 0.0f)
		{
			cooldownTimer -= delta;
		}
		
		if (Input.IsActionPressed("Shoot") && CanShoot())
		{
			ShootArrow();
			cooldownTimer = shootCooldown;
		}
	}
	
	public override void HandleInput(InputEvent @event) 
	{
		if (@event.IsActionReleased("Shoot")) 
		{
			asm.TransitionTo("noattackState");
		}
	}
	
	private bool CanShoot()
	{
		return playerManager != null && 
			   playerManager.HasRangedWeapon() && 
			   cooldownTimer <= 0.0f && 
			   arrowScene != null;
	}
	
	private void ShootArrow()
	{
		if (arrowScene == null || crossbowMesh == null)
		{
			GD.PrintErr("Arrow scene or crossbow mesh not found!");
			return;
		}

		var arrow = arrowScene.Instantiate() as RigidBody3D;
		if (arrow == null)
		{
			GD.PrintErr("Arrow scene must be a RigidBody3D!");
			return;
		}

		GetTree().CurrentScene.AddChild(arrow);
		arrow.GlobalPosition = crossbowMesh.GlobalPosition;

		Vector3 shootDirection = crossbowMesh.GlobalTransform.Basis.Z.Normalized();
		arrow.LinearVelocity = shootDirection * arrowSpeed;

		if (arrow.LinearVelocity != Vector3.Zero)
		{
			arrow.LookAt(arrow.GlobalPosition + arrow.LinearVelocity, Vector3.Up);
			arrow.RotateObjectLocal(Vector3.Up, Mathf.Pi / 2);
		}

		arrow.Scale *= 3f;
		arrow.GravityScale = 0.3f;

		GD.Print($"Shot arrow at {arrow.GlobalPosition} with velocity: {arrow.LinearVelocity}");
	}
}
