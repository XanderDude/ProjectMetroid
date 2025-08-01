using Godot;
using System;


public partial class TextureRect : Godot.TextureRect
{
	AnimationPlayer regaliaText;
	Area3D collider;
	Godot.Timer timers;
	private int entranceCount = 0;
	
	public override void _Ready() {
		regaliaText = GetNode<AnimationPlayer>("%AnimationPlayer"); 
		collider = GetNode<Area3D>("%Area3D");
		timers = GetNode<Godot.Timer>("../Timer"); 
		
		timers.WaitTime = 5.0f;
		timers.OneShot = true;
		
	}
	public void _on_area_3d_body_entered(Node3D body) {
		
		if (body == GetNode<Node3D>("../../../Player") && (collider.GlobalPosition.X > body.GlobalPosition.X) && entranceCount == 0) {
			 regaliaText.Play("fade_in_out");
			 entranceCount++;
		}
		else if (body == GetNode<Node3D>("../../../Player") && (collider.GlobalPosition.X > body.GlobalPosition.X)) {
			 regaliaText.Play("fade_in_out_mute");
		}
	}
	public void _on_area_3d_body_exited(Node3D body) {
			timers.Start();
			
		}
	
	
	
}
