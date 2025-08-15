using Godot;
using System;

public partial class attackState : State
{
	[Export] public PackedScene arrowScene;
	[Export] public float arrowSpeed = 20.0f;
	[Export] public float shootCooldown = 0.1f;
	[Export] public AudioStream shootSound; 
	[Export] Godot.AudioStreamPlayer laserSound;

	private float cooldownTimer = 0.1f;
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
		cooldownTimer = 0.1f;
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
	
	private Vector3 GetShootDirection()
{
	Vector3 direction = Vector3.Zero;
	
	
	bool up = Input.IsActionPressed("Up");
	bool down = Input.IsActionPressed("Down");
	bool left = Input.IsActionPressed("Left");
	bool right = Input.IsActionPressed("Right");
	

	GD.Print($"Input states - Up: {up}, Down: {down}, Left: {left}, Right: {right}");
	
	
	if (up && left)
		direction = new Vector3(-1, 1, 0).Normalized();  // Up-Left
	else if (up && right)
		direction = new Vector3(1, 1, 0).Normalized();   // Up-Right
	else if (down && left)
		direction = new Vector3(-1, -1, 0).Normalized(); // Down-Left
	else if (down && right)
		direction = new Vector3(1, -1, 0).Normalized();  // Down-Right
	else if (up)
		direction = new Vector3(0, 1, 0);                // Up
	else if (down)
		direction = new Vector3(0, -1, 0);               // Down
	else if (left)
		direction = new Vector3(-1, 0, 0);               // Left
	else if (right)
		direction = new Vector3(1, 0, 0);                // Right
	else
		direction = new Vector3(1, 0, 0);                // Default: Right
	
	GD.Print($"Final direction: {direction}");
	return direction;
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
	

	Vector3 shootDirection = GetShootDirection();
	arrow.LinearVelocity = shootDirection * arrowSpeed;
	
	if (arrow.LinearVelocity != Vector3.Zero)
	{
		arrow.LookAt(arrow.GlobalPosition + arrow.LinearVelocity, Vector3.Up);
		arrow.RotateObjectLocal(Vector3.Up, Mathf.Pi / 2);
	}
	
	arrow.Scale *= 3f;
	arrow.GravityScale = 0.3f;
	
	laserSound = GetNode<Godot.AudioStreamPlayer>("../../shootingsound");
	laserSound.Play();
	
	GD.Print($"Shot arrow at {arrow.GlobalPosition} with velocity: {arrow.LinearVelocity}");
}
}
