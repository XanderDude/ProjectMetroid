using Godot;
using System;

public partial class AudioStreamPlayer : Godot.AudioStreamPlayer
{
	
	public void _on_area_3d_body_entered(Node3D body) {
		
		if (body == GetNode<Node3D>("../../../Player")) {
			Playing = true;
		}
	}
	
	
}
