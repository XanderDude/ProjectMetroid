using Godot;
using System;

public partial class State : Node
{
	//Player related
	public MovementStateMachine msm;
	public AttackStateMachine asm;
	public CharacterBody3D player;
	public PlayerManager pm;
	

	public CharacterBody3D projectileArrow;

	//idk????
	public Node3D parentMesh;

	//Raven related
	public RavenStateMachine rsm;
	public Raven raven;

	//Enemy related
	public Enemy ec;
	public EnemyStateMachine esm;

	public float _gravity = 9.8f;

	public virtual void Enter(){}
	public virtual void Exit() {}
	new public virtual void Ready() {}
	public virtual void Update(float delta) {}
	public virtual void PhysicsUpdate(float delta) {}
	public virtual void HandleInput(InputEvent @event) {}
	
}
