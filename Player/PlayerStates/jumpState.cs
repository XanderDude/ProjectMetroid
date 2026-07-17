using Godot;
using System;

public partial class jumpState : PlayerState
{
    [ExportSubgroup("Jump State")]
    [Export] public float gravity = 9.8f;
    [Export] public float airMaxSpeed = 5.0f;
    [Export] public float jumpAcceleration = 10.0f;
    [Export] public float jumpDeceleration = 15f;
    [Export] public float jumpVelocity = 10.0f;
    [Export] public float jumpMaxHeight = 2f, jumpMinHeight = 0.2f;
    [Export] public float pmTopWhileInJump = 1.322f;

    private float _mantleCooldown = .2f;
    private float mantleTimer = 1;
    public float jumpHeight = 0.0f;
    private float startPosition = 0.0f;
    private bool cancelVelocity = true;
    public float meshTop = 0.0f;
    private const float pm_AND_MESH_OFFSET = 0.2f;

    private bool isTouching() => pm.GetSlideCollisionCount() != 0;

    private bool isSameHeight()
    {
        if (!isTouching() && !isGreaterHeight()) return false;

        for (int i = 0; i < pm.GetSlideCollisionCount(); i++)
        {
            KinematicCollision3D collision = pm.GetSlideCollision(i);
            try
            {
                var collider = collision.GetCollider() as StaticBody3D;
                if (!collider.GetCollisionLayerValue(1)) continue;
                var boxShape = (BoxShape3D)collider.GetChild<CollisionShape3D>(0).Shape;

                var scaleY = new Vector3(
                    collider.Transform.Basis.X.Y * boxShape.Size.Y,
                    collider.Transform.Basis.Y.Y * boxShape.Size.Y,
                    collider.Transform.Basis.Z.Y * boxShape.Size.Y).Length();

                float pmHeight = pm.GlobalPosition.Y + pmTopWhileInJump + 0.5f;
                meshTop = collider.GlobalPosition.Y + scaleY / 2;

                if (Mathf.Abs(pmHeight - meshTop) < pm_AND_MESH_OFFSET && pmHeight >= meshTop)
                    return true;
            }
            catch { continue; }
        }
        return false;
    }

    private bool isGreaterHeight()
    {
        if (pm.GetSlideCollisionCount() == 0) return false;
        var collider = pm.GetSlideCollision(0).GetCollider() as Node3D;
        float pmHeight = pm.GlobalPosition.Y + pmTopWhileInJump;
        return Mathf.Abs(pmHeight - collider.GlobalPosition.Y) > 0.1f;
    }

    private bool IsAscending(float delta, ref Vector3 velocity)
    {
        if (jumpHeight < jumpMaxHeight)
        {
            velocity.Y = jumpVelocity;
            jumpHeight = pm.GlobalPosition.Y - startPosition;
            return true;
        }
        velocity.Y = pm.Velocity.Y / 2;
        pm.jumpQueued = false;
        jumpHeight = jumpMaxHeight;
        return false;
    }

    private void CancelUpwardVelocity()
    {
        pm.jumpQueued = false;
        pm.Velocity = new Vector3(pm.Velocity.X, pm.Velocity.Y / 2, pm.Velocity.Z);
    }

    public override void Enter()
    {
        GD.Print("Entered Jump State");
        if (pm.jumpQueued)
        {
            if (pm.jumpVFX != null && pm.psm.previous_node_state_name != "mantleState")
                pm.SpawnJumpCloud(0, 0);
            jumpHeight = 0.0f;
            startPosition = pm.GlobalPosition.Y;
            SoundFriend.Play("player_jump_SFX");
            if (pm.noAimDirection) cancelVelocity = true;
        }
        pm.slideBoost = true;
        pm.pah.Airborne(pm.jumpQueued);
    }

    public override void Exit()
    {
        pm.jumpQueued = false;
    }

    public override void PhysicsUpdate(float delta)
    {
        mantleTimer += delta;

        if (cancelVelocity) { pm.Velocity = Vector3.Zero; cancelVelocity = false; }

        if (pm.jumpQueued && !Input.IsActionPressed("Jump") && jumpHeight > jumpMinHeight)
            CancelUpwardVelocity();

        if (pm.jumpQueued && jumpHeight != 0 &&
            Mathf.Floor(jumpHeight * 1000) == Mathf.Floor((pm.GlobalPosition.Y - startPosition) * 1000))
            CancelUpwardVelocity();

        HandleAirMovement(delta);
        pm.MoveAndSlide();

        // Check IsOnFloor() only after this frame's MoveAndSlide has actually resolved
        // collision - checking beforehand reflects last frame's (stale) floor state, so the
        // still-falling velocity gets fed into MoveAndSlide for one extra frame after actually
        // touching down, digging slightly into the floor every time before landing is recognized.
        if (!pm.jumpQueued && pm.IsOnFloor())
        {
            pm.Velocity = new Vector3(pm.Velocity.X, 0, 0);
            if (Input.IsActionPressed("Slide") && pm.aimDirection.X != 0)
            {
                pm.pah.Sliding(true);
                EmitSignal(SignalName.Transition, "slideState"); return;
            }
            else if (pm.vertColCheck != null && pm.vertColCheck.VertCheckIsColliding())
            {
                pm.pah.Crouch(true);
                EmitSignal(SignalName.Transition, "crouchState"); return;
            }
            else
            {
                pm.pah.Grounded();
                EmitSignal(SignalName.Transition, "groundedState"); return;
            }
        }
        else if (GameFriend.gameinstance.inventoryfriend.isUpgradeUnlocked("mantling") &&
                 mantleTimer >= _mantleCooldown && pm.Velocity.Y <= 0 && pm.aimDirection.X != 0 && isSameHeight())
        {
            mantleTimer = 0;
            pm.Velocity = Vector3.Zero;
            EmitSignal(SignalName.Transition, "mantleState");
        }
    }

    private void HandleAirMovement(float delta)
    {
        Vector3 velocity = pm.Velocity;
        float input = pm.noAimDirection ? 0 : Mathf.Sign(pm.aimDirection.X);

        if (pm.jumpQueued && IsAscending(delta, ref velocity))
        {
            velocity.Y -= gravity * delta;
            velocity.X = input == 0 ? Mathf.MoveToward(velocity.X, 0, jumpDeceleration) : input * airMaxSpeed;
        }
        else
        {
            pm.groundCheck.ForceShapecastUpdate();
            velocity.Y -= gravity * 2.5f * delta;
            velocity.X = input == 0 ? Mathf.MoveToward(velocity.X, 0, jumpDeceleration) : input * airMaxSpeed;
        }

        if (Input.IsActionPressed("Aim")) velocity.X = pm.Velocity.X;
        velocity.X = Mathf.Clamp(velocity.X, -airMaxSpeed, airMaxSpeed);
        velocity.Y = Mathf.Max(velocity.Y, -gravity * 2f);
        pm.Velocity = velocity;
    }

    public override void HandleInput(InputEvent @event)
    {
        if (@event.IsActionPressed("forgemode"))
            EmitSignal(SignalName.Transition, "forgeState");

        if (@event.IsActionPressed("Jump") &&
            GameFriend.gameinstance.inventoryfriend.isUpgradeUnlocked("wallJump") &&
            !pm.IsOnFloor() && isTouching() && !isSameHeight())
            EmitSignal(SignalName.Transition, "walljumpState");
    }
}