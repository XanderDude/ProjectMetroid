using Godot;
using System;
using System.Threading.Tasks;

public partial class aggroState : State
{

    private enum SubState {APPROACH, ATTACK, RECOVER}


    private SubState substate = SubState.APPROACH;

    private int rand;

    private float randX, randY;

    public override void Enter()
    {       
            ec.Velocity = Vector3.Zero;
            GD.Print($"{ec.Name}:aggroState");
            ec.timer = ec.attackcd;
            substate = SubState.APPROACH;
            rand = GD.RandRange(0, 3);
            randX = GD.RandRange(-2,2);
            randY = GD.RandRange(0,3);
            
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
            if (ec.isFlying) ec.MoveToPlayer(ec.runspeed, ec.acceleration, randY, randX);
            else ec.MoveToPlayer(ec.runspeed, ec.acceleration, 0, 0);
            if (ec.isPlayerInRange(ec.attackrange) && ec.timer <= 0) 
            {
                substate = SubState.ATTACK;
                ec.direction = ec.player.GlobalPosition - ec.GlobalPosition; //idk what this does
                ec.FaceDirection(ec.direction.X);
                ec.timer = 1.5f;
            }
        }
        
        if (this.substate == SubState.ATTACK)
        {
            ec.timer -= delta;
            if (ec.timer <= 0) 
            {
                if (rand > 0 && !ec.isFlying)
                {
                    ec.Velocity = Vector3.Zero;
                    ec.Attack(2, 2, ec.damagedealt, 0.2f);
                    ec.timer = 5f;
                    substate = SubState.RECOVER;
                }
                else if (rand == 0 || ec.isFlying)
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
                    esm.TransitionTo("patrolState");
            }
        }
        

    }

}