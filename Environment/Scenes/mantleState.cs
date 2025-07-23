using Godot;
using System;

public partial class mantleState : State
{

	public override void Enter()
	{
		GD.Print("Entered Mantle State.");
		parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Hanging();
		//parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Mantle();
	}
	
	public override void Exit()
	{
		GD.Print("Exited Mantle State");
		parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).BeginJump();
	}
	
	public override void PhysicsUpdate(float delta)
	{
		player.MoveAndCollide(Vector3.Zero);
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
