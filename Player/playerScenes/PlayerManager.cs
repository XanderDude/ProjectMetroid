using Godot;
using System;

public partial class PlayerManager : CharacterBody3D
{
	private NodePath playerMesh = "%PlayerMesh"; //unique local node to the Player scene
	private MovementStateMachine _stateMachine;


	[Export] public bool jumpQueued; //player is holding the jump button
	[Export] public bool slideQueued; //player is holding slide button
	[Export] public bool slideBoost; //player is airborn/just landed
	
		[Export]
	private MovementStateMachine StateMachine //init StateMachine
	{
		get { return _stateMachine; }
		set //assign self and mesh to state machine prior to _ready
		{
			_stateMachine = value;
			StateMachine.Parent = this;
			StateMachine.parentMesh = GetNode<Node3D>(playerMesh);
		}
	}


}
