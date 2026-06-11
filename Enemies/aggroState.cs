using Godot;
using System;
using System.Threading.Tasks;

public partial class aggroState : State
{

    private enum SubState {APPROACH, ATTACK, RECOVER}


    private SubState substate = SubState.APPROACH;



    public override void Enter()
    {       
            ec.timer = 0f;
            GD.Print($"{ec.Name}:aggroState");
            ec.Velocity = ec.runspeedVector;
            
    }

    public override void Exit()
    {
        GD.Print ($" Rat {ec.Name} is no longer aggroState");
    }

    public override void Update(float delta)
    {

    }

    public override async void PhysicsUpdate(float delta)
    {
        if (this.substate == SubState.APPROACH)
        {   
            ec.timer -= delta;
            ec.MoveToPlayer(ec.runspeed, ec.acceleration);
            if (ec.isPlayerInRange(ec.attackrange) && ec.timer <= 0) 
            {
                ec.timer = 2f;
                substate = SubState.ATTACK;
                ec.direction = ec.player.GlobalPosition - ec.GlobalPosition;   // ← this line
                ec.FaceDirection(ec.direction.X);
            }
        }
        
        if (this.substate == SubState.ATTACK)
        {
            ec.Velocity = Vector3.Zero;
            ec.timer -= delta;
            ec.justattacked = true;
            if (ec.timer <= 0) 
            {
                ec.Attack(2, 2, 100, 2);
                ec.timer = 5f;
                substate = SubState.RECOVER;
            }
        }
        if (this.substate == SubState.RECOVER)
        {

            ec.timer -= delta;
            if (ec.timer <= 0f)
            {
                this.substate = SubState.APPROACH;
                if (ec.justattacked) ec.timer = 8f;
                if (!ec.isPlayerInRange(ec.detectionrange))
                    esm.TransitionTo("patrolState");
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