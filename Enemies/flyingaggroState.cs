using Godot;
using System;

public partial class flyingaggroState : EnemyState
{
    private float outOfRangeTimer = 0f;
	private float outOfRangeDelay = 2f;
    
    public override void Enter()
    {
        GD.Print("Entered Flying Aggro State");
        ec.IDLE = ec.runspeed;
        outOfRangeTimer = 0f;
    }
    
    public override void Exit()
    {
        outOfRangeTimer = 0f;
    }
    
    public override void Update(float delta)
    {
        if (!ec.isPlayerInRange(ec.detectionrange))
        {
            outOfRangeTimer += delta;
            
            if (outOfRangeTimer >= outOfRangeDelay)
            {
                EmitSignal(SignalName.Transition, "patrolState");
            }
        }
        else
        {
            outOfRangeTimer = 0f;
        }
    }
    
    public override void PhysicsUpdate(float delta)
    {
        if (ec.player != null)
        {
            Vector3 directionToPlayer = (ec.player.GlobalPosition - ec.GlobalPosition).Normalized();
            
            ec.Velocity = directionToPlayer * ec.IDLE;
        }
    }
}