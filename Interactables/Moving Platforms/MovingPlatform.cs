using Godot;
using System;

public partial class MovingPlatform : Node3D
{
	[Export] float moveSpeed = 2f;
	[Export] Vector3 Dest1, Dest2;
	[Export] private bool autoMove = true;
	public bool isMoving = false;
	private bool moveTo2 = false;
	private Vector3 moveDir;
	private Node3D platform;

	public override void _Ready()
    {
		platform = GetNode<Node3D>("Platform");
		moveDir = (Dest2 - Dest1).Normalized();
        if (autoMove) isMoving = true;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
    {
        if (isMoving)
        {
            Vector3 target = moveTo2 ? Dest2 : Dest1;
            Vector3 direction = moveTo2 ? moveDir : -moveDir;
            platform.Position += direction * (float)delta * moveSpeed;

            if (platform.Position.DistanceTo(target) < 0.1f)
            {
                moveTo2 = !moveTo2;
            }
        }
	}
}

