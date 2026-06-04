using Godot;
using System;

public partial class Enemy : Actor
{

	[ExportGroup("EnemyReferences")]
	[Export] public Node3D mesh;
	[Export] public NavigationAgent3D navagent;
	[Export] public EnemyStateMachine statemachine { get; private set; }
	
	[Export] public RayCast3D ray;

	[Export] public RayCast3D edgeray;
	[Export] public PackedScene projectilescene = null;
	private float meshDirection = -90f;
	public PlayerManager player => GameFriend.gameinstance.player;
	public Godot.Vector3 direction { get; set; } = Godot.Vector3.Zero;
	public Godot.Vector3 targetposition { get; set; } = Godot.Vector3.Zero;

	private bool playerInDamageRange = false; //player is within damage collider

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

	public void DamagedReceived(int damage)
	{
		//GD.Print($"{this.Name} took {damage} damage");
		DamageFlicker();

		if (health <= 0) return; //already dead
		health -= damage;
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



	public bool isPlayerInRange(float range)
	{
		float distance = GlobalPosition.DistanceTo(player.GlobalPosition);
	
		if (distance <= range) return true;
		
		return false;
	}

	
	public bool isMovingRight()
	{
		if (this.Velocity.X >= 0) return true;
		return false;
	}
	
	}
