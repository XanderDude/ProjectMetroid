using Godot;
using System;

public partial class Enemy : Actor
{

	[ExportGroup("Enemy_References")]
	[Export] public Node3D mesh;
	[Export] public NavigationAgent3D navagent;
	[Export] public EnemyStateMachine esm;
	[Export] public PackedScene projectilescene = null;
	[Export] public RayCast3D ray;

	[ExportGroup("Enemy_Preferences")]
	[Export] public bool revengeMode;

	[Export] public float attackcd;

	[Export] public float lungespeed = 5f;

	public enum AttackDirMode { Any, HorizontalOnly, EightWay }

	public enum MovementMode { Normal, Hopping}

	[Export] public MovementMode movementmode = MovementMode.Normal;
	[Export] public AttackDirMode attackDirMode = AttackDirMode.HorizontalOnly;

	[Export] public bool isFlying { get; set; } = false;
	
	private bool hasAppliedKnockback = false;

	public bool justattacked = false;

	public float timer = 0f;

	public bool isDodging = false;

	public float snapshotSpeed = 0f; //gets speed a moment in time

	public float dodgeDuration = 1f;
	


	private float meshDirection = -90f;
	public PlayerManager player => GameFriend.gameinstance.player;
	public Godot.Vector3 direction { get; set; } = Godot.Vector3.Zero;

	private bool playerInDamageRange = false; //player is within damage collider

	public override void _Ready()
	{
		if (this.isFlying)
		{
			walkspeedVector = new Vector3(walkspeed, 0, 0);
			runspeedVector = new Vector3(runspeed, 0, 0);
		}
		else
		{
			walkspeedVector = new Vector3(walkspeed, gravity, 0);
			runspeedVector = new Vector3(runspeed, gravity, 0);
		}
		meshDirection = mesh.RotationDegrees.Y;
		 var sensor = mesh.GetNodeOrNull<Area3D>("DodgeSensor");
		if (sensor != null)
			sensor.BodyEntered += DodgeProjectile;
		
	}

	public override void _PhysicsProcess(double delta)
	{
		 if (isDodging)
		{
			Velocity = new Vector3(0, Velocity.Y, 0);   
		}
		else
		{
		}
		if (!isFlying && !IsOnFloor())
			Velocity += GravityVector * (float)delta;

		MoveAndSlide();
		}
		
	

	public void OnCollide(Node3D node)
	{
		playerInDamageRange = true;
		DamagePlayer(node, damagedealt);
	}

	public void DamagePlayer(Node3D node, int damage)
	{
		if (node is PlayerManager p && p.canBeDamaged)
	   {
		   p.health -= damage;
		   float knockDir = Mathf.Sign(p.GlobalPosition.X - GlobalPosition.X);
		   /*p.knockbackVelocity = new Vector3(knockDir, 1f, 0f).Normalized() * attackknockback * 3f;
		   p.psm.TransitionTo("knockbackState");
		   */
	   }
	}
	public void OnLeaveCollider(Node3D node)
	{
		playerInDamageRange = false;
	}

	public void DamagedReceived(int damage)
	{
		//GD.Print($"{this.Name} took {damage} damage");
		DamageFlicker();

		if (health <= 0) return; //already dead
		health -= damage;
		if (health <= 0)
		{
			KillEnemy();
		}
	}

	public void KillEnemy()
	{

		if (GameFriend.gameinstance?.camera != null)
		GameFriend.gameinstance.camera.ZoomTo(25f, 0.15f);
		DropItems();
		QueueFree();
	}

	private async void DamageFlicker()
	{
		mesh.Visible = false;
		await ToSignal(GetTree().CreateTimer(.1f, false, false, false), "timeout");
		mesh.Visible = true;
		
	}



	public bool isPlayerInRange(float range)
	{
		float distance = GlobalPosition.DistanceTo(player.GlobalPosition);
	
		if (distance <= range) return true;
		
		return false;
	}


	public void MoveToPlayer(float speed, float acceleration, float offsetY, float offsetX)
	{
		var playerpos = player.GlobalPosition + new Vector3 (offsetX,offsetY,0);
		Vector3 dir = playerpos - GlobalPosition;
		dir.Z = 0;                     
		if (!isFlying) dir.Y = 0;        
		if (dir.LengthSquared() < 0.1f) Velocity = Vector3.Zero;
		dir = dir.Normalized();

		Vector3 target = dir * speed;
		if (!isFlying) target.Y = Velocity.Y;  
		if (Velocity.X != maxspeed)
			Velocity = Velocity.MoveToward(target, acceleration);
		else
		{
			Velocity = Velocity.MoveToward(target, 0);
		}



	}

