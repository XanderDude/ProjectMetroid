using Godot;
using System;
using Godot.Collections;

public partial class PlayAnimation : AnimationPlayer
{
	private AnimationPlayer anim;
	[Export] private String idleAnim;

	public override void _Ready()
	{
		anim = this;
		if (idleAnim == null) { anim.CurrentAnimation = idleAnim; }
		anim.Play(idleAnim);
	}

}
