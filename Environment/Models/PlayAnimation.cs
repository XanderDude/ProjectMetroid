using Godot;
using System;
using Godot.Collections;

public partial class PlayAnimation : AnimationPlayer
{
	private AnimationPlayer anim;
	public String idleAnim = "mixamo_com";

	public override void _Ready()
	{
		anim = this;
		anim.CurrentAnimation = idleAnim;
		anim.Play("Take 001");
	}

}
