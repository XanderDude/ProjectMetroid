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
		SetContactMonitor(true);
		SetMaxContactsReported(10);
		timer = GetTree().CreateTimer(lifetime);
		timer.Timeout += DeleteProjectile;
	}
	
	private void DeleteProjectile()
	{
		QueueFree();
	}

	public override void _IntegrateForces(PhysicsDirectBodyState3D state)
	{
		if (!hasHit && state.GetContactCount() > 0)
		{
			for (int i = 0; i < state.GetContactCount(); i++)
			{
				var collider = state.GetContactColliderObject(i);
				
				if (collider is EnemyController enemy)
				{
					enemy.DamagedRecieved(damage);
				}
			}

			OnHitSurface();
			Freeze = true;
		}
	}
	
	private void OnHitSurface()
	{		
		hasHit = true;

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
