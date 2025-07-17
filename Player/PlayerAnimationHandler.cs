using Godot;
using System;

public partial class PlayerAnimationHandler : Node3D //goes on the playerMesh
{
    [Export] private CharacterBody3D player;
    [Export] private AnimationTree animTree;
    [Export] private string playbackFilePath; //ref to where we are in the animation state machine
    private AnimationNodeStateMachinePlayback playback;
    [Export] private string WalkingBlendPath {get; set;}
    [Export] private float transitionSpeed = 8f;
    [Export] private string JumpStateName;
    [Export] private string RunningStateName;
    [Export] private string SlideStateName;
    private float currentSpeed;
    private int currentDirection = 90; //-90 for left, 90 for right
    private Vector2 aimAngle; //angle to position shooting arm during aiming mode

    public override void _Ready()
    {
        base._Ready();
        //player = GetNode<Node3D>("%Player");
        //currentDirection = (int)playerMesh.Rotation.Y;
        playback = (AnimationNodeStateMachinePlayback)animTree.Get(playbackFilePath);
    }

    public override void _Process(double delta)
    {
        if (player == null) { GD.Print("No player node assigned"); return; } //dont calculate if playe hasn't been assigned 

        float newDelta = Mathf.Abs(player.Velocity.X) - currentSpeed; //find the value between current speed and desired speed
        if (newDelta > transitionSpeed * delta) //clamp new speed if it's greater than transition speed
            newDelta = transitionSpeed * (float)delta;

        currentSpeed += newDelta;
        if (player.Velocity.X == 0)
        {
            //currentSpeed = 0;
        }
        else
        {
            Rotation = new Vector3(0, currentDirection * Mathf.Sign(player.Velocity.X), 0);
        }
        animTree.Set(WalkingBlendPath, currentSpeed); //always blend animation tree with current speed
        if ((bool)player.Get("jumping")) BeginJump(); //run jump function
        if ((bool)player.Get("sliding")) BeginSlide();
        else animTree.Set("parameters/conditions/slideEnd", true);
    }

    private void BeginJump()
    {
        if (player.IsOnFloor()) playback.Travel(RunningStateName);
        else
        {
            playback.Travel(JumpStateName);
        }
    }

    private void BeginSlide()
    {
        if (!player.IsOnFloor()) playback.Travel(JumpStateName);
        else
        {
            animTree.Set("parameters/conditions/slideEnd", false);
            playback.Travel(SlideStateName);
        }
    }


    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent is InputEventJoypadMotion stickMotionEvent) //left stick motion during aiming mode
        {
            //get aim direction x and y

            //aimAngle = new Vector2(newX, newY);
        }
        if (inputEvent is InputEventMouseMotion mouseMotionEvent) //mouse motion during aiming mode
        {
            Vector2 delta = mouseMotionEvent.Relative;
            aimAngle = delta;

            //get aim direction x and y
            //float newX
            //float newY
            //aimAngle = new Vector2(newX, newY);

            //remember to toggle mouse visiblity
        }
    }

}
