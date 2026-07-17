using Godot;
using System;

public partial class State : Node
{

	[Signal] public delegate void TransitionEventHandler(string stateName);
	public virtual void Enter(){}
	public virtual void Exit() {}
	new public virtual void Ready() {}
	public virtual void Update(float delta) {}
	public virtual void PhysicsUpdate(float delta) {}
	public virtual void HandleInput(InputEvent @event) {}
	
}
