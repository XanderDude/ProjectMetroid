using Godot;

public partial class knockbackState : PlayerState
{

    public float gravity = 9.8f;
    public override void Enter()
    {
        pm.Velocity = pm.knockbackVelocity;
        pm.playerMesh.GetNode<PlayerAnimationHandler>(pm.playerMesh.GetPath()).Airborne(false);
    }

    public override void PhysicsUpdate(float delta)
    {
        Vector3 velocity = pm.Velocity;
        velocity.Y -= gravity * 2.5f * delta; // same as jumpState falling gravity
        
        float input = Mathf.Sign(pm.aimDirection.X);
        velocity.X = Mathf.MoveToward(velocity.X, input * 5f, 3f * delta);
        
        pm.Velocity = velocity;
        pm.MoveAndSlide();

        if (pm.IsOnFloor())
            EmitSignal(SignalName.Transition, "groundedState");
    }
}