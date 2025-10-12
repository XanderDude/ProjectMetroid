using Godot;
using System;
using System.Numerics;

public partial class postpatrolState : State
{

   
	public override void Enter()
	{

		GD.Print($"{Controller.Name} entered postpatrolState");
		Controller.speed = Controller.walkspeed * Mathf.Sign(Controller.speed);
		


		
		
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
		Controller.timer2 += delta;
		Controller.timer += delta;

		if (Controller.timer >= 5.0f)
		{
			var num = Controller.GetRandom1234();
			if (num == 1 || num == 2)
			{
			Controller.speed = -Controller.speed;
			}
				
			Controller.timer = 0;
		}
	
		
	
		if (Controller.timer2 >= 8.0f)
		{
			var num = Controller.GetRandom1234();
			GD.Print($"Random behavior: {num}");


			if (num == 1 || num == 2)
			{

				Controller.speed = Controller.walkspeed * Mathf.Sign(Controller.speed);
			}
			else if (num == 3)
			{
				Controller.speed = Controller.idle * Mathf.Sign(Controller.speed);
			}
			
			Controller.timer2 = 0;
		}
		
		Controller.Velocity = new Godot.Vector3(Controller.speed, Controller.gravity, 0);
	}
}
