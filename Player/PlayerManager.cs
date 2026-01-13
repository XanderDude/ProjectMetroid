using Godot;
using System;

public partial class PlayerManager : CharacterBody3D
{
    [Export] public NodePath playerMeshPath = "%PlayerMesh";

	public bool jumpQueued;
	public bool slideQueued;
	public bool slideBoost;
	[Export] public int health = 100;

	public bool isDead = false;
	public bool isPressed = false; 

	public bool inwater = false;
	[Export] private ShaderMaterial invulnMat;

	public Vector2 aimDirection;
	public bool noAimDirection;
	[Export] public RayCast3D groundCheck;

	public int Health
	{
		get { return health; }
		set
		{
			if (value < health && invulnTimer > 0) return;
			if (value < health) invulnTimer = _invulnTimer;
			health = value;
			if (health <= 0)
			{
				health = 0;
				isDead = true;

			}

		}
	}

	[Export] public bool canBeDamaged = true;
	[Export] private float invulnTimer = 1f;
	private float _invulnTimer;
	private MovementStateMachine _movementStateMachine;

	[Export]
	public MovementStateMachine StateMachine
	{
		get { return _movementStateMachine; }
		set
		{
			_movementStateMachine = value;
			StateMachine.Parent = this;
			StateMachine.ParentManager = this;
			StateMachine.parentMesh = GetNode<Node3D>(playerMeshPath);
		}
	}

	private AttackStateMachine _attackStateMachine;

	[Export]
	private AttackStateMachine AttackStateMachine
	{
		get { return _attackStateMachine; }
		set
		{
			_attackStateMachine = value;
			AttackStateMachine.Parent = this;
			AttackStateMachine.ParentManager = this;
			AttackStateMachine.parentMesh = GetNode<Node3D>(playerMeshPath);
		}
	}
	[Export] public PackedScene jumpVFX, slideBoostVFX;

	public override void _Ready()
	{
		//Input.GetActionStrength
		
		_invulnTimer = invulnTimer;
		invulnTimer = 0;
		invulnMat?.SetShaderParameter("alpha", 0f);

		// Small delay to ensure all nodes are ready
		var timer = GetTree().CreateTimer(0.1f);
		
	}

	

	public override void _Process(double delta)
	{
		if (isDead) StateMachine.TransitionTo("deathState");
	}
	

	public override void _PhysicsProcess(double delta)
	{
		var direction = new Vector2(Input.GetAxis("Left", "Right"), Input.GetAxis("Down", "Up")).Normalized();		
		//new Vector2(Mathf.CeilToInt(Mathf.Abs(Input.GetAxis("Left", "Right"))) * Mathf.Sign(Input.GetAxis("Left", "Right")), Mathf.CeilToInt(Mathf.Abs(Input.GetAxis("Down", "Up"))) * Mathf.Sign(Input.GetAxis("Down", "Up")));
		
		if (direction == Vector2.Zero) noAimDirection = true;
		else
        {
			noAimDirection = false;	
            if (Mathf.Abs(direction.X) > .9) direction = new(direction.X, 0);
			else if (Mathf.Abs(direction.Y) > .9) direction = new(0, direction.Y);
			else direction = new(Mathf.Sign(direction.X), Mathf.Sign(direction.Y));
        }
		
		if (direction != aimDirection) aimDirection = direction.Normalized();
		
		if (invulnTimer > 0)
		{
			invulnMat?.SetShaderParameter("alpha", 1f);
			GetNode<Node3D>(playerMeshPath).Visible = false;
			var timer = GetTree().CreateTimer(0.1f);
			timer.Timeout += () => { GetNode<Node3D>(playerMeshPath).Visible = true; };
			canBeDamaged = false;
			invulnTimer -= (float)delta;
		}
		else
		{
			canBeDamaged = true;
			invulnMat?.SetShaderParameter("alpha", 0f);
		}


		
	}
	public void SpawnJumpCloud(float rotation)
	{
		var jumpCloud = jumpVFX.Instantiate() as Node3D;
		this.AddChild(jumpCloud, true);
		jumpCloud.GlobalPosition = GlobalPosition;
		jumpCloud.RotationDegrees = new(rotation, 0, 0);
	}
}
