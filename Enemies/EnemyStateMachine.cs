using Godot;
using System;
using System.Collections.Generic;

public partial class EnemyStateMachine : Node
{
    [Export] public NodePath initialState;

    private Dictionary<string, State> _states;
    public State _currentState;
    public State _previousState;

    public override void _Ready()
    {
        _states = new Dictionary<string, State>();
        foreach (Node node in GetChildren())
        {
            if (node is State s)
            {
                _states[node.Name] = s;
                s.esm = this;
                s.ec = GetParent<Enemy>();
                s.parentMesh = s.ec.mesh;
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
            GD.PrintErr("EnemyStateMachine: initialState is not set!");
        }
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
        if (!_states.TryGetValue(key, out State value) || _currentState == value)
            return;
        _currentState.Exit();
        _previousState = _currentState;
        _currentState = value;
        _currentState.Enter();
    }
}
