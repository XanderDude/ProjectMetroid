using Godot;
using System;
using System.Collections.Generic;

public partial class MovementStateMachine : Node
{
	[Export] public NodePath initialState; //the node path to the starting state
	[Export] private CharacterBody3D parent;
	[Export] private AnimationTree animations;
	[Export] private Node3D mesh;

	private Dictionary<string, State> _states;
	private State _currentState;

	public bool jumpQueued;

	//Purpose: This is called when opening the game for the first time
	public override void _Ready()
	{
		animations = (AnimationTree)mesh.Get("animTree");
		_states = new Dictionary<string, State>();
		foreach (Node node in GetChildren())
		{
			if (node is State s)
			{
				_states[node.Name] = s;
				s.msm = this; //assign self to the states
				s.player = parent;
				s.animTree = animations;
				s.playerMesh = mesh;
				s.Ready();
				s.Exit(); //reset all states
			}
		}

		_currentState = GetNode<State>(initialState);
		_currentState.Enter(); //run initial state
	}

	//Purpose: This is called every frame. delta is time 

	public override void _Process(double delta) {

		_currentState.Update((float)delta);
	}

	public override void _PhysicsProcess(double delta)
	{
		_currentState.PhysicsUpdate((float)delta);
	}

	public override void _UnhandledInput(InputEvent @event) {
		_currentState.HandleInput(@event);
	}

	public void TransitionTo(string key) {
		if (!_states.ContainsKey(key) || _currentState == _states[key]) //return if state doesn't exist in dictionary or we're already in state
			return;

		_currentState.Exit();
		_currentState = _states[key];
		_currentState.Enter();
	}
	
}
