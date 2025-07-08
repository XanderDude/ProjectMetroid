using Godot;
using System;

public partial class Player : CharacterBody3D 
{
    [Export] public float initSpeed = 1.0f;
    [Export] public float acceleration = 2.0f;
    [Export] public float maxSpeed = 4.0f;
    [Export] public float jumpVelocity = 10.5f;
    [Export] public float gravity = 9.8f;
    [Export] public float deacceleration = 2.0f;
    
    
    public override void _PhysicsProcess(double delta) 
    {
        Vector3 velocity = Velocity;
        
        if (!IsOnFloor()) velocity.Y -= gravity * (float)delta;
        if (!IsOnFloor() && velocity.Y < 6.5f) velocity.Y -= gravity * (float)delta * 5;
        if (Input.IsKeyPressed(Key.Left)) {
            if (velocity.X > -maxSpeed) velocity.X -= (initSpeed + acceleration) * (float)delta; 
        }
         if (Input.IsKeyPressed(Key.Space) && IsOnFloor()) {
            velocity.Y = jumpVelocity;
        }
        if (Input.IsKeyPressed(Key.Right)) {
            if (velocity.X < maxSpeed) velocity.X += (initSpeed + acceleration) * (float)delta; 
        }
        
        
        else if (velocity.X < 0 ) velocity.X += deacceleration;
        else if (velocity.X > 0) velocity.X -= deacceleration;
        
        
        
        
        
       
        
        
        
        
        Velocity = velocity;
        MoveAndSlide();
    }
}
