using Godot;
using System;

public partial class Area3d : Area3D
{
	public override void _Ready() {
		Vector3 Xpos = GlobalPosition;
	}
	public override void _PhysicsProcess(double delta) {
		
	}
	
}
