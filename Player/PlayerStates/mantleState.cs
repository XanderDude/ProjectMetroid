using Godot;
using System;

public partial class mantleState : PlayerState
{

	public override void Enter()
	{
		//GD.Print("Entered Mantle State.");
		pm.pah.Hanging();
		//parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Mantle();
	}
	
	public override void Exit()
	{
		//GD.Print("Exited Mantle State");
	}
	
	public override void PhysicsUpdate(float delta)
	{
		pm.MoveAndCollide(Vector3.Zero);
	}
	
	public override void HandleInput(InputEvent @event)
	{
		
		if (@event.IsActionPressed("Down")) {
			EmitSignal(SignalName.Transition, "jumpState");
		}
		
		
		
		if (@event.IsActionPressed("Jump")) {
			pm.jumpQueued = true;
			pm.pah.Airborne(pm.jumpQueued);
			EmitSignal(SignalName.Transition, "jumpState");
		}

	
	
	
	}

}
