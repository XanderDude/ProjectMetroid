using Godot;
using System;

public partial class RavenIdleState : State
{
    public override void Enter()
    {
        //GD.Print("Raven: Entered Idle State");

        if (raven.player.IsOnFloor())
        {
            raven.canTeleport = true;
        }
        
    }

    public override void Exit()
    {
    }

    public override void Update(float delta)
    {
    }

    public override void PhysicsUpdate(float delta)
    {
        raven.Velocity = Vector3.Zero;
    }

    public override void HandleInput(InputEvent @event)
    {
        if (@event.IsActionPressed("RavenSpecial"))
        {
            rsm.TransitionTo("RavenRecallState");
        }

        if (@event.IsActionPressed("RavenSlash"))
        {
        

                var collision = raven.player.GetLastSlideCollision();
                if (collision != null && ((collision.GetCollider() as CollisionObject3D)?.CollisionLayer & (1 << 0)) != 0 && raven.player.StateMachine._currentState.Name != "mantleState")
                {
                    if (raven.canTeleport)
                    {
                    //GD.Print("Raven: Teleported to player position" + raven.player.StateMachine._currentState.Name);

                    raven.player.GlobalPosition = raven.GlobalPosition;
                    raven.canTeleport = false;
                    raven.player.Velocity = Vector3.Zero;
                    }
                }
                else
                {
                    //GD.Print("Raven: Teleport failed, no valid collision");
                    rsm.TransitionTo("RavenRecallState");
                }
                
                
                
            

           
        }
    }


}