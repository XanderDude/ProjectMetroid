using Godot;
using System;

public partial class recoverState : State
{
    public override void Enter()
    {
        GD.Print($"{Controller.Name} entered recover State");
        Controller.timer = 0;
    }

    public override void Exit()
    {
        
    }

    public override void Update(float delta)
    {
        Controller.timer += delta;
        var num = Controller.GetRandomNumber();
        if (Controller.timer >= num)
        {
            Controller.statemachine.TransitionTo("patrolState");
        }

        if (Controller.isPlayerInAggroBounds())
        {
            Controller.statemachine.TransitionTo("aggroState");
        }

    }

    public override void PhysicsUpdate(float delta)
    {
        if (Controller.speed > 0.1f)
        {
            Controller.speed -= delta;
        }
        if (Controller.speed < -0.1f)
        {
            Controller.speed += delta;
        }
        else
        {
            Controller.speed = Controller.idle * Mathf.Sign(Controller.speed);
        }
          Controller.Velocity = new Godot.Vector3(Controller.speed, Controller.gravity, 0);
        
    
        
    }


}