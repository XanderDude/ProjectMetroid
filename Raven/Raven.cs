using Godot;
using System;
public partial class Raven : CharacterBody3D
{
    public PlayerManager player => GameFriend.gameinstance.player;
    [Export] public float speed = 10.0f;
    
    [Export] public float launchTimer = 0.8f;

    public Vector3 direction = Vector3.Zero;
    public Vector3 targetPosition = Vector3.Zero;
    public Area3D swordHitbox;

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

    private RavenStateMachine _ravenStateMachine;

    /*
	[Export] public RavenStateMachine RavenStateMachine
	{
		get { return _ravenStateMachine; }
		set
		{
			_ravenStateMachine = value;
			RavenStateMachine.Parent = this;
			RavenStateMachine.PlayerManager = player;
		}
	}
    */

    public void init_raven()
    {
        if (player != null)
        {
            //RavenStateMachine.PlayerManager = player;
        }        
    }

/*
    public override void _PhysicsProcess(double delta)
    {        

        if (RavenStateMachine._currentState != null)
        {
            if (RavenStateMachine._currentState.Name == "RavenLaunchState" || RavenStateMachine._currentState.Name == "RavenIdleState" || RavenStateMachine._currentState.Name == "RavenAttackState")
            {
                this.CollisionMask = (1 << 0) | (1 << 1);
            }
            else
            {
                this.CollisionMask = 0;
            }
        }

        if (player != null && (player.IsOnFloor() || player.psm.current_node_state_name == "mantleState"))
        {
            canTeleport = true;
        }

        MoveAndSlide();
            
    }
    */



}
