using Godot;
using System;	

public partial class PlayerManager : CharacterBody3D
{
	public static PlayerManager instance;

	[Export] public Node3D playerMesh;
	[Export] private ShaderMaterial mainMat, weaponMat;
	[Export] public VerticalCollisionCheck vertColCheck;
	[Export] public ShapeCast3D groundCheck;
	[Export] public PlayerAnimationHandler pah;
	[Export] public PlayerStateMachine psm, asm;
	[Export] public PackedScene jumpVFX, slideBoostVFX;
	[Export] public Raven raven;
	[Export] public CameraFriend camera;
	[Export] public int health = 100;
	[Export] public int maxHealth = 100;
	[Export] public float invulnDuration = 1f;
	public bool jumpQueued;
	public bool slideQueued;
	public bool slideBoost;
	public Vector3 knockbackVelocity;

	public InventoryFriend inventoryfriend = new InventoryFriend();

	public Vector2 aimDirection = Vector2.Right;
	public bool noAimDirection;
	public int facingDirection = 1; 

    public float invulnTimer = 0f;
    public bool canBeDamaged => invulnTimer <= 0f;


	public float jumpBufferTimer = 0f;
	private const float JumpBufferTime = 0.1f;


    public override void _Ready()
    {
		NullChecks();
		instance = this;
		camera.init_camera();
	}
		

	public override void _Process(double delta)
	{
    	if (Input.IsActionJustPressed("Jump"))
        	jumpBufferTimer = JumpBufferTime;
    	else
        	jumpBufferTimer -= (float)delta;
	}

	public bool JumpBuffered => jumpBufferTimer > 0f;
	
	public override void _PhysicsProcess(double delta)
	{
		AimingLogic();
		UpdateInvulnerability((float)delta);
	}
	public void SpawnJumpCloud(float yOffset,float rotation)
	{
		var jumpCloud = jumpVFX.Instantiate() as Node3D;
		AddChild(jumpCloud, true);
		jumpCloud.GlobalPosition = new(GlobalPosition.X, GlobalPosition.Y + yOffset, GlobalPosition.Z);
		jumpCloud.RotationDegrees = new(0, 0, rotation);
	}

	public void LockMovement(bool locked)
	{
		AxisLockLinearX = locked;
		AxisLockLinearY = locked;
	}
	
	public void UpdateInvulnerability(float delta)
	{
		if (invulnTimer > 0)
		{
			invulnTimer -= (float)delta;
			mainMat?.SetShaderParameter("flashing", true);
			mainMat?.SetShaderParameter("timer", invulnTimer);
			weaponMat?.SetShaderParameter("flashing", true);
			weaponMat?.SetShaderParameter("timer", invulnTimer);
		}
		else
		{
			mainMat?.SetShaderParameter("flashing", false);
			weaponMat?.SetShaderParameter("flashing", false);
		}	
	}

	public void AimingLogic()
	{
		var input = new Vector2(Input.GetAxis("Left", "Right"), Input.GetAxis("Down", "Up")).Normalized();
		noAimDirection = input == Vector2.Zero;

		if (!noAimDirection)
		{
			float x = Mathf.Abs(input.Y) > .9f ? 0 : Mathf.Sign(input.X);
			float y = Mathf.Abs(input.X) > .9f ? 0 : Mathf.Sign(input.Y);
			aimDirection = new Vector2(x, y).Normalized();
		}
		else
		{
			aimDirection = new Vector2(facingDirection, 0);
		}
	}

	public void TakeDamage(int amount)
    {
        if (!canBeDamaged) return;
        health -= amount;
        invulnTimer = invulnDuration;
        if (health <= 0)
        {
            health = 0;
            psm.transition_to("deathState");
        }
    }

	public void Heal(int amount)
    {
        health = Mathf.Min(health + amount, maxHealth);
        if (health > 0 && psm.current_node_state_name == "deathState")
            psm.transition_to("groundedState");
    }

	public void NullChecks()
	{
		if (playerMesh == null) GD.PushError("PlayerMesh is not assigned in PlayerManager.");
		if (vertColCheck == null) GD.PushError("VerticalCollisionCheck is not assigned in PlayerManager.");
		if (groundCheck == null) GD.PushError("GroundCheck is not assigned in PlayerManager.");
		if (pah == null) GD.PushError("PlayerAnimationHandler is not assigned in PlayerManager.");
		if (psm == null) GD.PushError("PlayerStateMachine is not assigned in PlayerManager.");
		if (asm == null) GD.PushError("PlayerStateMachine is not assigned in PlayerManager.");
		if (jumpVFX == null) GD.PushError("JumpVFX is not assigned in PlayerManager.");
		if (slideBoostVFX == null) GD.PushError("SlideBoostVFX is not assigned in PlayerManager.");
		if (raven == null) GD.PushError("Raven is not assigned in PlayerManager.");
		if (camera == null) GD.PushError("Camera is not assigned in PlayerManager.");
	}
}
