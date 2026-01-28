using Godot;
using System;	

public partial class Player : Actor
{
	public bool jumpQueued, slideQueued, slideBoost;
	private bool isDead, isPressed = false;

	public bool inWater = false;

	[Export] private ShaderMaterial mainMat, weaponMat;
	private float alpha = 0f;

	public Vector2 aimDirection = Vector2.Right;
	public bool noAimDirection;
	public int facingDirection = 1; //1 is right, -1 is left
	[Export] public VerticalCollisionCheck vertColCheck;
	[Export] public ShapeCast3D groundCheck;

	[Export] public int Health
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
		}
	}
	[Export] public PackedScene jumpVFX, slideBoostVFX;

	public override void _Ready()
	{
		

		_invulnTimer = invulnTimer;
		invulnTimer = 0;
		
		mesh = GetNode<Node3D>("PlayerMesh");
		GD.Print($"Player mesh assigned: {mesh != null}");
		
		// Now set mesh on the state machines after everything is ready
		if (StateMachine != null)
		{
			StateMachine.parentMesh = mesh;
			GD.Print($"Movement SM parentMesh set: {StateMachine.parentMesh != null}");
		}
		if (AttackStateMachine != null)
		{
			AttackStateMachine.parentMesh = mesh;
			GD.Print($"Attack SM parentMesh set: {AttackStateMachine.parentMesh != null}");
		}
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
		
		if (direction != aimDirection) aimDirection = direction.Normalized(); //update aim direction only if it has changed
		
		if (invulnTimer > 0)
		{
			canBeDamaged = false;
			invulnTimer -= (float)delta;
			mainMat?.SetShaderParameter("flashing", true);
			mainMat?.SetShaderParameter("timer", invulnTimer);
			weaponMat?.SetShaderParameter("flashing", true);
			weaponMat?.SetShaderParameter("timer", invulnTimer);
		}
		else
		{
			canBeDamaged = true;
			mainMat?.SetShaderParameter("flashing", false);
			weaponMat?.SetShaderParameter("flashing", false);
		}		
	}
	public void SpawnJumpCloud(float yOffset,float rotation)
	{
		var jumpCloud = jumpVFX.Instantiate() as Node3D;
		AddChild(jumpCloud, true);
		jumpCloud.GlobalPosition = new(GlobalPosition.X, GlobalPosition.Y + yOffset, GlobalPosition.Z);
		jumpCloud.RotationDegrees = new(0, 0, rotation);
	}

}
