using Godot;
using System;

public partial class PlayerManager : CharacterBody3D
{
    private NodePath playerMeshPath = "%PlayerMesh"; //unique local node to the Player scene
    [Export] private GeometryInstance3D playerGeo;


    public bool jumpQueued; //player is holding the jump button
    public bool slideQueued; //player is holding slide button
    public bool slideBoost; //player is airborn/just landed

    private HUD hud;
    private int health = 100;

    [Export] private Material invulnMat;

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
            if (value < health) invulnTimer = _invulnTimer; //player is hurt
            health = value;
            if (health <= 0)
            {
                health = 0;
                hud?.PlayerDied();
                GamePaused = true;
            }
            hud?.UpdateHealthBar(Health);
        }
    }

    [Export] public bool canBeDamaged = true;
    [Export] private float invulnTimer = 1f; //time before player can be damaged again
    private float _invulnTimer;

    private MovementStateMachine _movementStateMachine;
    [Export]
    private MovementStateMachine StateMachine //init MovementStateMachine prior to _ready
    {
        get { return _movementStateMachine; }
        set //assign self and mesh to state machine prior to _ready
        {
            _movementStateMachine = value;
            StateMachine.Parent = this;
            StateMachine.ParentManager = this;
            StateMachine.parentMesh = GetNode<Node3D>(playerMeshPath);
        }
    }
    private AttackStateMachine _attackStateMachine;
    [Export]
    private AttackStateMachine AttackStateMachine //init AttackStateMachine
    {
        get { return _attackStateMachine; }
        set //assign self and mesh to state machine prior to _ready
        {
            _attackStateMachine = value;
            AttackStateMachine.Parent = this;
            AttackStateMachine.ParentManager = this;
            AttackStateMachine.parentMesh = GetNode<Node3D>(playerMeshPath);
        }
    }


    [Export] public Area3D slidingCollider;

    public override void _Ready()
    {
        invulnMat = ResourceLoader.Load<Material>("res://Environment/Materials/glowingMaterial.tres");
        hud = (HUD)GetTree().GetFirstNodeInGroup("hud");
        hud?.UpdateHealthBar(Health);
        _invulnTimer = invulnTimer;
        invulnTimer = 0; //reset timer
        playerGeo.MaterialOverlay = null;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (invulnTimer > 0)
        {
            playerGeo.MaterialOverlay = invulnMat;
            canBeDamaged = false;
            invulnTimer -= (float)delta;
        }
        else
        {
            canBeDamaged = true;
            playerGeo.MaterialOverlay = null;
        }

    }

}
