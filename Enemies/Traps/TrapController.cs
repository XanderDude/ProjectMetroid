using Godot;
using System;

public partial class TrapController : Node3D
{
    [Export] private Area3D damageCollider;
    [Export] private AnimationPlayer animPlayer;
    [Export] private string animationName;
    [Export] private int damage = 5;
    [Export] private float _damageCooldown = .3f;
    [Export] private float _triggerCooldown = 2f;
    private float triggerCooldown = 0;
    [Export] private bool autoTrap = false; //trap doesn't need or have a trigger
    private Node3D target = null;

    public override void _Ready()
    {
        if (autoTrap)
        {
            animPlayer.Play(animationName);
            if (damageCollider != null) damageCollider.Monitoring = true;
        }
        if (damageCollider != null) //set damage collider values
        {
            damageCollider.GetNode<TrapDamage>(damageCollider.GetPath()).damage = damage;
            damageCollider.GetNode<TrapDamage>(damageCollider.GetPath()).damageCooldown = _damageCooldown;
        }
    }


    public override void _PhysicsProcess(double delta)
    {
        if (autoTrap) return; // 
        if (triggerCooldown > 0) triggerCooldown -= (float)delta;
        
        if (triggerCooldown <= 0 && target != null) //trap is ready and there is a target
        {
            animPlayer.Play(animationName);
            triggerCooldown = _triggerCooldown;
            if (damageCollider == null) //if there is not another collider, deal damage now
            {
                target.GetNode<PlayerManager>(target.GetPath()).Health -= damage;
            }
                
        }

    }

    public void OnLeaveTrapTrigger(Node3D node)
    {
        target = null;
    }

    public void OnTrapTriggered(Node3D node)
    {
        target = node;
    }

}
