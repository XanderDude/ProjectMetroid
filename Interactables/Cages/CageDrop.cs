using Godot;
using System;

public partial class CageDrop : StaticBody3D
{
	private float fallSpeed = 0f;
	private bool collisionFound = false;
	private bool falling = false;
	[Export] private float dropLocationY = 20f; //calulated on ready
	private Area3D fallingCollider; //area3d that damages while falling
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        CallDeferred("CalculateFallDistance");
		fallingCollider = FindChild("Area3D", true, false) as Area3D;
		fallingCollider.Monitoring = false; //disable damage area until falling
    }

	private void CalculateFallDistance()
    {
		var rayCast = GetNode<RayCast3D>("RayCast3D");
		if (rayCast.IsColliding())
		{
			dropLocationY = rayCast.GetCollisionPoint().Y;
			collisionFound = true;
			rayCast.Enabled = false;
		}
    }

    public override void _PhysicsProcess(double delta)
    {
		if (!collisionFound) CalculateFallDistance();
		if (!falling) return;
		fallSpeed += (float)delta;
		this.Translate(new Vector3(0, -fallSpeed, 0));
		if (GlobalPosition.Y <= dropLocationY)
		{
			fallingCollider.Monitoring = false; //disable damage area
			falling = false;
			GlobalPosition = new Vector3(GlobalPosition.X, dropLocationY, GlobalPosition.Z);
		}
    }

	public void _on_chain_body_entered(Node3D body)
	{
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

}
