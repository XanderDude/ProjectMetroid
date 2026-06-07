using Godot;
using System;

public partial class Switch_Lever : Node3D
{
	[Export] public bool isOn = false;
	private Area3D area;
	// Called when the node enters the scene tree for the first time.
	
	public override void _Ready()
	{
		area = GetNode<Area3D>("Area3D");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        if (isOn)
		{
			this.RotationDegrees = new Vector3(0, 0, -45);
		}
		else
		{
			this.RotationDegrees = new Vector3(0, 0, 45);
		}
    }

	public void _on_area_3d_body_entered(Node3D body)
	{
		PlayerManager player = body as PlayerManager;
		if (player.Velocity.X > 0) //moving right
        {
            isOn = true;
        }
		else if (player.Velocity.X < 0)
		{
			isOn = false;
		}
	}
	
	public void Toggle()
	{
		isOn = !isOn;
	}
}
