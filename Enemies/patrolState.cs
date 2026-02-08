using Godot;
using System;
public partial class patrolState : State
{
   enum SubStates { IDLE, WAIT_TO_MOVE, MOVEMENT }; 
    public override void Enter()
    {
        GD.Print("Enemy " + ec.Name + " entered patrol state");
        ec.Velocity = ec.walkspeedVector;
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