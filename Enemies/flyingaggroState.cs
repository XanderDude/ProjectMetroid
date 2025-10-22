using Godot;
using System;

public partial class flyingaggroState : State
{
    private float outOfRangeTimer = 0f;
	private float outOfRangeDelay = 2f;
    
    public override void Enter()
    {
        GD.Print("Entered Flying Aggro State");
        Controller.speed = Controller.runspeed;
        outOfRangeTimer = 0f;
    }
    
    public override void Exit()
    {
        outOfRangeTimer = 0f;
    }
    
    public override void Update(float delta)
    {
        if (!Controller.isPlayerInRange(Controller.detectionrange))
        {
            outOfRangeTimer += delta;
            
            if (outOfRangeTimer >= outOfRangeDelay)
            {
                StateMachine.TransitionTo("patrolState");
            }
        }
        else
        {
            outOfRangeTimer = 0f;
        }
    }
    
    public override void PhysicsUpdate(float delta)
    {
        if (Controller.player != null)
        {
            Vector3 directionToPlayer = (Controller.player.GlobalPosition - Controller.GlobalPosition).Normalized();
            
            Controller.Velocity = directionToPlayer * Controller.speed;
        }
    }
}