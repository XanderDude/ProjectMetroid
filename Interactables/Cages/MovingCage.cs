using Godot;
using System;

public partial class MovingCage : AnimatableBody3D
{
	[Export] float moveSpeed = 2f;
	[Export] Vector3 destination;
	private Vector3 startPos;
	[Export] private bool autoMove = true;
	public bool isMoving = false;
	private bool moveToDest = true;
	private Vector3 moveDir;

	[Signal]
	public delegate void _movement_completedEventHandler();
	public override void _Ready()
    {
		startPos = GlobalPosition;
		destination = startPos + destination;
		moveDir = (destination - startPos).Normalized();
        if (autoMove) isMoving = true;
        if (HasMeta("State"))SetMeta("State", isMoving ? "ON" : "OFF");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
    {
        if (!HasMeta("State")) return;
        var state = GetMeta("State").ToString();
        if (state != "OFF")
        {
            moveToDest = state.ToString() == "FORWARD";
            Vector3 target = moveToDest ? destination : startPos;
            Vector3 direction = moveToDest ? moveDir : -moveDir;
            GlobalPosition += direction * (float)delta * moveSpeed;

            if (GlobalPosition.DistanceTo(target) < 0.1f)
            {
                EmitSignal(SignalName._movement_completed);
                //moveToDest = !moveToDest;
                if (!autoMove) 
                {
                    SetMeta("State", "OFF");
                    GlobalPosition = target;
                }
            }
        }
	}

    public void GetState(bool state)
    {
        moveToDest = state;
        isMoving = true;
    }

}