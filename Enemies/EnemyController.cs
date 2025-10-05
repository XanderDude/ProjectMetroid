using Godot;
using System;
using System.Collections.Generic;

public partial class EnemyController : CharacterBody3D
{
	[Export] public int Health = 100;

	[Export] public int itemdropamount = 0;
	[Export] public float moveSpeed = 30;
	[Export] public int damage = 15;
	[Export] public float damageCooldown = .5f;
	private float _damageCooldown;
	[Export] public Area3D damageCollider;
	[Export] public Node3D mesh;

	public PlayerManager target;

	private Dictionary<string, EnemyState> _states;
	private EnemyState _currentState;

	public override void _Ready()
	{
		
		_damageCooldown = damageCooldown;

		_states = new Dictionary<string, EnemyState>();
		foreach (Node node in GetChildren())
		{
			if (node is EnemyState s)
			{
				_states[node.Name] = s;
				s.controller = this;  //assign self to the states
				s.mesh = mesh;
				s.Ready();
				s.Exit(); //reset all states
			}
		}
	}


	public override void _PhysicsProcess(double delta)
	{
		_currentState?.PhysicsUpdate((float)delta);
		if (damageCooldown > 0) damageCooldown -= (float)delta;

		if (damageCooldown <= 0 && target != null && target.canBeDamaged) //trap is ready and there is a target
		{
			damageCooldown = _damageCooldown;
			target.Health -= damage;
		}
	}
	public override void _Process(double delta)
	{

		_currentState?.Update((float)delta);
	}

	public void OnLeaveCollider(Node3D node)
	{
		target = null;
	}

	public void OnCollide(Node3D node)
	{
		target = node.GetNode<PlayerManager>(node.GetPath());
		if (damageCooldown <= 0 && target.canBeDamaged) //damaging collider 
		{
			damageCooldown = _damageCooldown;
			node.GetNode<PlayerManager>(node.GetPath()).Health -= damage;
		}
	}

	public void TransitionTo(string key)
	{
		if (!_states.TryGetValue(key, out EnemyState value) || _currentState == value) //return if state doesn't exist in dictionary or we're already in requested state
			return;

		_currentState.Exit();
		_currentState = value;
		_currentState.Enter();
	}

	public void DamagedRecieved(int damage)
	{
		Health -= damage;
		mesh.Visible = false;
		var timer = GetTree().CreateTimer(0.1f);
		timer.Timeout += () => { mesh.Visible = true; };
	}

	public void KillEnemy()
{
   
    Vector3 dropPosition = GlobalPosition;

		for (int i = 0; i < itemdropamount; i++)
		{
			var itemDropScene = GD.Load<PackedScene>("res://ItemDrop.tscn");
			var itemDropNode = itemDropScene.Instantiate();
			var itemDrop = itemDropNode as ItemDrop;

			if (itemDrop != null)
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
    
    
    QueueFree();
}

}
