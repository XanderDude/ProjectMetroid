using Godot;
using System;
using System.Collections.Generic;

public partial class EnemyStateMachine : Node
{
    [Export] public NodePath initialState;

    private Dictionary<string, State> _states;
    public State currentState;
    public State previousState;

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
            currentState = GetNode<State>(initialState);
            currentState.Enter();
        }
        else
        {
            GD.PushError("EnemyStateMachine: initialState is not set!");
        }
    }

    public override void _Process(double delta)
    {
        currentState?.Update((float)delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        currentState?.PhysicsUpdate((float)delta);
    }

    public void TransitionTo(string key)
    {
        if (!_states.TryGetValue(key, out State value) || currentState == value)
            return;
        currentState.Exit();
        previousState = currentState;
        currentState = value;
        currentState.Enter();
    }
}
