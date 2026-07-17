using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class BombArrowProjectile : RigidBody3D
{
	[Export] public AudioStream explosionSound;
	[Export] private PackedScene explosionVFX;
	private Area3D explosionCollider;
	[Export] public float explosionRadius = 5.0f;
	[Export] public float explosionDuration = 2.0f;
	[Export] private int damage = 30;
	private bool hasExploded = false;
	private float lifetime = 3.0f;
	SceneTreeTimer timer;
	public override void _Ready()
	{
		SetContactMonitor(true);
		SetMaxContactsReported(10);
		timer = GetTree().CreateTimer(lifetime);
		timer.Timeout += DeleteProjectile;
		explosionCollider = GetNode<Area3D>("Area3D");
		explosionCollider.BodyEntered += EnemyCollisionEntered;//may be redundant
	}

	private void DeleteProjectile()
	{
		QueueFree();
	}
	
	public override void _IntegrateForces(PhysicsDirectBodyState3D state)
	{
		if (!hasExploded && state.GetContactCount() > 0)
		{
			for (int i = 0; i < state.GetContactCount(); i++)
			{
				var collider = state.GetContactColliderObject(i);
				
				if (collider is Enemy enemy)
				{
					//enemy.DamagedRecieved(damage);
				}
			}
			
			OnHitSurface();
			Freeze = true;
		}
	}
	
	private void OnHitSurface()
	{		
		hasExploded = true;
		explosionCollider.ProcessMode = ProcessModeEnum.Inherit;

		var explosion = explosionVFX.Instantiate() as Node3D;
		GetTree().CurrentScene.AddChild(explosion);
		explosion.GlobalPosition = GlobalPosition;

		//CreateExplosionSphere();

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
		timer.TimeLeft = explosionDuration;
	}

	private void EnemyCollisionEntered(Node3D body)
	{
		if (body is Enemy enemy)
		{
			//GD.Print("Bomb dealing damage via body entered");
			enemy.DamagedReceived(damage);

		}
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
