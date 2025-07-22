using Godot;
using System;

public partial class PlayerManager : CharacterBody3D
{
    private NodePath playerMesh = "%PlayerMesh"; //unique local node to the Player scene
    private MovementStateMachine _stateMachine;


    public bool jumpQueued; //player is holding the jump button
    public bool slideQueued; //player is holding slide button
    public bool slideBoost; //player is airborn/just landed

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

    [Export] public Area3D slidingCollider;

    public override void _Ready()
    {
        
    }
}
