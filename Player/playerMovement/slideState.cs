using Godot;
using System;

public partial class slideState : State
{
    //[Export] public float slideTime = 1.0f; //max time the player can stay in a slide before standing up/crouching
    [Export] public float slideMinTime = .2f; //min time the player has to stay in the slide state before standing up (can still jump)
   	[Export] public float slideMaxSpeed = 12.0f;
	//[Export] public bool slideEnter = false;
    [Export] public float groundInitSpeed = 1.0f;
    [Export] public float groundMaxSpeed = 6.0f;
    [Export] public float groundAcceleration = 15.0f;
    [Export] public float groundDeacceleration = 15.0f;

    private float slideTimer; //time in current slide
    public override void Enter()
    {
        GD.Print("Entered Slide State");
        slideTimer = 0;
        playerMesh.GetNode<PlayerAnimationHandler>(playerMesh.GetPath()).BeginSlide();
        //playerMesh.RotationDegrees = new Vector3(0, 0, 0);
    }

    public override void Exit()
    {
        GD.Print("Exited Slide State");
        playerMesh.GetNode<PlayerAnimationHandler>(playerMesh.GetPath()).EndSlide();
    }
    public override void PhysicsUpdate(double delta)
    {
        slideTimer += (float)delta;

        if (!player.IsOnFloor()) //immediately switch to jump state
        {
            msm.TransitionTo("jumpState");
        }
        if (!msm.slideQueued && slideTimer >= slideMinTime)
        {
            msm.TransitionTo("groundedState");
        }
        HandleSlidingMovement(delta);
        //CheckTransitions();
        player.MoveAndSlide();
    }

    private void HandleSlidingMovement(double delta)
    {
        Vector3 velocity = player.Velocity;
        if (velocity.X > slideMaxSpeed) velocity.X = slideMaxSpeed;
        if (velocity.X < -slideMaxSpeed) velocity.X = -slideMaxSpeed;
        //playerMesh.RotationDegrees = new Vector3(0, 0, 90);
        if (msm.slideBoost)
        {
            if (velocity.X > 0) velocity.X = slideMaxSpeed;
            else if (velocity.X < 0) { velocity.X = -slideMaxSpeed; }
            else { velocity.X = 0; }
            msm.slideBoost = false;
        }
        if (velocity.X > 0.1f) velocity.X -= groundDeacceleration * (float)delta;
        else if (velocity.X < -0.1f) velocity.X += groundDeacceleration * (float)delta;
        else { velocity.X = 0; }

        player.Velocity = velocity;
    }

    public override void HandleInput(InputEvent @event)
    {
        if (@event.IsActionPressed("Jump") && player.IsOnFloor()) //allow jumping out of slide
        {
            msm.jumpQueued = true;
            msm.TransitionTo("jumpState");
        }
        if (@event.IsActionPressed("Slide")) //is slide being pressed
        {
            msm.slideQueued = true;
            //slide out of slide?
        }
        else
        {
            msm.slideQueued = false;
        }
    }

}
