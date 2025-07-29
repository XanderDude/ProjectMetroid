using Godot;
using System;

public partial class PlayerManager : CharacterBody3D
{
	private NodePath playerMesh = "%PlayerMesh"; //unique local node to the Player scene
	private MovementStateMachine _stateMachine;
	private AttackStateMachine _attackstateMachine;


	public bool jumpQueued; //player is holding the jump button
	public bool slideQueued; //player is holding slide button
	public bool slideBoost; //player is airborn/just landed

	private HUD hud;
	private int health = 100;

	private bool gamePaused;
	public bool GamePaused
	{
		get { return gamePaused; }
		set
		{
			gamePaused = value;
			GetTree().Paused = value;
		}
	}

	public int Health //gets current health of the player, sets new health value and updates UI
	{
		get { return health; }
		set
		{
			health = value;
			if (health <= 0)
			{
				health = 0;
				hud.PlayerDied();
				GamePaused = true;
			}
			hud.UpdateHealthBar(Health);
		}
	}

	[Export]
	private MovementStateMachine StateMachine //init StateMachine
	{
		get { return _stateMachine; }
		set //assign self and mesh to state machine prior to _ready
		{
			_stateMachine = value;
			StateMachine.Parent = this;
			StateMachine.ParentManager = this;
			StateMachine.parentMesh = GetNode<Node3D>(playerMesh);
		}
	}
	
	[Export]
	private AttackStateMachine aStateMachine //init StateMachine
	{
		get { return _attackstateMachine; }
		set //assign self and mesh to state machine prior to _ready
		{
			_attackstateMachine = value;
			aStateMachine.Parent = this;
			aStateMachine.ParentManager = this;
			aStateMachine.parentMesh = GetNode<Node3D>(playerMesh);
		}
	}

	[Export] public Area3D slidingCollider;

	public override void _Ready()
	{
		
		hud = (HUD)GetTree().GetFirstNodeInGroup("hud");
		hud.UpdateHealthBar(Health);
	}
}
