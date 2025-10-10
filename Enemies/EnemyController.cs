using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;

public partial class EnemyController : CharacterBody3D
{
	[ExportGroup("Enemy Stats")]
	[Export] public int health = 100;
	[Export] public int itemdropamount = 0;
	[Export] public float runspeed { get; set; } = 2.5f;

	[Export] public float walkspeed { get; set; } = 1.0f;

	public float idle { get; set; } = 0.00000000000001f;
	
	[Export] public float acceleration { get; set; } = 0.1f;
	[Export] public float jumpheight { get; set; } = 5.0f;
	[Export] public int  damagedealt { get; set; } = 15;
	[Export] public float damagecooldown { get; set; } = 0.5f;

	[Export] public float gravity { get; set; } = -9.8f;
	[Export] public float attackspeed { get; set; } = 1.0f;
	[Export] public float attackknockback { get; set; } = 5;
	[Export] public float damageovertime { get; set; } = 0f; 


	[ExportGroup("Node References")]
	[Export] public Area3D damagecollider { get; set; }
	[Export] public Node3D mesh;
	
	public PlayerManager player;
	[Export] public RayCast3D pathfindingray;
	[Export] public EnemyStateMachine statemachine { get; private set; }
	[Export] public MeshInstance3D passivepatroldistance { get; set; } //The area that the enemy walks passively (no aggro)
	[Export] public MeshInstance3D activepatroldistance { get; set; } //The area that the enemy can detect the player in
	[Export] public Area3D attackrange { get; set; } // The area that the enemy can enter their attack state in
	[Export] public PackedScene projectilescene = null;

	public float speed = 0.0001f;

	public float originalMeshY;

	[ExportGroup("Enemy Type")]
	
	[Export] public EnemyType Type { get; set; } = EnemyType.Grounded;
	public enum EnemyType
	{
		Flying,
		Grounded
	}

	[Export] public EnemyAttackType AttackType { get; set; } = EnemyAttackType.Melee;
	public enum EnemyAttackType
	{
		Melee,
		Ranged
	
	}



	public float timer = 0f;
	public float timer2 = 0f;



	

	public Godot.Vector3 currentPos;



	public Godot.Vector3 direction { get; set; } = Godot.Vector3.Zero;
	public Godot.Vector3 targetposition { get; set; } = Godot.Vector3.Zero;

	

	private float _damageCooldownTimer;

	public string name;


	public override async void _Ready()
	{
		
		
		player = GetNode<PlayerManager>("%Player");
		if (!ValidateExports()) GD.PrintErr("EnemyController: missing required exports, check inspector."); //validate exports
		SetGravity();

		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		passivepatroldistance.Reparent(GetTree().Root, keepGlobalTransform: true);
    	GD.Print("Passive Patrol Distance New Parent = " + passivepatroldistance.GetParent().Name);

		_damageCooldownTimer = damagecooldown;

		
		
		
		
	}

	public override void _PhysicsProcess(double delta)
	{
		originalMeshY = this.RotationDegrees.Y;

		currentPos = GlobalPosition;
		if (CurrentDirection())
		{
			this.RotationDegrees = new Godot.Vector3(0, 0, 0);
		}
		else
		{
			this.RotationDegrees = new Godot.Vector3(0, 180, 0);
        }

			statemachine?._currentState?.PhysicsUpdate((float)delta);

		if (_damageCooldownTimer > 0)
			_damageCooldownTimer -= (float)delta;

		if (_damageCooldownTimer <= 0 && player != null && player.canBeDamaged)
		{
			_damageCooldownTimer = damagecooldown;
			player.Health -= damagedealt;
		}



		
		MoveAndSlide();
	}

	public override void _Process(double delta)
	{
		statemachine?._currentState?.Update((float)delta);
	}

	public void OnCollide(Node3D node)
	{
		var player = node as PlayerManager;
		if (_damageCooldownTimer <= 0 && player.canBeDamaged)
		{
			_damageCooldownTimer = damagecooldown;
			player.Health -= damagedealt;
		}
	}

	public void DamagedRecieved(int damage)
	{
		health -= damage;
		Visible = false;
		var timer = GetTree().CreateTimer(0.1f);
		timer.Timeout += () => { Visible = true; };
		if (health <= 0)
			KillEnemy();
	}

	public void KillEnemy()
	{
		DropItems();
		QueueFree();
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
		if (player == null) return false;
		if (GlobalPosition.DistanceTo(player.GlobalPosition) <= range && player.GlobalPosition.Y - GlobalPosition.Y < 0.2f)
			return true;
		else
			return false;

	}

	public bool isAtMeshEdge()
	{
		
		return !pathfindingray.IsColliding();
		
		
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


	private bool ValidateExports()
	{
		bool ok = true;

		if (damagecollider == null)
		{
			GD.PrintErr("EnemyController: 'damagecollider' export is null.");
			ok = false;
		}
		if (mesh == null)
		{
			GD.PrintErr("EnemyController: 'mesh' export is null.");
			ok = false;
		}
		if (player == null)
		{
			GD.PrintErr("EnemyController: 'player' export is null.");
			ok = false;
		}
		if (pathfindingray == null)
		{
			GD.PrintErr("EnemyController: 'pathfindingray' export is null.");
			ok = false;
		}
		if (statemachine == null)
		{
			GD.PrintErr("EnemyController: 'statemachine' export is null.");
			ok = false;
		}

		return ok;
	}

	public float GetRandomNumber()
	{
    return GD.RandRange(5, 9);
	}

	public void SetGravity()
	{
		if (Type == EnemyType.Flying) gravity = 0;
		else gravity = -9.8f;
	}

	public void initBounds()
	{

		



	}
	public bool isLeavingPatrolBounds()
	{



		bool isOutside =
		mesh.GlobalPosition.X < passivepatroldistance.Position.X - passivepatroldistance.Scale.X / 2 ||
		mesh.GlobalPosition.X > passivepatroldistance.Position.X + passivepatroldistance.Scale.X / 2 ||
		mesh.GlobalPosition.Y < passivepatroldistance.Position.Y - passivepatroldistance.Scale.Y / 2 ||
		mesh.GlobalPosition.Y > passivepatroldistance.Position.Y + passivepatroldistance.Scale.Y / 2;




		return isOutside;




	}

	public bool isPlayerInAggroBounds()
	{
		bool isOutside =
		player.GlobalPosition.X < activepatroldistance.GlobalPosition.X - activepatroldistance.Scale.X / 2 ||
		player.GlobalPosition.X > activepatroldistance.GlobalPosition.X + activepatroldistance.Scale.X / 2 ||
		player.GlobalPosition.Y < activepatroldistance.GlobalPosition.Y - activepatroldistance.Scale.Y / 2 ||
		player.GlobalPosition.Y > activepatroldistance.GlobalPosition.Y + activepatroldistance.Scale.Y / 2;




		return !isOutside;



	}
	
	public bool CurrentDirection()
    {
		if (this.Velocity.X >= 0) return true;
		else return false;
    }


}
