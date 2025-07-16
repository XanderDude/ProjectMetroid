using Godot;
using System;


public partial class TextureRect : Godot.TextureRect
{
	public void _on_area_3d_body_entered(Node3D body) {

		if (body == GetNode<Node3D>("%Player")) {
			Visible = true;
			
		}
	}
	
	public void _on_timer_timeout() {
		Visible = false;
		
	}
	
}
