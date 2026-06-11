using Godot;
using System;

public partial class SwitchManager : Node3D
{

	[Export] private bool switchOn = false;
	[Export] private bool oneShotSwitch = false; //if true, switch can only be flipped once
	private Switch_Lever switchObject;
	[Export] private Node3D[] switchedObjects; //objects that are affected by the switch

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		switchObject = GetNode<Switch_Lever>("Area3D");
		switchObject.isOn = switchOn; 
		SetSwitch(switchOn);
	
		//switchLever.Connect(Switch_Lever.SignalName.switch_flipped, Callable.From(_on_area_3d_switch_flipped)); //connect switch lever signal to toggle function
	}

	public void _on_area_3d_switch_flipped()
	{
		SetSwitch(!switchOn); //toggle
		if (oneShotSwitch)
		{
			switchObject.LockSwitch(true);
		}
	}

	
	public void SetSwitch(bool state)
    {
		switchOn = state;
		GD.Print("Toggling");
		foreach (Node3D obj in switchedObjects)
		{
			GD.Print("Toggling object: " + obj.Name);
			if (obj is MovingCage cage)
			{
				GD.Print("Toggling cage");
				cage.SetState(switchOn);
			}
			else if (obj is MovingPlatform platform)
			{
				platform.isMoving = switchOn;
			}
			//obj.Visible = switchState;
		}
	}


}
