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


	public RayCast3D Beam;

	public float timer = 0f;

	public PlayerManager player { get; set; }
	public EnemyStateMachine StateMachine { get; private set; }

	public Vector3 Direction { get; set; } = Vector3.Zero;
	public Vector3 TargetPosition { get; set; } = Vector3.Zero;

	private float _damageCooldownTimer;

	public bool jumpTrap = false;

	public bool isLeft = false;
	public override void _Ready()
	{
		jumpTrap = true;
		StateMachine = GetNode<EnemyStateMachine>("EnemyStateMachine");
		_damageCooldownTimer = DamageCooldown;

		Mesh ??= GetNodeOrNull<Node3D>("Mesh");
		DamageCollider ??= GetNodeOrNull<Area3D>("DamageCollider");
		Beam ??= GetNodeOrNull<RayCast3D>("Beam");
		player = GetNode<PlayerManager>("../Player");
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
		if (GlobalPosition.DistanceTo(player.GlobalPosition) <= range && player.GlobalPosition.Y - GlobalPosition.Y < 0.2f)
			return true;
		else
			return false;

	}

	public bool isAtMeshEdge()
	{
		if (Beam == null)
		{
			return false;
		}

		if (Beam.IsColliding())
		{
			return false;
		}
		else
		{
			return true;
		}
	}


	public int GetRandomSign()
	{
    	Random rand = new Random();
    	return rand.Next(0, 2) == 0 ? -1 : 1;
	}

}
