using Godot;

public partial class BombArrowProjectile : RigidBody3D
{
	[Export] public AudioStream explosionSound;
	[Export] public float explosionRadius = 5.0f;
	[Export] public float explosionDuration = 2.0f;
	private bool hasExploded = false;
	
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		SetContactMonitor(true);
		SetMaxContactsReported(10);
	}
	
	public override void _IntegrateForces(PhysicsDirectBodyState3D state)
	{
		if (!hasExploded && state.GetContactCount() > 0)
		{
			OnHitSurface();
		}
	}
	
	private void OnBodyEntered(Node body)
	{
		 if (!hasExploded)
	{
		// Check if we hit an enemy
		if (body is EnemyController enemy)
		{
			enemy.DamagedRecieved(25); 
		}
		
		OnHitSurface();
	}
	}
	
	private void OnHitSurface()
	{
		if (hasExploded) return;
		hasExploded = true;
		
		CreateExplosionSphere();
		
		if (explosionSound != null)
		{
			var audioPlayer = new AudioStreamPlayer3D();
			GetParent().AddChild(audioPlayer);
			audioPlayer.Stream = explosionSound;
			audioPlayer.GlobalPosition = GlobalPosition;
			audioPlayer.Play();
			audioPlayer.Finished += () => audioPlayer.QueueFree();
		}
		
		Visible = false;
		
		var timer = GetTree().CreateTimer(explosionDuration);
		timer.Timeout += () => QueueFree();
	}
	
	private void CreateExplosionSphere()
	{
		var meshInstance = new MeshInstance3D();
		var sphereMesh = new SphereMesh();
		sphereMesh.Radius = explosionRadius;
		sphereMesh.Height = explosionRadius * 2;
		meshInstance.Mesh = sphereMesh;
		
		var material = new StandardMaterial3D();
		material.AlbedoColor = new Color(1.0f, 0.5f, 0.0f, 0.7f);
		material.Emission = new Color(1.0f, 0.3f, 0.0f);
		material.EmissionIntensity = 2.0f; // Changed from EmissionEnergy
		material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
		meshInstance.MaterialOverride = material;
		
		GetParent().AddChild(meshInstance);
		meshInstance.GlobalPosition = GlobalPosition;
		
		var tween = GetTree().CreateTween();
		tween.SetParallel(true);
		
		meshInstance.Scale = Vector3.Zero;
		tween.TweenProperty(meshInstance, "scale", Vector3.One, 0.2f);
		tween.TweenProperty(meshInstance, "scale", Vector3.One * 1.5f, explosionDuration - 0.2f).SetDelay(0.2f);
		
		tween.TweenProperty(material, "albedo_color:a", 0.0f, explosionDuration);
		
		tween.Finished += () => meshInstance.QueueFree();
	}
}
