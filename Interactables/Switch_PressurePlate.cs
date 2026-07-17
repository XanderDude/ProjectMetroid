using Godot;
using System;

public partial class Switch_PressurePlate : Node3D
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
        if (isOn)
		{
			GetParent<Node3D>().Position = new Vector3(0, -.1f, 0);
		}
		else
		{
			GetParent<Node3D>().Position = new Vector3(0, 0, 0);
		}
    }

	public void _on_body_entered(Node3D body)
	{
		if (!interactable) return;
		PlayerManager player = body as PlayerManager;
		isOn = true;
		RotateLever(isOn);
	}
	
	public bool Toggle()
	{
		isOn = !isOn;
		RotateLever(isOn);
		return isOn;
	}

	public void LockSwitch(bool isLocked)
	{
		interactable = !isLocked;
	}

	private void RotateLever(bool on)
    {
		EmitSignal(SignalName.switch_flipped);
		//GD.Print("Emitting signal");
		if (on)
		{
			GetParent<Node3D>().RotationDegrees = new Vector3(0, 0, -45);
		}
		else
		{
			GetParent<Node3D>().RotationDegrees = new Vector3(0, 0, 45);
		}
		
    }
}