	public async void Attack(float distance, int size, int damage, float duration)
	{
		// --- Direction: locked at the moment the rat stopped (set in aggroState via ec.direction) ---
		Vector3 toPlayer = direction;
		toPlayer.Z = 0;

		if (attackDirMode == AttackDirMode.HorizontalOnly)
			toPlayer.Y = 0;

		if (toPlayer.LengthSquared() < 0.001f)
			toPlayer = Vector3.Zero;
		else
			toPlayer = toPlayer.Normalized();

		if (attackDirMode == AttackDirMode.EightWay && toPlayer != Vector3.Zero)
		{
			float angle = Mathf.Atan2(toPlayer.Y, toPlayer.X);
			float snapped = Mathf.Round(angle / (Mathf.Pi / 4f)) * (Mathf.Pi / 4f);
			toPlayer = new Vector3(Mathf.Cos(snapped), Mathf.Sin(snapped), 0);
		}

		FaceDirection(toPlayer.X);

		// --- Hitbox ---
		Area3D hitbox = new Area3D();
		hitbox.CollisionMask = 1 << 2;         // layer 3 = Player
		AddChild(hitbox);

		CollisionShape3D collisionShape = new CollisionShape3D();
		BoxShape3D boxShape = new BoxShape3D();
		boxShape.Size = new Vector3(size, size, size);
		collisionShape.Shape = boxShape;
		hitbox.AddChild(collisionShape);

		// Replace with Animation
		MeshInstance3D visual = new MeshInstance3D();
		BoxMesh boxMesh = new BoxMesh();
		boxMesh.Size = boxShape.Size;
		visual.Mesh = boxMesh;

		StandardMaterial3D mat = new StandardMaterial3D();
		mat.AlbedoColor = new Color(1f, 0.2f, 0.2f, 0.4f);
		mat.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
		mat.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
		visual.MaterialOverride = mat;
		hitbox.AddChild(visual);
		//

		// --- Position ---
		hitbox.GlobalPosition = GlobalPosition + toPlayer * distance;

		// --- Damage: route through the one damage pipeline ---
		hitbox.AreaEntered += (Area3D area) =>
		{
			if (area.GetOwner() is PlayerManager p)
				DamagePlayer(p, damage);
			else if (area.GetParent() is PlayerManager pp)
				DamagePlayer(pp, damage);
		};

		// --- Despawn (duration in seconds) ---
		await ToSignal(GetTree().CreateTimer(duration), "timeout");
		if (IsInstanceValid(hitbox))
			hitbox.QueueFree();
	}
	
	
	public bool isMovingRight()
	{
		if (this.Velocity.X >= 0) return true;
		return false;
	}

	public void FaceDirection(float x)
	{
		if (x < -0.01f) mesh.RotationDegrees = new Vector3(0, -meshDirection, 0);
		else if (x > 0.01f) mesh.RotationDegrees = new Vector3(0, meshDirection, 0);
		
	}

	public async void DodgeProjectile(Node3D body)
	{
		//GD.Print($"SAW {body.Name}");
		if (body is not NormalArrow and not BombArrowProjectile) return;
		if (!isdodgeprojectile || isDodging) return;
		isDodging = true;

		uint savedLayer = CollisionLayer;
		SetDeferred("collision_layer", CollisionLayer & ~(uint)(1 << 3));


		//Replace with Animation
		Tween tween = CreateTween();
		tween.TweenProperty(mesh, "position:z", -1.5f, dodgeDuration * 0.3f).SetEase(Tween.EaseType.Out);
		tween.TweenInterval(dodgeDuration * 0.4f);
		tween.TweenProperty(mesh, "position:z", 0f, dodgeDuration * 0.3f).SetEase(Tween.EaseType.In);
		await ToSignal(tween, "finished");
		//------------------------

		if (!IsInstanceValid(this)) return;
		SetDeferred("collision_layer", savedLayer);
		isDodging = false;
	}
	
	}
