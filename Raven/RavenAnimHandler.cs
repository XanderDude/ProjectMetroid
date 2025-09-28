using Godot;
using System;


public partial class RavenAnimHandler : AnimationTree
{
	[Export] private RavenStateMachine rsm;
	private AnimationNodeStateMachinePlayback playback;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		playback = (AnimationNodeStateMachinePlayback)Get("parameters/playback");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (rsm._currentState.Name == "RavenLaunchState")
		{
			playback?.Travel("Launching");
		}
		else if (rsm._currentState.Name == "RavenRecallState")
		{
			playback?.Travel("Flying");
		}
		else playback?.Travel("Hovering");
	}
}
