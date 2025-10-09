using Godot;
using System;
using System.Numerics;

public partial class patrolState : State
{
    public override void Enter()
    {
        
        GD.Print($"{Controller.Name} entered patrol State");
        
        
    }

    public override void Exit()
    {
    }

    public override void Update(float delta)
    {
        
        Controller.timer2 += delta;
        Controller.timer += delta;
    }

    public override void PhysicsUpdate(float delta)
    {
        if (Controller.isAtMeshEdge() && Controller.timer >= 2.0f)
        {
            Controller.RotationDegrees = new Godot.Vector3(0, Controller.RotationDegrees.Y + 180, 0);
            Controller.timer = 0;
            Controller.movespeed = -Controller.movespeed;


        }
            
        Controller.Velocity = new Godot.Vector3(Controller.movespeed, Controller.gravity, 0);

        

           

    }


}