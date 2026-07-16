using Godot;
using System;
using System.Collections.Generic;

public partial class RavenStateMachine : Node
{
    /*
    [Export] public NodePath ravenPath;
    [Export] public NodePath initialState;

    private Raven _raven;
    public Raven raven
    {
        get { return _raven; }
        set { _raven = value; }
    }
    private PlayerManager _manager;
	public PlayerManager PlayerManager //assign from parent script prior to _ready
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
    
    private CharacterBody3D _parent;
	public CharacterBody3D Parent //assign from parent script prior to _ready
	{
		get { return _parent; }
		set { _parent = value; }
	}

    private Dictionary<string, State> _states;
    public State _currentState;
    public State _previousState;

    
    public override void _Ready()
    {
        
        if (ravenPath != null && !ravenPath.IsEmpty)
        {
            _raven = GetNode<Raven>(ravenPath);
        }
        else
        {
            GD.PrintErr("RavenStateMachine: ravenPath is not set!");
        }

        _states = new Dictionary<string, State>();
        foreach (Node node in GetChildren())
        {
            if (node is State s)
            {
                _states[node.Name] = s;
                s.rsm = this;
                s.raven = _raven;
                s.pm = PlayerManager;
                s.player = Parent;
                s.parentMesh = parentMesh;
                s.Ready();
                s.Exit();
            }
        }

        if (initialState != null && !initialState.IsEmpty)
        {
            _currentState = GetNode<State>(initialState);
            _currentState.Enter();
        }
        else
        {
            GD.PrintErr("RavenStateMachine: initialState is not set!");
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        _currentState?.HandleInput(@event);
    }

    public override void _Process(double delta)
    {
        _currentState?.Update((float)delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        _currentState?.PhysicsUpdate((float)delta);
    }

    public void TransitionTo(string key)
    {

        if (!_states.TryGetValue(key, out State value))
        {
           
            return;
        }
        if (_currentState == value)
        {
         
            return;
        }
        _currentState.Exit();
        _previousState = _currentState;
        _currentState = value;
        _currentState.Enter();
    }
    */
}