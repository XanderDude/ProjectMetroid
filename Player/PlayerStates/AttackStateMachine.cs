using Godot;
using System;
using System.Collections.Generic;

public partial class AttackStateMachine : Node
{
	[Export] public NodePath initialState; 
	private CharacterBody3D _parent;
	public CharacterBody3D Parent //assign from parent script prior to _ready
	{
		get { return _parent; }
		set { _parent = value; }
	}
	private Player _manager;
	public Player ParentManager //assign from parent script prior to _ready
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
				s.player = Parent;
				s.pm = ParentManager;
				s.parentMesh = s.pm.mesh;
				s.Ready();
				s.Exit(); //reset all states
			}
		}

		_currentState = GetNode<State>(initialState);
		_currentState.Enter(); //run initial state
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (_currentState == null) return;
		_currentState.HandleInput(@event);

	}

	//Purpose: This is called every frame. delta is time 

	public override void _Process(double delta)
	{
		if (_currentState == null) return;
		_currentState.Update((float)delta);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_currentState == null) return;  
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
