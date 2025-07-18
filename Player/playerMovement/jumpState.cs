using Godot;
using System;

public partial class jumpState : State
{
    [ExportSubgroup("Jump State")]
	[Export] public bool jumping;
	[Export] public float jumpInitSpeed = 1.0f;
	[Export] public float jumpAcceleration = 10.0f;
	[Export] public float jumpMaxSpeed = 5.0f;
 	 [Export] public float jumpVelocity = 10.0f;
	[Export] public float jumpGravity = 9.8f;
	[Export] public float jumpMaxHeight = 0.17f;
	 public float jumpHeight = 0.0f;

    private bool isAscending(double delta, ref Vector3 velocity) //check if player should be ascending
    {
        if (jumpHeight < jumpMaxHeight)
        {
            velocity.Y = jumpVelocity;
            jumpHeight += (float)delta;
            GD.Print("Jump Height: " + jumpHeight);
            return true;
        }
        else
        {
            GD.Print("jump max height reached");
            jumpHeight = jumpMaxHeight;
            return false;
        }
    }

    public override void Enter()
    {
        GD.Print("Entered Jump State");
        //jump animation
        jumpHeight = 0.0f;
        if (jumpQueued)
        {
            /*
            Vector3 velocity = player.Velocity;
            velocity.Y = jumpVelocity;
            player.Velocity = velocity;
            player.MoveAndSlide();*/
        }
    }
    public override void PhysicsUpdate(float delta)
    {
        Vector3 velocity = player.Velocity;
        if (velocity.X > jumpMaxSpeed) velocity.X = jumpMaxSpeed;
        if (velocity.X < -jumpMaxSpeed) velocity.X = -jumpMaxSpeed;
        //playerMesh.RotationDegrees = new Vector3(0, 0, 0);
        velocity.Y -= jumpGravity * (float)delta;

        /*if (Input.IsKeyPressed(Key.Down)) {
				velocity.Y -= (jumpGravity * 4) * (float)delta;
				
			}
		*/
        GD.Print(jumpQueued);
        if (jumpQueued && isAscending(delta, ref velocity))
        { //jump queued set true outside this state. if the player releases jump, the bool is set false 
            GD.Print("Jumping");
            if (Input.IsKeyPressed(Key.Left))
            {
                velocity.X = -jumpMaxSpeed;
            }
            else if (Input.IsKeyPressed(Key.Right))
            {
                velocity.X = jumpMaxSpeed;
            }
            else
            {
                if (velocity.X > 0.3f) velocity.X -= 1.0f;
                else if (velocity.X < -0.3f) velocity.X += 1.0f;
                else { velocity.X = 0; }
            }
        }
        else //must be falling
        {
            GD.Print("Falling");
            slideBoost = true;
            velocity.Y -= jumpGravity * 2.5f * (float)delta;
            if (Input.IsKeyPressed(Key.Left))
            {
                velocity.X = -jumpMaxSpeed;
            }
            else if (Input.IsKeyPressed(Key.Right))
            {
                velocity.X = jumpMaxSpeed;
            }
            else
            {
                if (velocity.X > 0.1f) velocity.X = 0.5f;
                else if (velocity.X < -0.1f) velocity.X = -0.5f;

            }
            if (player.IsOnFloor())
            {
                //msm.TransitionTo("groundedState");
            }
        }
        player.Velocity = velocity;
        player.MoveAndSlide();
	}
    
    public override void HandleInput(InputEvent @event)
    {
        if (@event.IsActionReleased("Jump"))
        {
            jumpQueued = false;
        }
		if (@event.IsActionPressed("Slide") && player.IsOnFloor())
        {
            msm.TransitionTo("slideState");
        }
	}
    
}
