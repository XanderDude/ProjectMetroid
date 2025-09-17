using Godot;
using System;
public partial class BreakableBlock : Node3D
{
    [Export] public int health = 2;
    private StaticBody3D solidBody;
    private Area3D detectionArea;
    
    public override void _Ready()
    {
        // Get references to child nodes
        solidBody = GetNode<StaticBody3D>("StaticBody3D");
        detectionArea = GetNode<Area3D>("Area3D");
        
        // Connect the detection signal
        detectionArea.BodyEntered += OnBodyEntered;
    }
    
    private void OnBodyEntered(Node3D body)
    {
        GD.Print("Something entered: " + body.Name);
        
        if (body is BombArrowProjectile)
        {
            health -= 1;
            GD.Print("Hit by projectile! Health now: " + health);
            
            if (health <= 0)
            {
                GD.Print("Block destroyed!");
                QueueFree(); // This will remove the entire Node3D and all children
            }
        }
    }
}