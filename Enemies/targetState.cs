using Godot;
using System;

public partial class targetState : State
{
    public override void Enter()
    {
        GD.Print("enemy entered target state");
       
       
    }

    public override void Exit()
    {
        
    }

    public override void Update(float delta)
    {
        Controller.timer += delta;
        if (Controller.player != null && !Controller.isPlayerInRange(5.0f))
        {
            Controller.StateMachine.TransitionTo("recoverState");
        }


    }

    public override void PhysicsUpdate(float delta)
    {
        
        if (Controller.player != null && Controller.player.GlobalPosition.X < Controller.GlobalPosition.X)
        {
            
            Controller.Velocity = new Vector3(-Controller.MoveSpeed, -9.8f, 0);
        }
        else if (Controller.player != null && Controller.player.GlobalPosition.X > Controller.GlobalPosition.X)
        {
            Controller.Velocity = new Vector3(Controller.MoveSpeed * 2, -9.8f, 0);
        }
    
        
    }


}