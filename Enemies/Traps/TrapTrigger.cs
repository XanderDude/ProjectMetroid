using Godot;
using System;

public partial class TrapTrigger : Node3D
{
    [Export] private Area3D collider;
    [Export] private AnimationPlayer animPlayer;
    [Export] private int damage = 5;

    [Export] private float spikeCooldown = 2f;
    private float _spikeCooldown;
    private Node3D target;

    public override void _Ready()
    {
        _spikeCooldown = spikeCooldown;
        spikeCooldown = 0;
    }


    public override void _PhysicsProcess(double delta)
    {
        if (spikeCooldown > 0) spikeCooldown -= (float)delta;
        if (spikeCooldown <= 0 && target != null)
        {
            spikeCooldown = _spikeCooldown;
            animPlayer.Play("SpikeActivate");
            target.GetNode<PlayerManager>(target.GetPath()).Health -= damage;
        }

    }

    public void _on_collider_body_exited(Node3D node)
    {
        target = null;
    }

    public void _on_collider_body_entered(Node3D node)
    {
        target = node;
    }

}
