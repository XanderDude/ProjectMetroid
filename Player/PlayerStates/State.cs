using Godot;
using System;

public partial class State : Node
{
	//abstract class defining different states a character can be in
	public MovementStateMachine msm;
	public AttackStateMachine asm;

	public RavenStateMachine rsm;
	public CharacterBody3D player;
	public PlayerManager pm;

	public Raven raven;
	public Node3D parentMesh;
	public CharacterBody3D projectileArrow;
	
	public float _gravity = 9.8f;
	public virtual void Enter() //runs first whenever msm transitions to this state
	{
		
	}
	public virtual void Exit() {}
	
	new public virtual void Ready() //First time run by msm
	{
		GD.Print("Loaded: " + Name);
	}
	public virtual void Update(float delta) { }
	public virtual void PhysicsUpdate(float delta) {}
	public virtual void HandleInput(InputEvent @event) {}
	
}
