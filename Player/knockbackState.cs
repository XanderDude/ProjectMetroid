using Godot;

public partial class knockbackState : PlayerState
{

    public float gravity = 9.8f;
    public override void Enter()
    {
        player.Velocity = pm.knockbackVelocity;
        parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Airborne(false);
    }

    public override void PhysicsUpdate(float delta)
    {
        Vector3 velocity = player.Velocity;
        velocity.Y -= gravity * 2.5f * delta; // same as jumpState falling gravity
        
        float input = Mathf.Sign(pm.aimDirection.X);
        velocity.X = Mathf.MoveToward(velocity.X, input * 5f, 3f * delta);
        
        player.Velocity = velocity;
        player.MoveAndSlide();

        if (player.IsOnFloor())
            EmitSignal(SignalName.Transition, "groundedState");
    }
}