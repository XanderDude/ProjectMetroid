using Godot;
using System;

public partial class InteractableSoul : Node3D
{
	[Export] private float jumpSpeed = 12f;
	float cooldown = .2f;
	float cooldownTimer = 0f;
	[Export] private Node3D pulseVFX;
	public override void _PhysicsProcess(double delta)
	{
		if (cooldownTimer < cooldown) 
		{
			cooldownTimer += (float)delta;
			if (pulseVFX != null) pulseVFX.Scale = pulseVFX.Scale.Lerp(new(1,1,1), cooldown * (float)delta);
		}
		else {
			cooldownTimer = cooldown;
			if (pulseVFX != null) pulseVFX.Scale = new(1,1,1);
		}
	}

	public void _on_area_3d_area_entered(Area3D area)
	{
		if (cooldownTimer >= cooldown && GameFriend.gameinstance.player != null && GameFriend.gameinstance.raven.RavenStateMachine._currentState.Name == "RavenAttackState")
		{
			var player = GameFriend.gameinstance.player;
			if (GameFriend.gameinstance.raven.RavenStateMachine.GetNode<RavenAttackState>("RavenAttackState").attackDirection.Y < 0 && !player.IsOnFloor())
			{
				cooldownTimer = 0f;
				player.SlashJump();
				if (pulseVFX != null) PulseSoul();
			}
		} 
	}

	private void PulseSoul()
	{
		pulseVFX.Scale *= 2;
	}
}
