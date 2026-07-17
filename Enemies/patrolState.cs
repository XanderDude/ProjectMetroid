using Godot;
using System;
using System.Text.RegularExpressions;
public partial class patrolState : EnemyState
{
  
    private enum SubState { IDLE, WAIT_TO_MOVE, MOVEMENT }; 
    private SubState substate = SubState.IDLE;
    //Timers
    private float idle_timer_count = 0, idle_wait_time = 5.0f;

    private Vector3 flyTargetPosition;

    public override void Enter()
    {
        //GD.Print("Enemy " + ec.Name + " entered patrol state");
        ec.Velocity = ec.walkspeedVector * (float)GD.RandRange(0.5f, ec.walkspeedVector.X);
        ec.timer = 1.0f;
    }
   
    public override void Exit()
    {
    }
    public override void Update(float delta)
    {
        if (Mathf.Abs(ec.Velocity.X) > 0.5f)
                ec.FaceDirection(ec.Velocity.X);
    }
   
    public override void PhysicsUpdate(float delta)
    {
        ec.timer -= delta;
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
                    if (ec.isFlying)
                        _on_movement_flying();
                    else if (ec.movementmode == Enemy.MovementMode.Normal)
                        _on_movement();
                    else if (ec.movementmode == Enemy.MovementMode.Hopping && ec.IsOnFloor() && ec.timer <= 0)
                    {
                        _on_movement_hopping();
                        ec.timer = 2.0f;
                    }
                    else if (ec.timer > 0 && ec.IsOnFloor())
                    {
                        ec.Velocity = Vector3.Zero;
                    }
                    
                    break; 
                default:
                    break;
            }
        
            if (!ec.isFlying)
                ec.Velocity += ec.GravityVector * delta;
            if (ec.isPlayerInRange(ec.detectionrange) && ec.movementmode != Enemy.MovementMode.Hopping)
            {
                GD.Print("in detection range");
                ec.Velocity = Vector3.Zero;
                EmitSignal(SignalName.Transition, "aggroState");
            }
           
        }
        
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
            if (ec.isFlying)
            {
                flyTargetPosition = get_new_target_location_flying();
            }
            else
            {
                var target = get_new_target_location();
                ec.navagent.TargetPosition = target;
            }
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
    private void _on_movement_hopping()
    {
        var direction = 1;
        if (ec.IsOnWall()) direction = -1;
        ec.Velocity = new Vector3(ec.walkspeed * direction, ec.runspeed, 0);
        if (!ec.IsOnFloor() && ec.timer <= 0) 
        {
            ec.Velocity += new Vector3(0, ec.gravity,0);
        }
        GD.Print($"{ec.Velocity}");
    }
    private void _on_movement_flying()
    {
        var current_position = ec.GlobalTransform.Origin;
        var to_target = flyTargetPosition - current_position;
        to_target.Z = 0f;

        if (to_target.Length() < 0.3f)
        {
            substate = SubState.IDLE;
            return;
        }

        ec.Velocity = to_target.Normalized() * ec.walkspeed;
    }
    private Vector3 get_new_target_location()
    {
        float range = Random.Shared.NextSingle() * 4.0f + 2.0f; 
        float sign = Random.Shared.NextSingle() < 0.5f ? -1 : 1;
        var offset_x = range * sign;
        return ec.GlobalTransform.Origin + new Vector3(offset_x, 0, 0);
    }
    private Vector3 get_new_target_location_flying()
    {
        float range = Random.Shared.NextSingle() * 4.0f + 2.0f;
        float angle = Random.Shared.NextSingle() * Mathf.Tau;
        var offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * range;
        return ec.GlobalTransform.Origin + offset;
    }
    public void _on_navigation_agent_3d_navigation_finished()
    {
        substate = SubState.IDLE;
    }
    public override void _PhysicsProcess(double delta)
    {
        
    }
}