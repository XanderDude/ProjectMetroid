using Godot;
using System;

public partial class Enemy : Actor
{

	[Export] private int dropRate1 = 100, dropRate2 = 0; //<-  need to put this on item 

	[ExportGroup("Node References")]

	private float meshDirection = -90f;
	[Export] public RayCast3D ray;
	[Export] public PackedScene item1, item2;
	[Export] public RayCast3D edgeray;
	private EnemyStateMachine statemachine => GetNode<EnemyStateMachine>("EnemyStateMachine");

	public Godot.Vector3 direction { get; set; } = Godot.Vector3.Zero;
	public Godot.Vector3 targetposition { get; set; } = Godot.Vector3.Zero;

	private bool playerInDamageRange = false; //player is within damage collider

	public Player player => GameFriend.gameinstance.player;
	public override void _Ready()
	{
		meshDirection = mesh.RotationDegrees.Y;
    }

	public override void _PhysicsProcess(double delta)
	{
		statemachine?._currentState?.PhysicsUpdate((float)delta);
		if (playerInDamageRange && player.canBeDamaged)
		{
			player.Health -= damagedealt;
		}
		if (Velocity.X < -0.5) mesh.RotationDegrees = new Vector3(0, -meshDirection, 0);
		else if (Velocity.X > 0.5) mesh.RotationDegrees = new Vector3(0, meshDirection, 0);
		MoveAndSlide();
	}

	public override void _Process(double delta)
	{
		statemachine?._currentState?.Update((float)delta);
	}

	public void OnCollide(Node3D node)
	{
		playerInDamageRange = true;
	}

	public void OnLeaveCollider(Node3D node)
	{
		playerInDamageRange = false;
	}

	public void DamagedRecieved(int damage)
	{


		//GD.Print($"{Name} took {damage} damage");
		DamageFlicker();

		if (health <= 0) return; //already dead
		health -= damage;
		if (health <= 0)
		{
			Kill();
		}
	}

	public void Kill()
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
		Vector3 dropPosition = GlobalPosition;
		for (int i = 0; i < numItemDrops; i++)
        {
            if (item1 != null && GD.RandRange(0, 100) <= dropRate1)
            {
                SpawnItem(dropPosition, item1);
            }
            if (item2 != null && GD.RandRange(0, 100) <= dropRate2)
			{
				SpawnItem(dropPosition, item2);
			}

            
        }

        void SpawnItem(Vector3 dropPosition, PackedScene itemScene)
        {

            var itemDropNode = itemScene.Instantiate();
            if (itemDropNode is ItemDrop itemDrop)
            {
                GetParent().AddChild(itemDrop);
                itemDrop.GlobalPosition = dropPosition;
            }
            else
            {
                GD.PrintErr("Missing item drop scene reference");
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
