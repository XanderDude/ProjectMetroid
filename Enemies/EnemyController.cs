using Godot;
using System;
using System.Collections.Generic;

public partial class EnemyController : CharacterBody3D
{
	[Export] public int Health { get; set; } = 100;
	[Export] public int ItemDropAmount { get; set; } = 0;
	[Export] public float MoveSpeed { get; set; } = 2.5f;
	[Export] public int Damage { get; set; } = 15;
	[Export] public float DamageCooldown { get; set; } = 0.5f;
	[Export] public Area3D DamageCollider { get; set; }
	[Export] public Node3D Mesh;

	public float timer = 0f;
	public PlayerManager player { get; set; }
	public EnemyStateMachine StateMachine { get; private set; }

	public Vector3 Direction { get; set; } = Vector3.Zero;
	public Vector3 TargetPosition { get; set; } = Vector3.Zero;

	private float _damageCooldownTimer;

	public bool jumpTrap = false;
	public override void _Ready()
	{
		jumpTrap = true;
		StateMachine = GetNode<EnemyStateMachine>("EnemyStateMachine");
		_damageCooldownTimer = DamageCooldown;

		Mesh ??= GetNodeOrNull<Node3D>("Mesh");
		DamageCollider ??= GetNodeOrNull<Area3D>("DamageCollider");
	}

	public override void _PhysicsProcess(double delta)
	{
		StateMachine?._currentState?.PhysicsUpdate((float)delta);

		if (_damageCooldownTimer > 0)
			_damageCooldownTimer -= (float)delta;

		if (_damageCooldownTimer <= 0 && player != null && player.canBeDamaged)
		{
			_damageCooldownTimer = DamageCooldown;
			player.Health -= Damage;
		}

		MoveAndSlide();
	}

	public override void _Process(double delta)
	{
		StateMachine?._currentState?.Update((float)delta);
	}

	public void OnCollide(Node3D node)
	{
		var player = node as PlayerManager;
		if (_damageCooldownTimer <= 0 && player.canBeDamaged)
		{
			_damageCooldownTimer = DamageCooldown;
			player.Health -= Damage;
		}
	}

	public void DamagedRecieved(int damage)
	{
		Health -= damage;
		Visible = false;
		var timer = GetTree().CreateTimer(0.1f);
		timer.Timeout += () => { Visible = true; };
		if (Health <= 0)
			KillEnemy();
	}

	public void KillEnemy()
	{ 
		DropItems();
		QueueFree();
	}


	public void DropItems()
	{
		Vector3 dropPosition = GlobalPosition;
		for (int i = 0; i < ItemDropAmount; i++)
		{
			var itemDropScene = GD.Load<PackedScene>("res://ItemDrop.tscn");
			var itemDropNode = itemDropScene.Instantiate();
			if (itemDropNode is ItemDrop itemDrop)
			{
				GetParent().AddChild(itemDrop);
				itemDrop.GlobalPosition = dropPosition;
			}
			else
			{
				GD.PrintErr("ItemDrop.tscn root node is not an ItemDrop!");
				itemDropNode.QueueFree();
			}
		}
	}
	
	public bool isPlayerInRange(float range)
	{
		if (player == null) return false;
		return GlobalPosition.DistanceTo(player.GlobalPosition) <= range;
	}

	public bool IsAtMeshEdge(float checkDistance = 0.5f)
	{
		// 1. Check directly below
		var spaceState = GetWorld3D().DirectSpaceState;
		Vector3 down = -GlobalTransform.Basis.Y.Normalized();
		Vector3 origin = GlobalPosition;
		Vector3 to = origin + down * checkDistance;

		var query = new PhysicsRayQueryParameters3D
		{
			From = origin,
			To = to,
			CollisionMask = CollisionMask
		};

		var result = spaceState.IntersectRay(query);

		// 2. Check slightly ahead and down (to catch edge while moving)
		Vector3 ahead = Direction.Normalized() * checkDistance * 0.5f;
		Vector3 aheadOrigin = origin + ahead;
		Vector3 aheadTo = aheadOrigin + down * checkDistance;

		var aheadQuery = new PhysicsRayQueryParameters3D
		{
			From = aheadOrigin,
			To = aheadTo,
			CollisionMask = CollisionMask
		};

		var aheadResult = spaceState.IntersectRay(aheadQuery);

		// 3. Check if current Y is much higher than previous frame (falling off)
		// You may want to store previous Y in a field, e.g. prevY
		bool falling = false;
		if (prevY.HasValue && GlobalPosition.Y < prevY.Value - 0.2f)
			falling = true;
		prevY = GlobalPosition.Y;

		// Return true if no ground below, no ground ahead, or falling
		return result.Count == 0 || aheadResult.Count == 0 || falling;
	}

	// Add this field to your class:
	private float? prevY = null;
}
