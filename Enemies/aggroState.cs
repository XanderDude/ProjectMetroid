using Godot;
using System;
using System.Threading.Tasks;

public partial class aggroState : State
{

    private enum SubState {APPROACH, ATTACK, RECOVER}


    private SubState substate = SubState.APPROACH;

    private int rand;

    public override void Enter()
    {       

            GD.Print($"{ec.Name}:aggroState");
            substate = SubState.APPROACH;
            rand = GD.RandRange(0, 3);
            ec.timer = 5f;
        
            
    }

    public override void Exit()
    {
        GD.Print ($" Rat {ec.Name} is no longer aggroState");
    }

    public override void Update(float delta)
    {
        if (Mathf.Abs(ec.Velocity.X) > 0.5f)
				ec.FaceDirection(ec.Velocity.X);
    }

    public override async void PhysicsUpdate(float delta)
    {


        if (this.substate == SubState.APPROACH)
        {   
            ec.timer -= delta;
            ec.MoveToPlayer(ec.runspeed, ec.acceleration);
            if (ec.isPlayerInRange(ec.attackrange) && ec.timer <= 0) 
            {
                substate = SubState.ATTACK;
                ec.direction = ec.player.GlobalPosition - ec.GlobalPosition;   // ← this line
                ec.FaceDirection(ec.direction.X);
                ec.timer = 1.5f;
            }
        }
        
        if (this.substate == SubState.ATTACK)
        {
            ec.timer -= delta;
            if (ec.timer <= 0) 
            {
                if (rand > 0)
                {
                    ec.Velocity = Vector3.Zero;
                    ec.Attack(2, 2, 100, 0.2f);
                    ec.timer = 5f;
                    substate = SubState.RECOVER;
                }
                else
                { 
                    ec.esm.TransitionTo("lungeState");
                }
            }
        }
        if (this.substate == SubState.RECOVER)
        {
            ec.timer -= delta;
            if (ec.timer <= 0f)
            {
                if (!ec.isPlayerInRange(ec.detectionrange))
                    esm.TransitionTo("patrolState");
                else
                {
                    rand = GD.RandRange(0, 3);
                    ec.timer = 5f;
                    substate = SubState.APPROACH;
                }
            }
        }
        

    }

    private void MoveToPlayer()
    {
        if (ec.runspeed < ec.maxspeed) ec.runspeed += ec.acceleration;
        float dir = Mathf.Sign(ec.player.GlobalPosition.X - ec.GlobalPosition.X);
        ec.Velocity = new Vector3(dir * ec.runspeed, ec.Velocity.Y, 0);
    }

    public override void HandleInput(InputEvent @event)
    {
    }
}