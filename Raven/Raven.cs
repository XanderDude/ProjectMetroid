using Godot;
using System;
public partial class Raven : CharacterBody3D
{
    public PlayerManager pm => GetParent<PlayerManager>();
    [Export] public float speed = 10.0f;
    
    [Export] public float launchTimer = 0.8f;

    public Vector3 direction = Vector3.Zero;
    public Vector3 targetPosition = Vector3.Zero;
    public Area3D swordHitbox;

    [Export] public CollisionShape3D topCollider = null;
    [Export] public CollisionShape3D bottomCollider = null;
    [Export] public CollisionShape3D bodyCollider = null;

    public Vector3 Yoffset = new Vector3(0, 1.5f, 0);
    public Vector3 Xoffset = new Vector3(0, 0, 0);
    public bool isInAction = false;
    public bool isOnPlayer = false;
    public bool isLaunching = false;
    public bool canTeleport = false;

    public bool canLaunch = false;
    public float ravenMeleeDamage = 0.0f;

	[Export] public RavenStateMachine rm;

    private bool wasUnlocked = false;

    public override void _Ready()
    {
        TopLevel = true;
    }

    public override void _PhysicsProcess(double delta)
    {
        bool unlocked = pm != null && pm.inventoryfriend.isUpgradeUnlocked("ravenSlash");
        if (bodyCollider != null) bodyCollider.Disabled = !unlocked;

        if (!unlocked)
        {
            Visible = false;
            wasUnlocked = false;
            return;
        }
        if (!wasUnlocked)
        {
            Visible = true;
            wasUnlocked = true;
        }

        if (rm.current_node_state != null)
        {
            if (rm.current_node_state_name == "RavenLaunchState" || rm.current_node_state_name == "RavenIdleState" || rm.current_node_state_name == "RavenAttackState")
            {
                this.CollisionMask = (1 << 0) | (1 << 1);
            }
            else
            {
                this.CollisionMask = 0;
            }
        }

        if (pm.IsOnFloor() || pm.psm.current_node_state_name == "mantleState")
        {
            canTeleport = true;
        }

        MoveAndSlide();
    }
}
