using Godot;
using System;

public partial class MovingCageDrop : AnimatableBody3D
{
	[ExportGroup("Drop Cage Values")]
	private float fallSpeed = 0f;
	private bool collisionFound = false;
	private bool falling = false;
	[Export] private float dropLocationY = 20f; //calulated on ready
	private Area3D fallingCollider; //area3d that damages while falling

	[ExportGroup("Moving Cage Values")]

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
		fallingCollider = FindChild("Area3D", true, false) as Area3D;
		fallingCollider.Monitoring = false; //disable damage area until falling

		startPos = GlobalPosition;
		destination = startPos + destination;
		moveDir = (destination - startPos).Normalized();
        if (autoMove) isMoving = true;
        if (HasMeta("State"))SetMeta("State", isMoving ? "ON" : "OFF");
    }

	private void CalculateFallDistance()
    {
		var rayCast = GetNode<RayCast3D>("RayCast3D");
		if (rayCast.IsColliding())
		{
			dropLocationY = rayCast.GetCollisionPoint().Y;
			GD.Print(dropLocationY);
			collisionFound = true;
			rayCast.Enabled = false;
		}
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!HasMeta("State")) return;
		
		if (falling) DropCage(delta);
		else CalculateMovingCage(delta);

    }

    private void DropCage(double delta)
    {
        //if (!collisionFound) CalculateFallDistance();
        fallSpeed += (float)delta;
        this.Translate(new Vector3(0, -fallSpeed, 0));
        if (GlobalPosition.Y <= dropLocationY)
        {
            fallingCollider.Monitoring = false; //disable damage area
            falling = false;
            GlobalPosition = new Vector3(GlobalPosition.X, dropLocationY, GlobalPosition.Z);
			ProcessMode = ProcessModeEnum.Disabled;
        }
    }

    private void CalculateMovingCage(double delta)
    {
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

	public void _on_chain_body_entered(Node3D body)
	{
		//CalculateFallDistance();
		falling = true;
		fallingCollider.Monitoring = true; //enable damage area
		GD.Print("chain broken");
	}

	public void _on_cage_crush(Node3D body)
    {
		GD.Print("cage crush");
		if (body is EnemyController enemy)
		{
			enemy.DamagedRecieved(100000);
		}
		else if (body is PlayerManager player)
        {
			player.Health -= 100000;
        }
    }

	public void GetState(bool state)
    {
        moveToDest = state;
        isMoving = true;
    }
}
