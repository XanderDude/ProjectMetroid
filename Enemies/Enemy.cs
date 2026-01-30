using Godot;
using System;
using System.ComponentModel;
using System.Collections.Generic;

public partial class Enemy : Actor
{
	private EnemyStateMachine emsm;
	
	[ExportGroup("Available States")]
	private Dictionary<string, bool> _stateToggles = new Dictionary<string, bool>();

	public override void _Ready()
	{
		
	
	}

	public override void _PhysicsProcess(double delta)
	{
		emsm._currentState.PhysicsUpdate((float)delta);
		
	}

	public override void _Process(double delta)
	{
		emsm._currentState.Update((float)delta);
	}

	public void DamagedRecieved(int damage)
	{


		GD.Print($"{this.Name} took {damage} damage");
		

		if (health <= 0) return; //already dead
		health -= damage;
		if (health <= 0)
		{
			
		}
	}

	


	


	
	
}
