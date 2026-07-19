using Godot;

public partial class knockbackState : State
{
    public override void Enter()
    {
        
        pm.Velocity = pm.knockbackVelocity;
        parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Airborne(false);
    }

    public override void PhysicsUpdate(float delta)
    {
        Vector3 velocity = pm.Velocity;
        velocity.Y -= _gravity * 2.5f * delta; // same as jumpState falling gravity
        
        float input = Mathf.Sign(pm.aimDirection.X);
        velocity.X = Mathf.MoveToward(velocity.X, input * 5f, 3f * delta);
        
        pm.Velocity = velocity;
        pm.MoveAndSlide();

        if (pm.IsOnFloor())
            msm.TransitionTo("groundedState");
    }
}