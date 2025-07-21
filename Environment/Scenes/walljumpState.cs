using Godot;
using System;

public partial class walljumpState : State
{

	public override void Enter()
	{
		GD.Print("Entered Wall Jump State.");
		//parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Mantle();
	}
	
	public override void Exit()
	{
		GD.Print("Exited Mantle State");
		parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).BeginJump();
	}
	
	public override void PhysicsUpdate(float delta)
	{
		
		
	}
	
	public override void HandleInput(InputEvent @event)
	{
		
		if (@event.IsActionPressed("Down")) {
			msm.TransitionTo("jumpState");
		}
		
		
		
		if (@event.IsActionPressed("Jump")) {
			player.Set("jumpQueued", true);
			msm.TransitionTo("jumpState");
		}

	
	
	
	}

}
