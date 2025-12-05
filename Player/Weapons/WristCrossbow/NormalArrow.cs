using Godot;

public partial class NormalArrow : RigidBody3D
{
	[Export] public AudioStream hitSound;
	private bool hasHit = false;

	[Export] public int damage = 10; //we should override value in attack state o-o
	private float lifetime = 2.0f;
	SceneTreeTimer timer;
	
	public override void _Ready()
	{
		//GD.Print("ArrowProjectile script is running!");
		SetContactMonitor(true);
		SetMaxContactsReported(10);
		timer = GetTree().CreateTimer(lifetime);
		timer.Timeout += DeleteProjectile;
		
		/*
		// Set collision layers to avoid hitting the player
		CollisionLayer = 2; // Arrow layer
		CollisionMask = 4; // Only hit enemies (layer 3)
		
		// Disable collision with player initially
		var timer = GetTree().CreateTimer(0.1f);
		timer.Timeout += () => {
			CollisionMask = (uint)(0xFFFFFFFF & ~1); // Hit everything except player layer (layer 1)
		};*/
	}
	
	private void DeleteProjectile()
	{
		QueueFree();
	}

	public override void _IntegrateForces(PhysicsDirectBodyState3D state)
	{
		if (hasHit) return;
		
		if (state.GetContactCount() > 0)
		{
			for (int i = 0; i < state.GetContactCount(); i++)
			{
				var collider = state.GetContactColliderObject(i);
				
				//GD.Print($"Arrow hit: {collider.GetType().Name}");
				
				if (collider is EnemyController enemy)
				{
					GD.Print("Hit enemy! Dealing damage...");
					enemy.DamagedRecieved(damage);
					OnHitSurface();
					hasHit = true;
					return;
				}
				
				// Hit something else (wall, etc.)
				OnHitSurface();
			}
		}
	}
	
	private void OnHitSurface()
	{		
		if (hitSound != null)
		{
			var audioPlayer = new AudioStreamPlayer3D();
			GetParent().AddChild(audioPlayer);
			audioPlayer.Stream = hitSound;
			audioPlayer.GlobalPosition = GlobalPosition;
			audioPlayer.Play();
			audioPlayer.Finished += audioPlayer.QueueFree;
		}

		timer.TimeLeft = 0.1f;
	}
}
