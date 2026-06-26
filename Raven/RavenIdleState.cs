using Godot;
using System;

public partial class RavenIdleState : State
{
	public override void Enter()
	{
		//GD.Print("Raven: Entered Idle State");

		
	}

	public override void Exit()
	{
	}

	public override void Update(float delta)
	{
	}

	public override void PhysicsUpdate(float delta)
	{
		raven.Velocity = Vector3.Zero;
		if ((pm.GlobalPosition + raven.Yoffset + raven.Xoffset).DistanceTo(raven.GlobalPosition) > 4.0f)
		{
			rsm.TransitionTo("RavenRecallState");
		}
	}

	public override void HandleInput(InputEvent @event)
	{
		

		if (@event.IsActionPressed("RavenSpecial"))
		{


				if (raven.canTeleport)
				{
					//GD.Print("Raven: Teleported to player position" + pm.StateMachine._currentState.Name);
					// Define start and end points for the ray
					Vector3 from = pm.GlobalPosition;
					Vector3 to = raven.GlobalPosition;

					// Create the ray query parameters
					PhysicsRayQueryParameters3D query = new PhysicsRayQueryParameters3D
					{
						From = from,
						To = to,
						CollisionMask = (1 << 0) | (1 << 1) 
					};

					// Perform the raycast
					var spaceState = raven.GetWorld3D().DirectSpaceState;
					var result = spaceState.IntersectRay(query);

					// Check if something was hit
					if (result.Count > 0)
					{
						GD.Print("Ray hit: " + result["collider"]);
					}
					else
					{
						GD.Print("Ray did not hit anything.");
					}
					pm.GlobalPosition = raven.GlobalPosition;
					raven.canTeleport = false;
					pm.Velocity = Vector3.Zero;
				}


			if (pm.StateMachine._currentState.Name == "mantleState")
			{
				pm.StateMachine.TransitionTo("jumpState");  
			}

				rsm.TransitionTo("RavenRecallState");
		
				
				
				
				
			

		   
		}
	}


}
