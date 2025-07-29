using Godot;
using System;

public partial class PlayerManager : CharacterBody3D
{
    private NodePath playerMeshPath = "%PlayerMesh"; //unique local node to the Player scene
    [Export] private GeometryInstance3D playerGeo;
    private MovementStateMachine _stateMachine;

    public bool jumpQueued; //player is holding the jump button
    public bool slideQueued; //player is holding slide button
    public bool slideBoost; //player is airborn/just landed

    private HUD hud;
    private int health = 100;

    [Export] private Material invulnMat;
    private PackedScene _projectile;

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
                hud.PlayerDied();
                GamePaused = true;
            }
            hud?.UpdateHealthBar(Health);
        }
    }

    [Export] public bool canBeDamaged = true;
    [Export] private float invulnTimer = 1f; //time before player can be damaged again
    private float _invulnTimer;

    [Export]
    private MovementStateMachine StateMachine //init StateMachine
    {
        get { return _stateMachine; }
        set //assign self and mesh to state machine prior to _ready
        {
            _stateMachine = value;
            StateMachine.Parent = this;
            StateMachine.ParentManager = this;
            StateMachine.parentMesh = GetNode<Node3D>(playerMeshPath);
        }
    }

    [Export] public Area3D slidingCollider;

    public override void _Ready()
    {
        invulnMat = ResourceLoader.Load<Material>("res://Environment/Materials/glowingMaterial.tres");
        _projectile = ResourceLoader.Load<PackedScene>("res://Environment/Props/Light_CandleTriple_Plate.tscn");
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
    public override void _UnhandledInput(InputEvent @event)
    {
       /* if (@event.IsActionPressed("Shoot"))
        {
            GD.Print("Spawning");
            var newProj = _projectile.Instantiate();
            GetParent().AddChild(newProj);
            newProj.GetNode<Node3D>(newProj.GetPath()).Transform = this.Transform;
        }*/
        
    } 


}
