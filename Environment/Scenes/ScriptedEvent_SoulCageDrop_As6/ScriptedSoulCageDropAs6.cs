using Godot;
using System;

public partial class ScriptedSoulCageDropAs6 : Node3D
{

	[Export] public bool activated = false;
	[Export] AnimationPlayer player;
	[Export] private string libraryName = "ScriptedSoulCageDrop_as6";
	private string startAnim, endAnim, reset;
 	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		startAnim = libraryName + "/Start";
		endAnim = libraryName + "/End";
		reset = libraryName + "/RESET";

		if (HasMeta("State"))
		{
			SetMeta("State", activated ? "ON" : "OFF");
			string state = GetMeta("State").ToString();
			if (state == "ON") //If Metadata "State" = "ON" when scene is loaded, event has already occurred.
			{
				player.Play(endAnim);
			}
			else 
			{
				player.Play(reset);
			}
		}

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (GetMeta("State").ToString() == "FORWARD" && !activated)
		{
			activated = true;
			player.Play(startAnim);
		}
	}
}
