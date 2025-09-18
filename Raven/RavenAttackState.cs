using Godot;
using System;

public partial class RavenAttackState : State
{
	private Area3D slashArea;
	private MeshInstance3D slashMesh;
	private CollisionShape3D slashCollision;
	private float attackDuration = 0.3f;
	private float attackTimer = 0.0f;
	private bool hasDealtDamage = false;

	private Vector3 playerPos = Vector3.Zero;
	private Vector3 slashOffset = Vector3.Zero;
	
	private Vector2 attackDirection = Vector2.Zero;
	public override void Enter()
	{
		parentMesh = raven.player.GetNode<Node3D>("PlayerMesh");
		attackDirection = GetAttackDirection();
		raven.Visible = false;
		CreateSlashMesh();
		attackTimer = 0.0f;
		hasDealtDamage = false;
		raven.isInAction = true;
	}
	
	public override void Exit()
	{
		raven.Visible = true;
		if (slashArea != null && IsInstanceValid(slashArea))
		{
			slashArea.QueueFree();
		}
		raven.isInAction = false;
	}

	public override void Update(float delta)
	{
		attackTimer += delta;
		if (slashArea != null && IsInstanceValid(slashArea))
		{
			playerPos = raven.player.GlobalPosition;
			slashOffset = new Vector3(attackDirection.X * 1.0f, attackDirection.Y + 1.0f, 0.0f);
			slashArea.GlobalPosition = playerPos + slashOffset;
		}
	

		if (attackTimer >= attackDuration)
		{
			rsm.TransitionTo("RavenOnPlayerState");
		}



	}
	
	private Vector2 GetAttackDirection()
	{


		
	
		Vector2 direction = new Vector2(Input.GetAxis("Left", "Right"), Input.GetAxis("Down", "Up"));

		if (direction == Vector2.Zero)
		{
			if (Input.IsActionPressed("Up") || Input.IsActionPressed("Down"))
			{
				direction = new Vector2(0, Input.GetAxis("Down", "Up"));
			}
			else
			{
				direction = new Vector2(Math.Sign(parentMesh.RotationDegrees.Y), 0);
			}
			
			direction = new(Math.Sign(parentMesh.RotationDegrees.Y), direction.Y);
		}
		
		return direction.Normalized();
	}
	
	public override void PhysicsUpdate(float delta)
	{
	}
	
	public override void HandleInput(InputEvent @event)
	{
	}
	
	private void CreateSlashMesh()
	{
		slashArea = new Area3D();
		slashArea.CollisionLayer = 0;
		slashArea.CollisionMask = 0xFFFFFFFF;
		raven.GetParent().AddChild(slashArea);
		
		slashMesh = new MeshInstance3D();
		var slashShape = new BoxMesh();
		slashShape.Size = new Vector3(2.0f, 1.5f, 0.2f);
		slashMesh.Mesh = slashShape;
		slashArea.AddChild(slashMesh);
		
		slashCollision = new CollisionShape3D();
		var boxShape = new BoxShape3D();
		boxShape.Size = new Vector3(2.0f, 1.5f, 0.2f);
		slashCollision.Shape = boxShape;
		slashArea.AddChild(slashCollision);
		
		Vector3 playerPos = raven.player.GlobalPosition;
		Vector3 slashOffset = new Vector3(1.0f, 0.5f, 0);
		
		if (Input.GetAxis("Left", "Right") == -1)
		{
			slashOffset.X = -1.0f;
		}
		
		slashArea.GlobalPosition = playerPos + slashOffset;
		
		slashArea.BodyEntered += OnBodyEntered;
	}
	
	private void OnBodyEntered(Node3D body)
	{
		if (hasDealtDamage) return;
		
		GD.Print($"Body entered slash area: {body.Name} - Type: {body.GetType().Name}");
		
		if (body is EnemyController enemy)
		{
			GD.Print("Dealing damage to enemy!");
			enemy.DamagedRecieved(15);
			hasDealtDamage = true;
		}
	}
	
	private void OnAreaEntered(Area3D area)
	{
		if (hasDealtDamage) return;
		
		GD.Print($"Area entered slash area: {area.Name} - Type: {area.GetType().Name}");
		
		Node3D parent = area.GetParent<Node3D>();
		if (parent != null && parent is EnemyController enemy)
		{
			GD.Print("Dealing damage to enemy via area!");
			enemy.DamagedRecieved(15);
			hasDealtDamage = true;
		}
	}
}
