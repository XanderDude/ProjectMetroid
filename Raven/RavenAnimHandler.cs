using Godot;
using System;


public partial class RavenAnimHandler : AnimationTree
{
	[Export] private RavenStateMachine rsm;
	private AnimationNodeStateMachinePlayback playbackStates;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		playbackStates = (AnimationNodeStateMachinePlayback)Get("parameters/playback");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (rsm == null) return;

		if (rsm.current_node_state_name == "RavenLaunchState")
		{
			playbackStates?.Travel("Launching");
		}
		else if (rsm.current_node_state_name == "RavenRecallState")
		{
			playbackStates?.Travel("Flying");
		}
		else playbackStates?.Travel("Hovering");
	}
}
