using Godot;
using System;

public partial class PlayerManager : CharacterBody3D
{
    [Export] public NodePath playerMeshPath = "%PlayerMesh";

	public bool jumpQueued;
	public bool slideQueued;
	public bool slideBoost;
	public int health = 10000;

	public bool isDead = false;
	public bool isPressed = false; 
	[Export] private ShaderMaterial invulnMat;


	



	public int Health
	{
		get { return health; }
		set
		{
			if (value < health) invulnTimer = _invulnTimer;
			health = value;
			if (health <= 0)
			{
				health = 0;


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

	[Export] public Area3D slidingCollider;
	[Export] public PackedScene jumpVFX;

	public override void _Ready()
	{

		
		_invulnTimer = invulnTimer;
		invulnTimer = 0;
		invulnMat?.SetShaderParameter("alpha", 0f);

		

		// Small delay to ensure all nodes are ready
		var timer = GetTree().CreateTimer(0.1f);
		
	}

	

	

	



	// Method to add items to appropriate inventory
	

	public override void _PhysicsProcess(double delta)
	{
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

		



	
	if (health <= 0 && !isDead)
	{
		ProcessMode = Node.ProcessModeEnum.Always;
		void MakeNodeBlack(Node node)
		{
			if (node is MeshInstance3D mesh)
			{
				var material = new StandardMaterial3D();
				material.AlbedoColor = Colors.Black;
				material.Emission = Colors.Black;
				mesh.MaterialOverride = material;
			}
			foreach (Node child in node.GetChildren())
				MakeNodeBlack(child);
		}

    void MakeNodeWhite(Node node)
    {
        if (node is MeshInstance3D mesh)
        {
            var material = new StandardMaterial3D();
            material.AlbedoColor = Colors.White;
            material.EmissionEnabled = true;
            material.Emission = Colors.White;
            mesh.MaterialOverride = material;
        }
        foreach (Node child in node.GetChildren())
            MakeNodeWhite(child);
    }

    MakeNodeBlack(GetTree().CurrentScene);
    MakeNodeWhite(GetNode<Node3D>("%PlayerMesh"));
    GetTree().Paused = true;
			_movementStateMachine.TransitionTo("deathState");
    isDead = true;
}
else if (health > 0 && isDead)
{
    GetTree().Paused = false;
    ProcessMode = Node.ProcessModeEnum.Inherit;
    SetPhysicsProcess(true);
    SetProcess(true);
    
    void RestoreMaterials(Node node)
    {
        if (node is MeshInstance3D mesh)
        {
            mesh.MaterialOverride = null;
        }
        foreach (Node child in node.GetChildren())
            RestoreMaterials(child);
    }

    RestoreMaterials(GetTree().CurrentScene);
    isDead = false;
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
