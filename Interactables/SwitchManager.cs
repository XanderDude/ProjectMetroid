using Godot;
using System;

public partial class SwitchManager : Node3D
{

	[Export] private bool switchOn = false;
	[Export] private bool waitToReactivate = false;
	private bool waiting = false;
	[Export] private bool oneShotSwitch = false; //if true, switch can only be flipped once
	private Switch_Lever switchObject;
	[Export] private Node3D[] switchedObjects; //objects that are affected by the switch



	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		switchObject = GetNode<Switch_Lever>("Area3D");
		switchObject.isOn = switchOn; 
		SetSwitch(switchOn);

		foreach (Node3D obj in switchedObjects)
		{
			if (obj is MovingCage cage)
			{
				if (!waitToReactivate)cage.Connect(MovingCage.SignalName._movement_completed, Callable.From(FinishWaiting)); 
			}
		}
	}

	public void _on_area_3d_switch_flipped()
	{

		SetSwitch(!switchOn); //toggle
		if (oneShotSwitch)
		{
			switchObject.LockSwitch(true);
		}
	}

	private void FinishWaiting()
	{
		waiting = false;
		switchObject.LockSwitch(false);
	}
	
	public void SetSwitch(bool state)
    {
		if (switchOn != state)
		{
			GD.Print("Switch is now " + (state ? "FORWARD" : "BACK"));
			if (waitToReactivate)
			{
				switchObject.LockSwitch(true);
				waiting = true;
			}
		}
		switchOn = state;
		GD.Print("Toggling");
		foreach (Node3D obj in switchedObjects)
		{
			GD.Print("Toggling object: " + obj.Name);
			if (obj.HasMeta("State"))
			{
				GD.Print("Toggling cage");
				obj.SetMeta("State", switchOn ? "FORWARD" : "BACK");
			}
			else if (obj is MovingPlatform platform)
			{
				platform.isMoving = switchOn;
			}
			//obj.Visible = switchState;
		}
	}


}
