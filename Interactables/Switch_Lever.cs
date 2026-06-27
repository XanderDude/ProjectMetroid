using Godot;
using System;

public partial class Switch_Lever : Node3D
{
	[Export] public bool isOn = false;
	private bool interactable = true;

	// Called when the node enters the scene tree for the first time.

	[Signal]
	public delegate void switch_flippedEventHandler();
	
	public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
		GetParent<Node3D>().RotationDegrees = new Vector3(0, 0, (interactable ? 40 : 75) * (isOn ? -1 : 1));
    }

	public void _on_body_entered(Node3D body)
	{
		if (!interactable) return;
		PlayerManager player = body as PlayerManager;
		if (player.Velocity.X > 0 && !isOn) //moving right
        {
			RotateLever(true);
        }
		else if (player.Velocity.X < 0 && isOn)
		{
			RotateLever(false);
		}
	}

	public void LockSwitch(bool isLocked)
	{
		interactable = !isLocked;
	}

	private void RotateLever(bool on)
    {
		isOn = on;
		EmitSignal(SignalName.switch_flipped);
		GD.Print("Emitting signal");

    }
}
