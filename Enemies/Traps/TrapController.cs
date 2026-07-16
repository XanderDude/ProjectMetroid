using Godot;
using System;

public partial class TrapController : Node3D
{
    [Export] private Area3D damageCollider;
    [Export] private Godot.AnimationPlayer animPlayer;
    [Export] private string animationName;
    [Export] private int damage = 5;
    [Export] private float _damageCooldown = .3f;
    [Export] private float _triggerCooldown = 2f;
    private float triggerCooldown = 0;
    [Export] private bool autoTrap = false; //trap doesn't need or have a trigger
    private PlayerManager player;
    private bool playerInRange = false;
    public override void _Ready()
    {
        if (autoTrap)
        {
            animPlayer?.Play(animationName);
            if (damageCollider != null) damageCollider.Monitoring = true;
        }
        if (damageCollider != null) //set damage collider values
        {
            damageCollider.GetNode<TrapDamage>(damageCollider.GetPath()).damage = damage;
            damageCollider.GetNode<TrapDamage>(damageCollider.GetPath()).damageCooldown = _damageCooldown;
        }
        AssignPlayer();
    }

    private async void AssignPlayer()
    {
        await ToSignal(GetTree().GetCurrentScene(), Node.SignalName.Ready);
        player = GetTree().GetCurrentScene().GetNode<PlayerManager>("%Player");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (autoTrap) return; // 
        if (triggerCooldown > 0) triggerCooldown -= (float)delta;
        
        if (triggerCooldown <= 0 && playerInRange) //trap is ready and player is within collider
        {
            animPlayer?.Play(animationName);
            triggerCooldown = _triggerCooldown;
            if (damageCollider == null && player.canBeDamaged) //if there is not another collider, deal damage now
            {
                player.health -= damage;
            }
                
        }

    }

    public void OnLeaveTrapTrigger(Node3D node)
    {
        playerInRange = false;
    }

    public void OnTrapTriggered(Node3D node)
    {
        playerInRange = true;
    }

}
