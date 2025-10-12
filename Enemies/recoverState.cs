using Godot;
using System;

public partial class recoverState : State
{
	
	public override void Enter()
	{
		GD.Print($"{Controller.Name} entered recover State");
		Controller.timer = 0;
	}

	public override void Exit()
	{
		
	}

	public override void Update(float delta)
	{
		
		
		

		if (Controller.isPlayerInAggroBounds()) 
		{
			Controller.statemachine.TransitionTo("aggroState");
		}

	}

	public override void PhysicsUpdate(float delta)
	{
		
		//Thought Process - Enemy comes to a complete stop when entering "recoverState"
		if (Controller.speed > 0.1f)
		{
			Controller.speed -= delta;
		}
		if (Controller.speed < -0.1f)
		{
			Controller.speed += delta;
		}
		else 
		{
			Controller.statemachine.TransitionTo("postpatrolState");
		}
		
		
		
		  Controller.Velocity = new Godot.Vector3(Controller.speed, Controller.gravity, 0);
		
	
		
	}


}
