using Godot;
using System;
using System.Collections.Generic;

public partial class FiniteStateMachine : Node
{
	[Export] public State initialState;
	State current_node_state;
	String current_node_state_name;
	Dictionary<string, State> node_states;

    public override void _Ready()
	{
		foreach (State child in GetChildren())
		{
					node_states[child.Name] = child;
					child.Transition += transition_to;
				
		}
		if (initialState != null)
		{
			initialState.Enter();
			current_node_state = initialState;

		}
	}

	public void Process(float delta)
	{
		if (current_node_state != null)
		{
			current_node_state.Update(delta);
		}
	}
    public void _PhysicsProcess(float delta)
    {
        if (current_node_state != null)
		{
			current_node_state.PhysicsUpdate(delta);
		}
    }

    
	public void transition_to(String node_state_name)
	{
		if (node_state_name == current_node_state_name)
       		return;

    	if (!node_states.ContainsKey(node_state_name))
        	return;

  		State new_node_state = node_states[node_state_name];

    	current_node_state?.Exit();
    	current_node_state = new_node_state;
    	current_node_state_name = node_state_name;
    	current_node_state.Enter();
	}

}
