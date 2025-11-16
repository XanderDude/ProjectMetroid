using System;
using Godot;

public partial class RavenLaunchState : State
{


	private float timer = 0f;

	public override void Enter()
	{
		//GD.Print("Raven: Entered Launch State");
		raven.canLaunch = false;
		timer = 0f;

		raven.sound.Stream = GD.Load<AudioStream>("res://Raven/Sounds/LaunchSound.mp3");
		raven.sound.Play();
		
	

	}
	public override void Exit()
	{
	}

	


	public override void Update(float delta)
	{
		if (raven.player.GlobalPosition < raven.GlobalPosition)
			{
				
				raven.RotationDegrees = new Vector3(0, 180, 0);
			}
			else if (raven.player.GlobalPosition > raven.GlobalPosition)
			{
				raven.RotationDegrees = new Vector3(0, 0, 0);
			}
	}
	public override void PhysicsUpdate(float delta)
	{
		RavenLaunch(delta);
	}

	public override void HandleInput(InputEvent @event)
	{
		if (@event.IsActionPressed("RavenSpecial"))
		{


				if (raven.canTeleport)
				{
					//GD.Print("Raven: Teleported to player position" + raven.player.StateMachine._currentState.Name);
					// Define start and end points for the ray
					
					raven.player.GlobalPosition = new Vector3(raven.GlobalPosition.X, raven.GlobalPosition.Y - 1.0f, raven.GlobalPosition.Z);
					raven.canTeleport = false;
					raven.player.Velocity = Vector3.Zero;
				}


			if (raven.player.StateMachine._currentState.Name == "mantleState")
			{
				raven.player.StateMachine.TransitionTo("jumpState");  
			}

				rsm.TransitionTo("RavenRecallState");
		
				
				
				
				
			

		   
		}
	}


	public Vector3 RavenDirection()
	{
		Vector3 dir = Vector3.Zero;
		if (Input.IsActionPressed("Up")) dir.Y += 1;
		if (Input.IsActionPressed("Down")) dir.Y -= 1;
		if (Input.IsActionPressed("Left")) dir.X -= 1;
		if (Input.IsActionPressed("Right")) dir.X += 1;
		if (dir != Vector3.Zero)
			return new Vector3(Input.GetAxis("Left", "Right"), Input.GetAxis("Down", "Up"), 0).Normalized();

		else
			return new Vector3(Mathf.Sign(raven.player.GetNode<Node3D>("%PlayerMesh").RotationDegrees.Y), 0, 0);  
	}

	public void RavenLaunch(float delta)
	{

	
				
		if (raven.player == null)
		{
			GD.PrintErr("Raven: PlayerManager is null");
			return;
		}
		if (!raven.isInAction)
		{
			raven.direction = RavenDirection();
			raven.Velocity = raven.direction * raven.speed;

		}
		raven.isInAction = true;

		

		if (raven.direction != Vector3.Zero)
		{

			timer += delta;

			//check collision of top collider


			if (timer >= raven.launchTimer)
			{
				rsm.TransitionTo("RavenRecallState");
			}
		}
	}
}
