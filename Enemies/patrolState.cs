using Godot;
using System;

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
        


    }

    public override void PhysicsUpdate(float delta)
    {
        
       
    }


}