using Godot;
using System;

public partial class forgeState : State
{

    [Export] public float moveSpeed = 10.0f;
    private uint layerMasks = 0;

    public override void Enter()
    {
        //GD.Print("Entering forge state");
        parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Airborne(false);
        pm.Velocity = Vector3.Zero;
        layerMasks = pm.CollisionMask;
        pm.CollisionMask = 0;        
    }

    public override void Exit()
    {
        //GD.Print("Exiting forge state");
        //Restore collision
       
        if (layerMasks != 0) pm.CollisionMask = layerMasks; //layerMasks = 0 during initialization
    }

    public override void PhysicsUpdate(float delta)
    {
        Vector3 moveDirection = new(pm.aimDirection.X, pm.aimDirection.Y, 0);
        if (Input.IsActionPressed("Jump")) moveDirection.Y += 1;
        pm.Velocity = moveDirection.Normalized() * moveSpeed;
        pm.MoveAndSlide();
    }

    public override void HandleInput(InputEvent @event)
    {
        if (@event.IsActionPressed("forgemode"))
		{
			msm.TransitionTo("jumpState");
        }
    }
}
