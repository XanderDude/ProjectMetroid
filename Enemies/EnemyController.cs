using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Threading.Tasks;

public partial class EnemyController : CharacterBody3D
{
	[ExportGroup("Enemy Stats")]
	[Export] public int health = 100;
	[Export] public int itemdropamount = 0;
	[Export] public float runspeed { get; set; } = 2.5f;
	[Export] public float walkspeed { get; set; } = 1.0f;
	[Export] public float idle { get; set; } = 0f;
	[Export] public float acceleration { get; set; } = 0.1f;
	[Export] public float jumpmaxheight { get; set; } = 10.0f;
	[Export] public int  damagedealt { get; set; } = 15;
	[Export] public float damagecooldown { get; set; } = 0.5f;
	[Export] public float gravity { get; set; } = -9.8f;
	[Export] public float attackspeed { get; set; } = 1.0f;
	[Export] public float attackknockback { get; set; } = 5;
	[Export] public float damageovertime { get; set; } = 0f;
	[Export] public float detectionrange { get; set; } = 10f;
	[Export] public bool isFlying = false;

	[ExportGroup("Node References")]
	[Export] public Node3D mesh;
	public PlayerManager player => GameFriend.gameinstance.player;
	[Export] public RayCast3D ray;

	[Export] public RayCast3D edgeray;
	[Export] public EnemyStateMachine statemachine { get; private set; }
	[Export] public PackedScene projectilescene = null;
	public float speed = 0.0f;
	public Godot.Vector3 direction { get; set; } = Godot.Vector3.Zero;
	public Godot.Vector3 targetposition { get; set; } = Godot.Vector3.Zero;

	public override void _Ready()
	{
		if (player == null) GD.Print("Player not assigned");
    }

	public override void _PhysicsProcess(double delta)
	{
		statemachine?._currentState?.PhysicsUpdate((float)delta);
		MoveAndSlide();
	}

	public override void _Process(double delta)
	{
		statemachine?._currentState?.Update((float)delta);
	}

	public void OnCollide(Node3D node)
	{
		player.Health -= damagedealt;
	}

	public void DamagedRecieved(int damage)
	{
		health -= damage;
		DamageFlicker();
		if (health <= 0)
		{
			KillEnemy();
		}
	}

	public void KillEnemy()
	{
		DropItems();
		QueueFree();
	}

	private async void DamageFlicker()
    {
        mesh.Visible = false;
		await ToSignal(GetTree().CreateTimer(.1f, false, false, false), "timeout");
		mesh.Visible = true;
		
    }

	public void DropItems()
	{
		Godot.Vector3 dropPosition = GlobalPosition;
		for (int i = 0; i < itemdropamount; i++)
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
		float distance = GlobalPosition.DistanceTo(player.GlobalPosition);
		float yDifference = Mathf.Abs(player.GlobalPosition.Y - GlobalPosition.Y);
		
		
		if (distance <= range && yDifference < 3f) 
		{
			//GD.Print("Player IN RANGE!");
			return true;
		}
		else
		{
			return false;
		}
	}




	public int GetRandomSign()
	{
		Random rand = new Random();
		return rand.Next(0, 2) == 0 ? -1 : 1;
	}

	public int GetRandom1234()
	{
		Random rnd = new Random();
	
		return rnd.Next(1, 5); 
	}


	public float GetRandomNumber()
	{
	return GD.RandRange(5, 9);
	}


	public void initBounds()
	{

		
	}
	
	public bool CurrentDirection()
	{
		if (this.Velocity.X >= 0) return true;
		else return false;
	}
	
	}
