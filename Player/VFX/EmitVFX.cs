using Godot;
using System;

public partial class EmitVFX : Node3D
{
	[Export] private bool playOnReady;
	[Export] private Node3D[] VFXNodes;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (playOnReady) PlayVFX();
	}

	public void PlayVFX()
	{
		foreach (GpuParticles3D vfx in VFXNodes)
		{
			vfx.Emitting = true;
		}
	}

}
