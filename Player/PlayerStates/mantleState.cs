using Godot;
using System;

public partial class mantleState : PlayerState
{

	public override void Enter()
	{
		//GD.Print("Entered Mantle State.");
		parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Hanging();
		//parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Mantle();
	}
	
	public override void Exit()
	{
		//GD.Print("Exited Mantle State");
	}
	
	public override void PhysicsUpdate(float delta)
	{
		player.MoveAndCollide(Vector3.Zero);
	}
	
	public override void HandleInput(InputEvent @event)
	{
		
		if (@event.IsActionPressed("Down")) {
			EmitSignal(SignalName.Transition, "jumpState");
		}
		
		
		
		if (@event.IsActionPressed("Jump")) {
			player.Set("jumpQueued", true);
			EmitSignal(SignalName.Transition, "jumpState");
		}

	
	
	
	}

}
