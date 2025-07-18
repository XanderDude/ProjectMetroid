using Godot;
using System;

public partial class State : Node
{
	public MovementStateMachine msm;
	
	public CharacterBody3D player;
	public Node3D playerMesh;
	public AnimationTree animTree;

	[Export] public float gravity = 9.8f;
	[Export] public bool JumpQueued { get; set; }
	public bool slideBoost = false;
	
	public virtual void Ready() //initialize the node
	{
		GD.Print("Test");
	}
	public virtual void Enter() //runs first whenever msm transitions to this state
	{
		
	}
	public virtual void Exit() {}
	public virtual void Update(double delta) {}
	public virtual void PhysicsUpdate(double delta) {}
	public virtual void HandleInput(InputEvent @event) {}
	
}
