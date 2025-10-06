using Godot;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;

public partial class RavenAttackState : State
{
    [Export] private int damage = 15;
    private Area3D slashArea;
	private Area3D slashHitboxes;
	private List<Node3D> targetsDamaged = new();
	private MeshInstance3D slashMesh;
	private CollisionShape3D slashCollision;
	[Export] private PackedScene slashVFX;
	private Node3D newSlash;
	[Export] private float attackSpeed = 0.3f;
	private float attackTimer = 0.0f;

	private Vector3 slashOffset = Vector3.Zero;

	private Vector2 attackDirection = Vector2.Zero;
	public override void Ready()
	{
		//if (ravenSlash != null) ravenSlash = ResourceLoader.Load((stringravenSlash.ResourcePath)
	}

	public override void Enter()
	{
		parentMesh = raven.player.GetNode<Node3D>(raven.player.playerMeshPath);
		attackDirection = GetAttackDirection();
		raven.Visible = false;
		SpawnSlashVFX();
		//CreateSlashMesh();
		attackTimer = 0.0f;
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
		targetsDamaged.Clear();
	}

	public override void Update(float delta)
	{
		attackTimer += delta;
		if (slashArea != null && IsInstanceValid(slashArea))
		{
			var playerPos = raven.player.GlobalPosition;
			slashOffset = new Vector3(attackDirection.X * 1.0f, attackDirection.Y + 1.0f, 0.0f);
			slashArea.GlobalPosition = playerPos + slashOffset;
		}


		if (attackTimer >= attackSpeed)
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

	private void SpawnSlashVFX()
	{
		if (newSlash == null)
		{
			newSlash = slashVFX.Instantiate() as Node3D; //instantiate loaded vfx
			raven.player.AddChild(newSlash); //add vfx to player
			slashHitboxes = newSlash.GetNode<Area3D>("Area3D");
			slashHitboxes.BodyEntered += OnBodyEntered;
		}

		newSlash.Position = new Vector3(0, 1f, 0);

		float dot = attackDirection.Dot(new Vector2(Mathf.Abs(attackDirection.X), 0));
		float rotation = (attackDirection.Y >= 0)
		? (1f - dot) / 4f
		: (dot + 3f) / 4f;

		rotation *= 360f;

		newSlash.RotationDegrees = new(0, 0, rotation);
		newSlash.GetNode<AnimationPlayer>("AnimationPlayer").Play("RESET");
		newSlash.GetNode<AnimationPlayer>("AnimationPlayer").Play("RavenAttack1");

	}
	private void CreateSlashMesh()
	{
		slashArea = new Area3D();
		slashArea.CollisionLayer = 0;
		slashArea.CollisionMask = 0xFFFFFFFF;
		raven.GetParent().AddChild(slashArea);

		/*slashMesh = new MeshInstance3D();
		var slashShape = new BoxMesh();
		slashShape.Size = new Vector3(2.0f, 1.5f, 0.2f);
		slashMesh.Mesh = slashShape;
		slashArea.AddChild(slashMesh);*/

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
		//GD.Print($"Body entered slash area: {body.Name} - Type: {body.GetType().Name}");

		if (targetsDamaged.Contains(body)) return;

		if (body is EnemyController enemy)
			{
				//GD.Print("Dealing damage to enemy!");
				enemy.DamagedRecieved(damage);
				targetsDamaged.Add(body);
			}
	}
	
	private void OnAreaEntered(Area3D area)
	{
		
		//GD.Print($"Area entered slash area: {area.Name} - Type: {area.GetType().Name}");
		
		Node3D parent = area.GetParent<Node3D>();
		if (parent != null && parent is EnemyController enemy)
		{
			//GD.Print("Dealing damage to enemy via area!");
			enemy.DamagedRecieved(damage);
		}
	}
}
