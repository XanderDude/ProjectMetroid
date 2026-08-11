using Godot;
using System;
using System.Collections.Generic;

public partial class AttackStateMachine : Node
{
	[Export] public NodePath initialState; //the node path to the starting state
	private CharacterBody3D _parent;
	public CharacterBody3D Parent //assign from parent script prior to _ready
	{
		get { return _parent; }
		set { _parent = value; }
	}
	private PlayerManager _manager;
	public PlayerManager ParentManager //assign from parent script prior to _ready
	{
		get { return _manager; }
		set { _manager = value; }
	}

	private Node3D _mesh;
	public Node3D parentMesh //assign from parent script prior to _ready
	{
		get { return _mesh; }
		set { _mesh = value; }
	}
	private Dictionary<string, State> _states;
	private State _currentState;
	public  State _previousState;

	//Purpose: This is called when opening the game for the first time, after all child nodes are in the scene
	public override void _Ready()
	{
		_states = new Dictionary<string, State>();
		foreach (Node node in GetChildren())
		{
			if (node is State s)
			{
				_states[node.Name] = s;
				s.asm = this;  //assign self to the states
				s.pm = ParentManager;
				s.parentMesh = parentMesh;
				s.Ready();
				s.Exit(); //reset all states
			}
		}

		_currentState = GetNode<State>(initialState);
		_currentState.Enter(); //run initial state
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		_currentState.HandleInput(@event);

	}

	//Purpose: This is called every frame. delta is time 

	public override void _Process(double delta)
	{

		_currentState.Update((float)delta);
	}

	public override void _PhysicsProcess(double delta)
	{
		_currentState.PhysicsUpdate((float)delta);
	}

	public void TransitionTo(string key) {
		if (!_states.TryGetValue(key, out State value) || _currentState == value) //return if state doesn't exist in dictionary or we're already in requested state
			return;
		_currentState.Exit();
		_previousState = _currentState;
		_currentState = value;
		_currentState.Enter();
	}
	
}
