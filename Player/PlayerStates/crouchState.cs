using Godot;
public partial class crouchState : PlayerState
{
    public override void Enter()
    {
        pm.ApplyFloorSnap();
        pm.aimDirection = new Vector2(pm.facingDirection, 0);
        pm.playerMesh.GetNode<PlayerAnimationHandler>(pm.playerMesh.GetPath()).Crouch(false);
    }
    public override void PhysicsUpdate(float delta)
    {
        pm.ApplyFloorSnap();
        if (!pm.IsOnFloor())
        {
            EmitSignal(SignalName.Transition, "jumpState");
            return;
        }
        Vector3 velocity = pm.Velocity;
        velocity.Y = 0;
        velocity.X = Mathf.MoveToward(velocity.X, 0, 15f * delta);
        pm.Velocity = velocity;
        pm.MoveAndSlide();
    }
    public override void HandleInput(InputEvent @event)
    {
        if (@event.IsActionPressed("Jump"))
        {
            if (pm.vertColCheck != null && pm.vertColCheck.VertCheckIsColliding())
                pm.playerMesh.GetNode<PlayerAnimationHandler>(pm.playerMesh.GetPath()).StandingBlocked();
            else
            {
                pm.jumpQueued = true;
                EmitSignal(SignalName.Transition, "jumpState");
            }
        }
        if (@event.IsActionPressed("Up") && !Input.IsActionPressed("Aim"))
        {
            if (pm.vertColCheck != null && pm.vertColCheck.VertCheckIsColliding())
                pm.playerMesh.GetNode<PlayerAnimationHandler>(pm.playerMesh.GetPath()).StandingBlocked();
            else
            {
                pm.playerMesh.GetNode<PlayerAnimationHandler>(pm.playerMesh.GetPath()).Grounded();
                EmitSignal(SignalName.Transition, "groundedState");
            }
        }
        if (@event.IsActionPressed("forgemode"))
            EmitSignal(SignalName.Transition, "forgeState");
        if (@event.IsActionPressed("Slide"))
            EmitSignal(SignalName.Transition, "slideState");
    }
}