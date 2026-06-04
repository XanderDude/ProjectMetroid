using Godot;
using System;
using System.Text.RegularExpressions;
public partial class patrolState : State
{
  
    private enum SubState { IDLE, WAIT_TO_MOVE, MOVEMENT }; 
    private SubState substate = SubState.IDLE;

    //Timers
    private float idle_timer_count = 0, idle_wait_time = 5.0f;




    public override void Enter()
    {
        //GD.Print("Enemy " + ec.Name + " entered patrol state");
        ec.Velocity = ec.walkspeedVector * (float)GD.RandRange(0.5f, ec.walkspeedVector.X);;
    }
   
    public override void Exit()
    {
        
    }
   
    public override void PhysicsUpdate(float delta)
    {
        switch (substate)
        {
            case SubState.IDLE: 
                _on_idle();
                break;
            case SubState.WAIT_TO_MOVE:
                _on_wait_to_move(delta);
                break;
            case SubState.MOVEMENT: 
                _on_movement();
                break; 
            default:
                break;
        }
    
        ec.Velocity += ec.GravityVector * delta;
        
    }

    private void _on_idle()
    {
        //GD.Print("Enemy " + ec.Name + " is idling");
        ec.Velocity = Vector3.Zero;
        idle_timer_count = idle_wait_time;
        substate = SubState.WAIT_TO_MOVE;
    }
    private void _on_wait_to_move(float delta)
    {
        //GD.Print("Enemy " + ec.Name + " is waiting to move");
        idle_timer_count -= delta;
        if (idle_timer_count <= 0.0f)
        {
            var target = get_new_target_location();
            ec.navagent.TargetPosition = target;
            substate = SubState.MOVEMENT;

        }
    }
    private void _on_movement()
    {
        //GD.Print("Enemy " + ec.Name + " is moving");
        var current_position = ec.GlobalTransform.Origin;
        var next_position = ec.navagent.GetNextPathPosition();
        var direction = (next_position - current_position).Normalized(); 
        ec.Velocity = direction * ec.walkspeed;   
    }

    private Vector3 get_new_target_location()
    {
        float range = Random.Shared.NextSingle() * 4.0f + 2.0f; 
        float sign = Random.Shared.NextSingle() < 0.5f ? -1 : 1;
        var offset_x = range * sign;
        return ec.GlobalTransform.Origin + new Vector3(offset_x, 0, 0);
    }

    public void _on_navigation_agent_3d_navigation_finished()
    {
        substate = SubState.IDLE;
    }
}