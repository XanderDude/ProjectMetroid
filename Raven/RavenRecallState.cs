using Godot;
using System;

public partial class RavenRecallState : State
{
    public override void Enter()
    {
        //GD.Print("Raven: Entered Recall State");
        raven.isInAction = false;

    }

    public override void Exit()
    {
        //GD.Print("Raven: Exited Recall State");
      
    }

    public override void Update(float delta)
    {
    }

    public override void PhysicsUpdate(float delta)
    {
        raven.Yoffset = new Vector3(0, 1.5f, 0);
        raven.direction = raven.GlobalPosition.DirectionTo(raven.player.GlobalPosition + raven.Yoffset);

        raven.Velocity = (raven.direction) * raven.speed;

        raven.isOnPlayer = (raven.player.GlobalPosition + raven.Yoffset).DistanceTo(raven.GlobalPosition) < 0.8f;
       
        

    }

    public override void HandleInput(InputEvent @event)
    {

        if (@event.IsActionPressed("RavenSpecial") && raven.isOnPlayer)
        {
            rsm.TransitionTo("RavenLaunchState");
        }

        if (@event.IsActionPressed("RavenSlash") && raven.isOnPlayer)
        {

            rsm.TransitionTo("RavenAttackState");
            
        }

    }

}