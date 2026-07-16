using System;
using System.Collections.Generic;
using Godot;

public partial class NormalArrow : RigidBody3D
{
    [Export] public bool isBombArrow = false;

    // Shared
    [Export] public AudioStream hitSound;
    [Export] public int damage = 10;
    private bool hasHit = false;
    private float lifetime = 2.0f;
    private SceneTreeTimer timer;
    private List<Node3D> targetsDamaged = new();

    // Bomb-only
    [Export] public AudioStream explosionSound;
    [Export] private PackedScene explosionVFX;
    [Export] public float explosionRadius = 5.0f;
    [Export] public float explosionDuration = 2.0f;
    private Area3D explosionCollider;

    public override void _Ready()
    {
        SetContactMonitor(true);
        SetMaxContactsReported(10);
        timer = GetTree().CreateTimer(lifetime);
        timer.Timeout += DeleteProjectile;

        if (isBombArrow)
        {
            explosionCollider = GetNode<Area3D>("Area3D");
            explosionCollider.Monitoring = false;
            explosionCollider.BodyEntered += OnExplosionBodyEntered;
        }
    }

    private void DeleteProjectile() => QueueFree();

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        if (hasHit || state.GetContactCount() == 0) return;

        if (!isBombArrow)
        {
            for (int i = 0; i < state.GetContactCount(); i++)
            {
                var collider = state.GetContactColliderObject(i);
                if (collider is Enemy enemy && !targetsDamaged.Contains(enemy))
                {
                    enemy.DamagedReceived(damage);
					
                }
            }
        }

        OnHitSurface();
        Freeze = true;
    }

    private void OnHitSurface()
    {
        hasHit = true;

        if (isBombArrow)
        {
            explosionCollider.Monitoring = true;

            if (explosionVFX != null)
            {
                var explosion = explosionVFX.Instantiate() as Node3D;
                GetTree().CurrentScene.AddChild(explosion);
                explosion.GlobalPosition = GlobalPosition;
            }

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
        else
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

    private void OnExplosionBodyEntered(Node3D body)
    {
        if (body is Enemy enemy && !targetsDamaged.Contains(enemy))
        {
            enemy.DamagedReceived(damage);
            targetsDamaged.Add(enemy);
        }
    }
}