using System;
using Godot;

// TODO : Fix and refactor this class. It's a mess and needs to be cleaned up.
public partial class lungeState : State
{
	private enum SubStateParry { FLASH, LUNGE, KNOCKBACK, DONE }

	private enum SubState {TURN, APPROACH, ATTACK}
	private SubStateParry substate = SubStateParry.FLASH;

	private float lungeAirborneTimer = 0f;
	private bool hasLeftGround = false;
	private float flashTimer = 0f;
	private const float FLASH_DURATION = 0.8f;
	private float flashIntervalTimer = 0f;
	private const float FLASH_INTERVAL = 0.12f;

	// Flying lunge
	private const float FLYING_LUNGE_DURATION = 0.9f;
	private float flyingLungeTimer = 0f;
	private Vector3 lungeDirection;
	private Vector3 lungeTargetPos;

	private OmniLight3D parryLight;
	private bool parryHandled = false;

	private float lightTweenTimer = 0f;
	private bool lightGrowing = true;

	private const float ORIGINAL_CAMERA_DISTANCE = 25f;
	private const float ZOOM_DISTANCE = 20f;

	Vector3 dir;
	public override void Enter()
	{
		substate = SubStateParry.FLASH;
		dir = ec.player.GlobalPosition - ec.GlobalPosition;
		ec.FaceDirection(dir.X);
		flashTimer = 0f;
		flashIntervalTimer = 0f;
		parryHandled = false;
		lightTweenTimer = 0f;
		lightGrowing = true;
		ec.Velocity = Vector3.Zero;
		SpawnParryLight();
		RavenAttackState.OnRavenSlashHit += OnSlashHitEnemy;

	}

	public override void Exit()
	{
		RavenAttackState.OnRavenSlashHit -= OnSlashHitEnemy;

		if (IsInstanceValid(parryLight))
			parryLight.QueueFree();
		parryLight = null;

		ec.Velocity = Vector3.Zero;
	}

	public override void Update(float delta)
	{
		if (substate == SubStateParry.FLASH)
			UpdateFlash(delta);
	}

	public override void PhysicsUpdate(float delta)
	{
		if (!ec.isFlying)
			ec.Velocity += ec.GravityVector * delta;

		switch (substate)
		{
			case SubStateParry.FLASH:
				// Hold still during flash
				ec.Velocity = ec.isFlying ? Vector3.Zero : new Vector3(0f, ec.Velocity.Y, 0f);
				break;

			case SubStateParry.LUNGE:
				UpdateLunge(delta);
				break;
		}
	}

	private void UpdateFlash(float delta)
	{
		flashTimer += delta;
		flashIntervalTimer += delta;

		if (IsInstanceValid(parryLight))
		{
			lightTweenTimer += delta * (lightGrowing ? 1f : -1f);
			lightTweenTimer = Mathf.Clamp(lightTweenTimer, 0f, FLASH_INTERVAL);
			float t = lightTweenTimer / FLASH_INTERVAL;
			parryLight.LightEnergy = Mathf.Lerp(0.5f, 3.5f, t);

			if (lightTweenTimer >= FLASH_INTERVAL) lightGrowing = false;
			else if (lightTweenTimer <= 0f) lightGrowing = true;
		}

		if (flashTimer >= FLASH_DURATION)
		{
			if (IsInstanceValid(parryLight))
				parryLight.LightEnergy = 3.5f;
			StartLunge();
		}
	}

	private void StartLunge()
	{
		SoundFriend.Play("ratGrunt1");
		substate = SubStateParry.LUNGE;
		lungeAirborneTimer = 0f;
		hasLeftGround = false;
		flyingLungeTimer = 0f;

		if (ec.isFlying)
		{
			// Dive at wherever the player is right now (X and Y), but bias the
			// direction toward more downward pull so it reads as a dive-bomb
			// even when the player is mostly off to the side.
			lungeTargetPos = ec.player.GlobalPosition;
			Vector3 toPlayer = lungeTargetPos - ec.GlobalPosition;
			toPlayer.Z = 0f;

			if (toPlayer.LengthSquared() > 0.001f)
			{
				toPlayer = toPlayer.Normalized();
				float steepenedY = Mathf.Min(toPlayer.Y, toPlayer.Y - 0.4f); // pull further negative (downward)
				lungeDirection = new Vector3(toPlayer.X, steepenedY, 0f).Normalized();
			}
			else
			{
				
				lungeDirection = Vector3.Down;
			}

			ec.FaceDirection(lungeDirection.X);
			ec.Velocity = lungeDirection * ec.runspeed * ec.lungespeed;
		}
		else
		{
			ec.FaceDirection(ec.Velocity.X);
			ec.Velocity = new Vector3(ec.runspeed * Mathf.Sign(dir.X) * 3f, 7f, 0f);
		}
	}

