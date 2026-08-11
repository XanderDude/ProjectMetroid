using Godot;

public partial class aggroStateWIP : State
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


    private OmniLight3D parryLight;
    private bool parryHandled = false;

    private float lightTweenTimer = 0f;
    private bool lightGrowing = true;

    private const float ORIGINAL_CAMERA_DISTANCE = 25f;
    private const float ZOOM_DISTANCE = 20f;

    public override void Enter()
    {
        substate = SubStateParry.FLASH;
        flashTimer = 0f;
        flashIntervalTimer = 0f;
        parryHandled = false;
        lightTweenTimer = 0f;
        lightGrowing = true;

        SpawnParryLight();
        RavenAttackState.OnRavenSlashHit += OnSlashHitEnemy;

    }

    public override void Exit()
    {
        RavenAttackState.OnRavenSlashHit -= OnSlashHitEnemy;

        if (IsInstanceValid(parryLight))
            parryLight.QueueFree();
        parryLight = null;

    }

    public override void Update(float delta)
    {
        if (substate == SubStateParry.FLASH)
            UpdateFlash(delta);
    }

    public override void PhysicsUpdate(float delta)
    {
        ec.Velocity += ec.GravityVector * delta;

        switch (substate)
        {
            case SubStateParry.FLASH:
                // Hold still during flash
                ec.Velocity = new Vector3(0f, ec.Velocity.Y, 0f);
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
		substate = SubStateParry.LUNGE;
		lungeAirborneTimer = 0f;
		hasLeftGround = false;

		float lungeDir = Mathf.Sign(ec.player.GlobalPosition.X - ec.GlobalPosition.X);
		ec.Velocity = new Vector3(lungeDir * ec.runspeed * 5f, 3f, 0f);
	}

	private void UpdateLunge(float delta)
	{
		lungeAirborneTimer += delta;

		// wait a few frames for him to actually leave the ground
		if (lungeAirborneTimer > 0.15f && !ec.IsOnFloor())
			hasLeftGround = true;

		if (hasLeftGround && ec.IsOnFloor() && !parryHandled)
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

		float knockbackDir = Mathf.Sign(ec.GlobalPosition.X - ec.player.GlobalPosition.X);
		ec.Velocity = new Vector3(knockbackDir * ec.attackknockback * 3f, 4f, 0f);
		ec.DamagedReceived(ec.parryDamage);

		var tween = ec.CreateTween();
		tween.TweenInterval(0.4f);
		tween.TweenCallback(Callable.From(() =>
		{
			if (IsInstanceValid(ec))
            
				esm.TransitionTo("patrolState");
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