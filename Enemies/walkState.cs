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
        
    }

    public override void PhysicsUpdate(float delta)
    {
        Controller.timer += delta;
        Controller.Velocity = new Vector3(Controller.MoveSpeed, -9.8f, 0);

        if (Controller.IsAtMeshEdge() && Controller.timer > 2.0f)
        {
            Controller.timer = 0;
            Controller.MoveSpeed = -Controller.MoveSpeed;
        }
       
    }

    public override void HandleInput(InputEvent @event)
    {



        
    }
}