	private void UpdateLunge(float delta)
	{
		if (ec.isFlying)
		{
			UpdateLungeFlying(delta);
			return;
		}

		lungeAirborneTimer += delta;

		// wait a few frames for him to actually leave the ground
		if (lungeAirborneTimer > 0.15f && !ec.IsOnFloor())
			hasLeftGround = true;

		if (hasLeftGround && ec.IsOnFloor() && !parryHandled)
		{
			parryHandled = true;
			substate = SubStateParry.DONE;
			esm.TransitionTo("aggroState");
		}
	}

	private void UpdateLungeFlying(float delta)
	{
		flyingLungeTimer += delta;

		Vector3 toTarget = lungeTargetPos - ec.GlobalPosition;
		toTarget.Z = 0f;

		// once the dot flips negative we've flown past the target point
		bool overshot = toTarget.Dot(lungeDirection) <= 0f;
		bool timedOut = flyingLungeTimer >= FLYING_LUNGE_DURATION;

		if ((overshot || timedOut) && !parryHandled)
		{
			parryHandled = true;
			substate = SubStateParry.DONE;
			esm.TransitionTo("patrolState");
		}
	}


	private void OnSlashHitEnemy(Enemy hitEnemy)
	{
		if (hitEnemy != ec) return;
		if (substate != SubStateParry.LUNGE) return;
		if (parryHandled) return;

		parryHandled = true;
		OnParrySuccess();
	}

	

	private void OnParrySuccess()
	{
		GD.Print("Parry success!");
		substate = SubStateParry.KNOCKBACK; // stops UpdateLunge from running

		if (GameFriend.gameinstance?.camera != null)
			GameFriend.gameinstance.camera.ZoomTo(ZOOM_DISTANCE, 0.10f);
		var tweene = CreateTween().SetIgnoreTimeScale(true);
		GameFriend.gameinstance.player.LockMovement(true);
		tweene.TweenProperty(Engine.Singleton, "time_scale", 0.2, 0.1);
		tweene.TweenInterval(0.3);
		tweene.TweenProperty(Engine.Singleton, "time_scale", 1.0, 0.2);
		tweene.Finished += () => GameFriend.gameinstance.player.LockMovement(false);

		if (IsInstanceValid(parryLight))
		{
			parryLight.LightColor = new Color(0.3f, 1f, 0.4f);
			parryLight.LightEnergy = 5f;
		}

		if (ec.isFlying)
		{
			Vector3 away = ec.GlobalPosition - ec.player.GlobalPosition;
			away.Z = 0f;
			away = away.LengthSquared() > 0.001f ? away.Normalized() : Vector3.Right;
			ec.Velocity = away * ec.attackknockback * 3f;
		}
		else
		{
			float knockbackDir = Mathf.Sign(ec.GlobalPosition.X - ec.player.GlobalPosition.X);
			ec.Velocity = new Vector3(knockbackDir * ec.attackknockback * 3f, 4f, 0f);
		}

		ec.DamagedReceived(ec.parryDamage);

		var tween = ec.CreateTween();
		tween.TweenInterval(0.4f);
		tween.TweenCallback(Callable.From(() =>
		{
			if (IsInstanceValid(ec))
			{
				if (GameFriend.gameinstance?.camera != null)
					GameFriend.gameinstance.camera.ZoomTo(ORIGINAL_CAMERA_DISTANCE, 0.01f);

				esm.TransitionTo("patrolState");
			}
		}));

	}
   

	private void SpawnParryLight()
	{
		parryLight = new OmniLight3D();
		parryLight.LightColor = new Color(1f, 1f, 1f);
		parryLight.LightEnergy = 0f;
		parryLight.OmniRange = 3f;
		parryLight.Position = new Vector3(0f, 1f, 0f);
		ec.AddChild(parryLight);
	}
}
