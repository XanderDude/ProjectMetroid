using Godot;
using System;

public partial class slideState : State
{
    //[Export] public float slideTime = 1.0f; //max time the player can stay in a slide before standing up/crouching
    [Export] public float slideMinTime = .2f; //min time the player has to stay in the slide state before standing up (can still jump)
   	[Export] public float slideMaxSpeed = 12.0f;
    [Export] public float boostMaxSpeed = 6.0f;
    [Export] public float slideAcceleration = 15.0f;
    [Export] public float slideDeceleration = 15.0f;
    private float currentSlideSpeed;
    private float input;
    private float slideTimer; //time in current slide
    public override void Enter()
    {
        input = Mathf.Sign(Input.GetAxis("Left", "Right"));
        GD.Print("Entered Slide State. Facing " + input);
        slideTimer = 0;
        if ((bool)player.Get(PlayerManager.PropertyName.slideBoost))
        {
            GD.Print("Boosting");
            currentSlideSpeed = boostMaxSpeed * input;
        }
        else
        {
            currentSlideSpeed = slideMaxSpeed * input;
        }
        parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).BeginSlide();
    }

    public override void Exit()
    {
        player.Set(PlayerManager.PropertyName.slideQueued, false);
        player.Set(PlayerManager.PropertyName.slideBoost, false);
        GD.Print("Exited Slide State");
        parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).EndSlide();
    }
    public override void PhysicsUpdate(float delta)
    {
        slideTimer += delta;

        if (!player.IsOnFloor()) //immediately switch to jump state
        {
            msm.TransitionTo("jumpState");
        }
        if (!Input.IsActionPressed("Slide") && slideTimer >= slideMinTime)
        {
            msm.TransitionTo("groundedState"); //switch to grounded if slide is released
        }
        HandleSlidingMovement(delta);
        player.MoveAndSlide();
    }

    private void HandleSlidingMovement(float delta)
    {
        Vector3 velocity = player.Velocity;

        if ((bool)player.Get(PlayerManager.PropertyName.slideBoost))
        {
            currentSlideSpeed = Mathf.MoveToward(currentSlideSpeed, 0, delta * slideDeceleration);
        }
        else //normal slide
        {
            currentSlideSpeed = Mathf.MoveToward(currentSlideSpeed, 0, delta * slideDeceleration);
        }

        //velocity.X = Mathf.MoveToward(slideMaxSpeed, 0, delta + slideDeceleration);
        velocity.X = currentSlideSpeed;
        player.Velocity = velocity; 
    }

    public override void HandleInput(InputEvent @event)
    {
        if (@event.IsActionPressed("Jump") && player.IsOnFloor()) //allow jumping out of slide
        {
            player.Set("jumpQueued", true);
            msm.TransitionTo("jumpState");
        }
        /*
        if (@event.IsActionPressed("Slide")) //is slide being pressed
        {
            msm.slideQueued = true;
            //slide out of slide?
        }
        else
        {
            msm.slideQueued = false;
        }*/
    }

}
