using Godot;
using System;
using System.Numerics;

public partial class patrolState : State
{

    public float speed;
    public override void Enter()
    {

        GD.Print($"{Controller.Name} entered patrol State");
        Controller.Velocity = new Godot.Vector3(Controller.walkspeed, Controller.gravity, 0);
        speed = Controller.walkspeed;
        
        
    }

    public override void Exit()
    {
    }

    public override void Update(float delta)
    {
        
    }
    public override void PhysicsUpdate(float delta)
    {
        Controller.timer2 += delta;
        Controller.timer += delta;

        if (Controller.isLeavingPatrolBounds() && Controller.timer >= 0.5f)
        {
            Controller.mesh.RotateY(3.14159f);
            speed = -speed;
            Controller.timer = 0;

        }


        if (Controller.timer2 >= 8.0f)
        {
            var num = Controller.GetRandom1234();
            GD.Print($"{num}");
            if ( num == 1 || num == 2)
            {
                speed = Controller.walkspeed * Mathf.Sign(speed);
            }
            else if (num == 3)
            {
                speed = Controller.idle * Mathf.Sign(speed);
            }
            else if (num == 4)
            {
                speed = Controller.idle * Mathf.Sign(speed);
                Controller.mesh.RotateY(3.14159f);
                speed = -speed;
            }
    
            Controller.timer2 = 0;
        }

        Controller.Velocity = new Godot.Vector3(speed, Controller.gravity, 0);




    }


}