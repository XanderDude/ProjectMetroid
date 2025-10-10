using Godot;
using System;
using System.Numerics;

public partial class patrolState : State
{

   
    public override void Enter()
    {

        GD.Print($"{Controller.Name} entered patrol State");
        Controller.speed = Controller.walkspeed;
            


        
        
    }

    public override void Exit()
    {
    }

    public override void Update(float delta)
    {
        if (Controller.isPlayerInAggroBounds())
        {
            Controller.statemachine.TransitionTo("aggroState");
        }
        
    }
    public override void PhysicsUpdate(float delta)
    {
        Controller.timer2 += delta;
        Controller.timer += delta;
        
        if (Controller.isLeavingPatrolBounds() && Controller.timer >= 0.5f)
        {
            Controller.mesh.RotateY(Mathf.Pi);
            Controller.speed = -Controller.speed;
            Controller.timer = 0;
        }
        
    
        if (Controller.timer2 >= 8.0f)
        {
            var num = Controller.GetRandom1234();
            GD.Print($"Random behavior: {num}");
            
            float currentDirection = Mathf.Sign(Controller.speed);
            
            if (num == 1 || num == 2)
            {
                
                Controller.speed = Controller.walkspeed * currentDirection;
            }
            else if (num == 3)
            { 
                Controller.speed = Controller.idle * currentDirection;
            }
            else if (num == 4)
            {
                Controller.mesh.RotateY(Mathf.Pi);
                Controller.speed = -(Controller.idle * currentDirection);
            }
            
            Controller.timer2 = 0;
        }
        
        Controller.Velocity = new Godot.Vector3(Controller.speed, Controller.gravity, 0);
    }


}