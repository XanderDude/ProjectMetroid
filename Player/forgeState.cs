using Godot;
using System;

public partial class forgeState : State
{

    

    public override void Enter()
    {
        GD.Print("Entering forge state");
        parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).BeginJump();
        //Remove collision on layer 3 
        pm.CollisionMask = (0 << 1) | (0 << 2) | (0 << 4);

        
        
    }

    public override void Exit()
    {
        GD.Print("Exiting forge state");
        //Restore collision
       
        pm.CollisionMask = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 4);
       
    }

    public override void Update(float delta)
    {
       
        

    }

    public override void PhysicsUpdate(float delta)
    {
        player.MoveAndSlide();
    }

    public override void HandleInput(InputEvent @event)
    {
        if (@event.IsActionPressed("forgemode"))
        {
            player.Set("jumpQueued", false);
            
            msm.TransitionTo("jumpState");
        }

        if (@event.IsActionPressed("Jump") || @event.IsActionPressed("Up"))
        {
            player.Velocity = new Vector3(player.Velocity.X, 10, player.Velocity.Z);
        }

        else if (@event.IsActionPressed("Down"))
        {
            player.Velocity = new Vector3(player.Velocity.X, -10, player.Velocity.Z);
        }

        else if (@event.IsActionReleased("Jump") || @event.IsActionReleased("Up") || @event.IsActionReleased("Down"))
        {
            player.Velocity = new Vector3(player.Velocity.X, 0, player.Velocity.Z);
        }

        if (@event.IsActionPressed("Left"))
        {
            player.Velocity = new Vector3(-10, player.Velocity.Y, player.Velocity.Z);
        }

        else if (@event.IsActionPressed("Right"))
        {
            player.Velocity = new Vector3(10, player.Velocity.Y, player.Velocity.Z);
        }

        else if (@event.IsActionReleased("Left") || @event.IsActionReleased("Right"))
        {
            player.Velocity = new Vector3(0, player.Velocity.Y, player.Velocity.Z);
        }
    }
}
