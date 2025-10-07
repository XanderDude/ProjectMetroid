using Godot;
using System;

public partial class walkState : State
{
    public override void Enter()
    {
        GD.Print("enemy entered walk state");
        
    }

    public override void Exit()
    {
    }

    public override void Update(float delta)
    {
        if (Controller.player != null && Controller.isPlayerInRange(2.0f))
        {
            Controller.StateMachine.TransitionTo("targetState");
        }


    }

    public override void PhysicsUpdate(float delta)
    {
        
        Controller.timer += delta;
        
        Controller.Velocity = new Vector3(Controller.MoveSpeed, -9.8f, 0);

        if (Controller.isAtMeshEdge() && Controller.timer > 0.5f)
        {
            Controller.RotationDegrees = new Vector3(0, Controller.RotationDegrees.Y + 180, 0);
            Controller.MoveSpeed = -Controller.MoveSpeed;
            Controller.timer = 0.0f;
        }
       
    }


}