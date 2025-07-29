using Godot;
using System;

public partial class TrapDamage : Area3D
{
    public int damage = 5;
    public float damageCooldown = 2f;
    private float _damageCooldown;
    private PlayerManager target = null;

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
        if (damageCooldown <= 0 && target != null && target.canBeDamaged) //trap is ready and there is a target
        {
            damageCooldown = _damageCooldown;
            target.Health -= damage;
        }
    }

    public void OnLeaveCollider(Node3D node)
    {
        target = null;
    }

    public void OnCollide(Node3D node)
    {
        target = node.GetNode<PlayerManager>(node.GetPath());
        if (damageCooldown <= 0 && target.canBeDamaged) //damaging collider 
        {
            damageCooldown = _damageCooldown;
            node.GetNode<PlayerManager>(node.GetPath()).Health -= damage;
        }
    }
}
