using Godot;
using System;

public partial class backgroundState : State
{
    [Export] public PackedScene arrowScene;
    [Export] public float arrowSpeed = 20f;
    [Export] public float aimSpeed = 6f;
    [Export] public float aimRangeX = 6f;
    [Export] public float aimRangeYUp = 4f;
    [Export] public float aimRangeYDown = 1f;
    [Export] public float aimDepth = 15f;
    [Export] public float shootCooldown = 0.25f;
    [Export] public float camZoomIn = 8f;
    [Export] public Vector3 camShoulderOffset = new Vector3(0f, 0.4f, 0);

    private Vector2 aimPoint = Vector2.Zero;
    private float shootTimer = 0f;
    private float savedCamDistance;
    private MeshInstance3D laser;
    private Node3D arrowSpawnLoc;
    private bool active = false;

    public override void Ready()
    {
        var crossbow = pm.GetNodeOrNull<Node3D>("PlayerMesh/Skeleton3D/Crossbow");
        if (crossbow == null)
        {
            GD.PrintErr("backgroundState: crossbow not found at PlayerMesh/Skeleton3D/Crossbow");
            return;
        }
        if (crossbow.GetChildCount() > 0)
            arrowSpawnLoc = crossbow.GetChild(0) as Node3D;

            
    }

    public override void Enter()
    {
        active = true;
        asm.TransitionTo("noattackState");
        pm.movementlocked = true;
        player.Velocity = Vector3.Zero;
        aimPoint = Vector2.Zero;
        shootTimer = 0f;

        parentMesh.RotationDegrees = new Vector3(0, 180f, 0);

        var cam = GameFriend.gameinstance.camera;
        savedCamDistance = cam.cameraDistance;
        cam.ZoomTo(camZoomIn, 0.2f);
        cam.OffsetTo(camShoulderOffset, 0.2f);

        laser = new MeshInstance3D();
        laser.Mesh = new BoxMesh { Size = new Vector3(0.03f, 0.03f, 1f) };
        laser.MaterialOverride = new StandardMaterial3D
        {
            AlbedoColor = new Color(1f, 0f, 0f),
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            EmissionEnabled = true,
            Emission = new Color(1f, 0.1f, 0.1f),
            EmissionEnergyMultiplier = 2f
        };
        player.GetParent().AddChild(laser);
    }

    public override void Exit()
    {
        if (!active) return;   // machine calls Exit() on all states at boot — nothing to undo
        active = false;

        pm.movementlocked = false;

        var cam = GameFriend.gameinstance.camera;
        cam.ZoomTo(savedCamDistance, 0.2f);
        cam.OffsetTo(Vector3.Zero, 0.2f);

        parentMesh.RotationDegrees = new Vector3(0, pm.facingDirection == 1 ? 90f : -90f, 0);

        if (IsInstanceValid(laser))
            laser.QueueFree();
    }

    public override void PhysicsUpdate(float delta)
    {
        player.Velocity = new Vector3(0, player.IsOnFloor() ? 0 : player.Velocity.Y, 0);
        player.ApplyFloorSnap();
        player.MoveAndSlide();

        Vector2 input = new(Input.GetAxis("Left", "Right"), Input.GetAxis("Down", "Up"));
        aimPoint += input * aimSpeed * delta;
        aimPoint.X = Mathf.Clamp(aimPoint.X, -aimRangeX, aimRangeX);
        aimPoint.Y = Mathf.Clamp(aimPoint.Y, -aimRangeYDown, aimRangeYUp);

        if (arrowSpawnLoc != null)
            UpdateLaser();

        shootTimer -= delta;
        if (Input.IsActionPressed("Shoot") && shootTimer <= 0f)
        {
            ShootIntoBackground();
            shootTimer = shootCooldown;
        }

        if (!Input.IsActionPressed("ShootBackground"))
            msm.TransitionTo("groundedState");
    }

    private Vector3 AimTarget()
    {
        Vector3 origin = arrowSpawnLoc.GlobalPosition;
        return new Vector3(origin.X + aimPoint.X, origin.Y + aimPoint.Y, origin.Z - aimDepth);
    }

    private void UpdateLaser()
    {
        Vector3 origin = arrowSpawnLoc.GlobalPosition;
        Vector3 target = AimTarget();
        Vector3 dir = target - origin;

        laser.GlobalPosition = origin + dir * 0.5f;
        laser.LookAt(target, Vector3.Up);
        laser.Scale = new Vector3(1, 1, dir.Length());
    }

    private void ShootIntoBackground()
    {
        if (arrowScene == null || arrowSpawnLoc == null) return;

        var arrow = arrowScene.Instantiate() as RigidBody3D;
        arrow.GravityScale = 0f;
        arrow.AxisLockLinearZ = false;     // background shots need Z
        arrow.AxisLockAngularX = false;
        arrow.AxisLockAngularY = false;
        player.GetParent().AddChild(arrow);
        arrow.GlobalPosition = arrowSpawnLoc.GlobalPosition;

        Vector3 dir = (AimTarget() - arrowSpawnLoc.GlobalPosition).Normalized();
        arrow.LinearVelocity = dir * arrowSpeed;
        arrow.LookAt(arrow.GlobalPosition + dir, Vector3.Up);

        SoundFriend.Play("player_normal_shoot_SFX");
        parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Shooting();
    }

    public override void Update(float delta) { }
    public override void HandleInput(InputEvent @event) { }
}