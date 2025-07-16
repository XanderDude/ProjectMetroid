using Godot;
using System;


public partial class TextureRect : Godot.TextureRect
{
	AnimationPlayer regaliaText;
	public override void _Ready() {
		regaliaText = GetNode<AnimationPlayer>("../AnimationPlayer"); 
	}
	public void _on_area_3d_body_entered(Node3D body) {

		if (body == GetNode<Node3D>("%Player")) {
			 regaliaText.Play("fade_in_out");
			
		}
	}
	
	
	
}
