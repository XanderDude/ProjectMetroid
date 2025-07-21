using Godot;
using System;

public partial class jumpState : State
{
    [ExportSubgroup("Jump State")]
    [Export] public float airMaxSpeed = 5.0f;
	[Export] public float jumpAcceleration = 10.0f;
    [Export] public float jumpDeceleration = 15f;
 	[Export] public float jumpVelocity = 10.0f;
	[Export] public float jumpMaxHeight = 0.17f;
	public float jumpHeight = 0.0f;
    private bool neutralJump = false;
    private bool IsAscending(float delta, ref Vector3 velocity) //check if player should be ascending
    {
        if (jumpHeight < jumpMaxHeight)
        {
            velocity.Y = jumpVelocity; //continously set upward velocity
            jumpHeight += delta;
            //GD.Print("Jump Height: " + jumpHeight);
            return true;
        }
        else
        {
            GD.Print("jump max height reached");
            jumpHeight = jumpMaxHeight; //clamp
            return false;
        }
    }

    public override void Enter()
    {
        GD.Print("Entered Jump State. Jump queued: " + player.Get("jumpQueued"));
        if ((bool)player.Get("jumpQueued"))//jump state entered due to player jumping
        {
            jumpHeight = 0.0f;
            //play jump sound
            if (Input.GetAxis("Left", "Right") == 0) neutralJump = true; //freeze horizontal velocity
        }

        player.Set(PlayerManager.PropertyName.slideBoost, true); //player must be airborne, enable boost
        parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).BeginJump();
    }
    public override void Exit()
    {
        GD.Print("Exited Jump State");
        player.Set("jumpQueued", false); //don't jump on exit if holding jump
        parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Grounded();
    }

    public override void PhysicsUpdate(float delta)
    {
        /*if (Input.IsKeyPressed(Key.Down)) {
				velocity.Y -= (jumpGravity * 4) * (float)delta;
			}
		*/
        HandleAirMovement(delta);
        player.MoveAndSlide();
    }

    private void HandleAirMovement(float delta)
    {
        Vector3 velocity = player.Velocity;
        if (neutralJump) velocity.X = 0; neutralJump = false;
        float input = Input.GetAxis("Left", "Right");

        if ((bool)player.Get("jumpQueued") && IsAscending(delta, ref velocity))
        { //jump queued set true outside this state. if the player releases jump, the bool is set false 
            GD.Print("Jumping");
            velocity.Y -= _gravity * delta;
            if (input == 0)
            {
                velocity.X = Mathf.MoveToward(velocity.X, 0, jumpDeceleration);
                /*
                if (velocity.X > 0.3f) velocity.X -= 1.0f;
                else if (velocity.X < -0.3f) velocity.X += 1.0f;
                else { velocity.X = 0; }*/
            }
            else velocity.X = input * airMaxSpeed;

        }
        else //must be falling
        {
            velocity.Y -= _gravity * 2.5f * delta; //faster falling speed
            if (input == 0)
            {
                velocity.X = Mathf.MoveToward(velocity.X, 0, jumpDeceleration);
                /*
                if (velocity.X > 0.1f) velocity.X = 0.5f;
                else if (velocity.X < -0.1f) velocity.X = -0.5f;
                */
            }
            else velocity.X = input * airMaxSpeed;
            if (player.IsOnFloor())
            {
                if (Input.IsActionPressed("Slide")) msm.TransitionTo("slideState");
                else msm.TransitionTo("groundedState");
            }
        }

        velocity.X = Mathf.Clamp(velocity.X, -airMaxSpeed, airMaxSpeed);//clamp horizontal speed
        player.Velocity = velocity;
    }

    public override void HandleInput(InputEvent @event)
    {
        if (@event.IsActionReleased("Jump")) //check when jump is released
        {
            player.Set(PlayerManager.PropertyName.jumpQueued, false);
        }
	}
    
}
