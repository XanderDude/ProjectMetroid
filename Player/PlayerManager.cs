using Godot;
using System;	

public partial class PlayerManager : CharacterBody3D
{
	[Export] public NodePath playerMeshPath = "%PlayerMesh";

	[Export] private ShaderMaterial mainMat, weaponMat;
	[Export] public VerticalCollisionCheck vertColCheck;
	[Export] public ShapeCast3D groundCheck;
	public bool jumpQueued;
	public bool slideQueued;
	public bool slideBoost;

	public Vector3 knockbackVelocity = Vector3.Zero;
	public bool isDead = false;
	public bool isPressed = false; 
	public bool inwater = false;

	public bool movementlocked = false;

	public Vector2 aimDirection = Vector2.Right;
	public bool noAimDirection;
	public int facingDirection = 1; 

	[Export] public int health = 100;

	[Export] public float invulnDuration = 1f;
    public float invulnTimer = 0f;
    public bool canBeDamaged => invulnTimer <= 0f;
	
	[Export] public PlayerStateMachine psm;
	[Export] public PackedScene jumpVFX, slideBoostVFX;

	public float jumpBufferTimer = 0f;
	private const float JumpBufferTime = 0.1f; 

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
	}

	public void TakeDamage(int amount)
    {
        if (!canBeDamaged) return;
        health -= amount;
        invulnTimer = invulnDuration;
        if (health <= 0)
        {
            health = 0;
            isDead = true;
        }
    }
}
