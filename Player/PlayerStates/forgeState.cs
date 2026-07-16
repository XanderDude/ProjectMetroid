using Godot;
using System;

public partial class forgeState : PlayerState
{

    [Export] public float moveSpeed = 10.0f;
    private uint layerMasks = 0;

    public override void Enter()
    {
        GD.Print("Entering forge state");
        parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Airborne(false);
        player.Velocity = Vector3.Zero;
        layerMasks = player.CollisionMask;
        player.CollisionMask = 0;        
    }

    public override void Exit()
    {
        GD.Print("Exiting forge state");
        //Restore collision
       
        if (layerMasks != 0) player.CollisionMask = layerMasks; //layerMasks = 0 during initialization
    }

    public override void PhysicsUpdate(float delta)
    {
        Vector3 moveDirection = new(pm.aimDirection.X, pm.aimDirection.Y, 0);
        if (Input.IsActionPressed("Jump")) moveDirection.Y += 1;
        player.Velocity = moveDirection.Normalized() * moveSpeed;
        player.MoveAndSlide();
    }

    public override void HandleInput(InputEvent @event)
    {
        if (@event.IsActionPressed("forgemode"))
		{
			EmitSignal(SignalName.Transition, "jumpState");
        }
    }
}
