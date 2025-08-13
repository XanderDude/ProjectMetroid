using Godot;
using System;
public partial class attackState : State
{
	
	
	public override void _Ready() 
	{
		
	}
	
	public override void Enter() 
	{
	
	}
	
	public override void Exit()
	{
	
	}
	
	
	public override void PhysicsUpdate(float delta) 
	{
		
	
			
		
	}
	
	public override void HandleInput(InputEvent @event) 
	{
		if (@event.IsActionReleased("Shoot")) 
		{
			asm.TransitionTo("noattackState");
		}
	}
	
}
