using Godot;
using System;

public partial class TrapTrigger : Node3D
{
    [Export] private Area3D collider; //the trap's hurt box. Null if trigger is also hurt box area, like for spikes
    [Export] private AnimationPlayer animPlayer;
    [Export] private string animationName;
    [Export] private int damage = 5;
    [Export] private float trapCooldown = 2f;
    private float _trapCooldown;
    [Export] private bool constantAnimation = false;
    private Node3D target = null;

    public override void _Ready()
    {

        _trapCooldown = trapCooldown;
        trapCooldown = 0;
        if (constantAnimation) animPlayer.Play(animationName);
        else collider?.SetDeferred("monitoring",false);//only disable collider if null and not constant, ie spikes. Enable collider in animation
    }


    public override void _PhysicsProcess(double delta)
    {
        if (trapCooldown > 0) trapCooldown -= (float)delta;
        if (trapCooldown <= 0 && target != null) //trap is ready and there is a target
        {
            animPlayer.Play(animationName);
            trapCooldown = _trapCooldown;
            if (collider == null) //if there is not another collider, deal damage now
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

    public void OnTrapCollide(Node3D node)
    {
        GD.Print("Trap Collided");
        if (!constantAnimation && collider != null) //triggered trap but uses a seperate collider.
        {
            node.GetNode<PlayerManager>(node.GetPath()).Health -= damage;
            collider?.SetDeferred("monitoring", false);
        }
        else if (trapCooldown <= 0) //constant trap or triggered trap without seperate collider
        {
            trapCooldown = _trapCooldown;
            node.GetNode<PlayerManager>(node.GetPath()).Health -= damage;
            collider?.SetDeferred("monitoring", false);
        }            
        
    }

}
