using Godot;
using System;

public partial class EnemyState : Node
{
	//abstract class defining different states a character can be in
	public EnemyController controller;
	public Node3D mesh;
	
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
	public virtual void PhysicsUpdate(float delta)
	{
		
	}
	
}
