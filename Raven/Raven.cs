using Godot;
using System;
public partial class Raven : CharacterBody3D
{
    [Export] public PlayerManager player = null;

    public Godot.AudioStreamPlayer sound = null;
    [Export] public float speed = 10.0f;
    
    [Export] public float launchTimer = 0.8f;

    public Vector3 direction = Vector3.Zero;
    public Vector3 targetPosition = Vector3.Zero;
    public Area3D swordHitbox;
    public RavenStateMachine rsm;

    [Export] public CollisionShape3D topCollider = null;
    [Export] public CollisionShape3D bottomCollider = null;

    public Vector3 Yoffset = new Vector3(0, 1.5f, 0);
    public Vector3 Xoffset = new Vector3(0, 0, 0);
    public bool isInAction = false;

    public bool isOnPlayer = false;
    public bool isLaunching = false;
    public bool canTeleport = false;

    public bool canLaunch = false;
    public float ravenMeleeDamage = 0.0f;

    public InventoryFriend inventoryfriend => GameFriend.gameinstance.inventoryfriend;

    public override void _Ready()
    {
        sound = GetNode<Godot.AudioStreamPlayer>("Sound");
        rsm = GetNode<RavenStateMachine>("RavenStateMachine");
        if (player == null)
        {
            GD.PrintErr("Raven: player not found");
        }

        

    }

    public override void _PhysicsProcess(double delta)
    {
        

        if (rsm != null && rsm._currentState != null)
        {
            if (rsm._currentState.Name == "RavenLaunchState" || rsm._currentState.Name == "RavenIdleState" || rsm._currentState.Name == "RavenAttackState")
            {
                this.CollisionLayer = 1 << 4;
                this.CollisionMask = (1 << 0) | (1 << 1);
            }
            else
            {
                this.CollisionLayer = 0;
                this.CollisionMask = 0;
            }
        }

        if (player.IsOnFloor() || player.StateMachine._currentState.Name == "mantleState")
        {
            canTeleport = true;
        }

        MoveAndSlide();
            
    }
    



}
