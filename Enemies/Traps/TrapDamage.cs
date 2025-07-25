using Godot;
using System;

public partial class TrapDamage : Area3D
{
    public int damage = 5;
    public float damageCooldown = 2f;
    private float _damageCooldown;
    private Node3D target = null;

    private Area3D collider;

    public override void _Ready()
    {
        _damageCooldown = damageCooldown;
        damageCooldown = 0;
        collider = GetNode<Area3D>(GetPath());
        collider.Monitoring = true;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (damageCooldown > 0) damageCooldown -= (float)delta;
        if (damageCooldown <= 0 && target != null) //trap is ready and there is a target
        {
            damageCooldown = _damageCooldown;
            target.GetNode<PlayerManager>(target.GetPath()).Health -= damage;
        }
    }

    public void OnLeaveTrapCollider(Node3D node)
    {
        target = null;
    }
    public void OnTrapCollide(Node3D node)
    {
        target = node;
        if (damageCooldown <= 0) //damaging collider 
        {
            damageCooldown = _damageCooldown;
            node.GetNode<PlayerManager>(node.GetPath()).Health -= damage;
        }
    }
}
