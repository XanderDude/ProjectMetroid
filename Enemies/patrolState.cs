using Godot;
using System;
public partial class patrolState : State
{
    private float patrolTimer = 0f;
    private float patrolDuration = 0f;
    private Godot.Vector3 patrolDirection = Godot.Vector3.Zero;
    private float bobTimer = 0f;
    private float bobSpeed = 2.0f;
    private float bobAmount = 0.3f;
   
    public override void Enter()
    {
        //GD.Print("Entered Patrol State");
        ec.Velocity = new Vector3(ec.maxWalkSpeed, 0, 0);
        SetNewPatrolDirection();
    }
   
    public override void Exit()
    {
        patrolTimer = 0f;
    }
   
    public override void Update(float delta)
    {
        if (ec.isPlayerInRange(ec.detectionrange) && !ec.isFlying)
        {
            esm.TransitionTo("aggroState");
        }
        else if (ec.isPlayerInRange(ec.detectionrange) && ec.isFlying)
        {
            esm.TransitionTo("flyingaggroState");
        }
       
        patrolTimer += delta;
       
        if (patrolTimer >= patrolDuration)
        {
            SetNewPatrolDirection();
        }
    }
   
    public override void PhysicsUpdate(float delta)
    {
        ec.direction = patrolDirection;
        
        float verticalVelocity;
        
        if (ec.isFlying)
        {
            bobTimer += delta;
            float bobOffset = Mathf.Sin(bobTimer * bobSpeed) * bobAmount;
            verticalVelocity = patrolDirection.Y * new Vector3(0, ec.maxWalkSpeed, 0).Y + bobOffset;
        }
        else
        {
            verticalVelocity = ec.Velocity.Y + ec.gravity * delta;
        }
       
        ec.Velocity = new Godot.Vector3(
            patrolDirection.X * new Vector3(ec.maxWalkSpeed, 0, 0).X,
            verticalVelocity,
            0
        );
    }
   
    private void SetNewPatrolDirection()
    {
        patrolTimer = 0f;
        patrolDuration = ec.GetRandomNumber();
        
        if (ec.isFlying)
        {
            int randomChoice = GD.RandRange(0, 8);
            
            switch (randomChoice)
            {
                case 0:
                    patrolDirection = Godot.Vector3.Left;
                    break;
                case 1:
                    patrolDirection = Godot.Vector3.Right;
                    break;
                case 2:
                    patrolDirection = Godot.Vector3.Up;
                    break;
                case 3:
                    patrolDirection = Godot.Vector3.Down;
                    break;
                case 4:
                    patrolDirection = (Godot.Vector3.Left + Godot.Vector3.Up).Normalized();
                    break;
                case 5:
                    patrolDirection = (Godot.Vector3.Right + Godot.Vector3.Up).Normalized();
                    break;
                case 6:
                    patrolDirection = (Godot.Vector3.Left + Godot.Vector3.Down).Normalized();
                    break;
                case 7:
                    patrolDirection = (Godot.Vector3.Right + Godot.Vector3.Down).Normalized();
                    break;
                case 8:
                    patrolDirection = Godot.Vector3.Zero;
                    break;
            }
        }
        else
        {
            int randomChoice = GD.RandRange(0, 2);
            
            switch (randomChoice)
            {
                case 0:
                    patrolDirection = Godot.Vector3.Left;
                    break;
                case 1:
                    patrolDirection = Godot.Vector3.Right;
                    break;
                case 2:
                    patrolDirection = Godot.Vector3.Zero;
                    break;
            }
        }
       
        //GD.Print($"New patrol direction: {patrolDirection}, duration: {patrolDuration}");
    }
}