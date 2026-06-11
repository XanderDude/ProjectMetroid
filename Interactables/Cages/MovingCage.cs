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

	public override void _Ready()
    {
		startPos = GlobalPosition;
		destination = startPos + destination;
		moveDir = (destination - startPos).Normalized();
        if (autoMove) isMoving = true;

    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
    {
        if (isMoving)
        {
            Vector3 target = moveToDest ? destination : startPos;
            Vector3 direction = moveToDest ? moveDir : -moveDir;
            GlobalPosition += direction * (float)delta * moveSpeed;

            if (GlobalPosition.DistanceTo(target) < 0.1f)
            {
                moveToDest = !moveToDest;
                if (!autoMove) 
                {
                    isMoving = false;
                    GlobalPosition = target;
                }
            }
        }
	}

    public void SetState(bool state)
    {
        moveToDest = state;
        isMoving = true;
    }

}