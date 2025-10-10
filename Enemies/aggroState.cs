using Godot;
using System;
using System.Numerics;

public partial class aggroState : State
{

    
    public override void Enter()
    {

        GD.Print($"{Controller.Name} entered aggro State");
        Controller.speed = Controller.runspeed * Mathf.Sign(Controller.speed);
        
        
    }

    public override void Exit()
    {
    }

    public override void Update(float delta)
    {
         if (!Controller.isPlayerInAggroBounds())
        {
            Controller.statemachine.TransitionTo("patrolState");
        }
        
    }
    public override void PhysicsUpdate(float delta)
    {
        if (Controller.currentDirection != Mathf.Sign(Controller.speed) * Controller.currentDirection) Controller.mesh.RotateY(Mathf.Pi);
         float directionToPlayer = Mathf.Sign(Controller.player.GlobalPosition.X - Controller.GlobalPosition.X);
        
        float targetSpeed = Controller.runspeed * directionToPlayer;
        
        if (Mathf.Sign(targetSpeed) != Mathf.Sign(Controller.speed))
        {
            Controller.mesh.RotateY(Mathf.Pi);
        }
        
        Controller.speed = targetSpeed;
        Controller.Velocity = new Godot.Vector3(Controller.speed, Controller.gravity, 0);
    }




}