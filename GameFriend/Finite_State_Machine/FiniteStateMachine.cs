using Godot;
using System;
using System.Collections.Generic;

public partial class FiniteStateMachine : Node
{
	[Export] public State initialState;
	public State current_node_state;

	protected State previous_node_state;

	public String previous_node_state_name;
	public String current_node_state_name;
	Dictionary<string, State> node_states = new Dictionary<string, State>();

    public override void _Ready()
	{
		foreach (Node child in GetChildren())
		{			
			if (child is State s) {
					node_states[s.Name] = s;
					s.Transition += transition_to;
			}
				
		}
		if (initialState != null)
		{
			initialState.Enter();
			current_node_state = initialState;

		}
	}

	public override void _Process(double delta)
	{
		if (current_node_state != null)
		{
			current_node_state.Update((float)delta);
		}
	}
    public override void _PhysicsProcess(double delta)
    {
        if (current_node_state != null)
		{
			current_node_state.PhysicsUpdate((float)delta);
		}
    }

    
	public void transition_to(String node_state_name)
	{
		GD.Print($"transition_to called: {node_state_name}");
		if (node_state_name == current_node_state_name)
			return;

		if (!node_states.ContainsKey(node_state_name))
			return;

		State new_node_state = node_states[node_state_name];

		current_node_state?.Exit();
		previous_node_state = current_node_state; 
		previous_node_state_name = current_node_state_name;
		current_node_state = new_node_state;
		current_node_state_name = node_state_name;
		current_node_state.Enter();
	}

}
